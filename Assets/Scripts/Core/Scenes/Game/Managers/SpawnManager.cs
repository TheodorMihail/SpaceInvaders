using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Project;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface ISpawnManager : IDisposable
    {
        UniTask<IPlayerSpaceship> SpawnPlayer();
        UniTask<List<IEnemySpaceship>> SpawnEnemies(WaveConfigDTO waveConfig);
        UniTask<IEnemySpaceship> SpawnEnemy(EnemyTypes enemyType, Vector3 localPosition);
        BaseHazardBehaviourComponent SpawnHazard(HazardConfigSO config, Vector3 direction, float entryRatio);
        ProjectileBehaviourComponent SpawnProjectile(ProjectileBehaviourComponent prefab, Vector3 muzzleWorldPosition, Vector3 direction, AttackSourceDTO source, string shooterTag);
        PowerupBehaviourComponent SpawnPowerup(PowerupConfigSO config, Vector3 localPosition);
        ItemPickupBehaviourComponent SpawnItemPickup(ItemRarityConfigSO rarityConfig, InventoryItemEntry item, Vector3 localPosition);
        VFXBehaviourComponent SpawnVFX(VFXBehaviourComponent prefab, Vector3 localPosition);
        void Despawn<T>(T instance) where T : MonoBehaviour, IPoolableObject;

        /// <summary>Converts a world point into the container space spawn positions use. The container
        /// is offset from the world, so the two never match.</summary>
        Vector3 GetContainerLocalPosition(Vector3 worldPosition);

        /// <summary>Converts a spawn position back into world space.</summary>
        Vector3 GetContainerWorldPosition(Vector3 localPosition);
    }

    /// <summary>
    /// Creates all runtime objects through the object pool, and tracks transient ones for cleanup on
    /// game end.
    /// </summary>
    public class SpawnManager : ISpawnManager, IGameInitializeListener, IGameEndListener
    {
        [Inject] private readonly IShipsRepository _shipsRepository;
        [Inject] private readonly IItemsRepository _itemsRepository;
        [Inject] private readonly IPowerupsRepository _powerupsRepository;
        [Inject] private readonly IAddressablesManager _addressablesManager;
        [Inject] private readonly IObjectPooling _objectPooling;
        /// <summary>Everything spawns as a child of this, so all spawn positions are local to it.</summary>
        [Inject] private readonly Transform _container;

        /// <summary>Transients despawned on game end. Types not registered here are never cleaned up.</summary>
        private readonly HashSet<ScreenBoundedMovingComponent> _activeObjects = new();

        /// <summary>Whether a run is live. Only awaited spawns check it, so listener order does not
        /// matter.</summary>
        private bool _isRunActive;

        public void Dispose()
        {
            _objectPooling.ClearAll();
        }

        public UniTask GameInitialize()
        {
            _isRunActive = true;
            return UniTask.CompletedTask;
        }

        public UniTask GameEnd()
        {
            _isRunActive = false;

            var pendingDespawns = new List<ScreenBoundedMovingComponent>(_activeObjects);
            foreach (var obj in pendingDespawns)
            {
                Despawn(obj);
            }

            return UniTask.CompletedTask;
        }

        public async UniTask<IPlayerSpaceship> SpawnPlayer()
        {
            if (!_shipsRepository.TryGetPlayerConfig(PlayerTypes.Player1, out var playerConfig))
            {
                return null;
            }

            var prefabPath = playerConfig.SpaceshipPrefabAddress;

            var prefab = await LoadPrefabAsync<PlayerSpaceshipBehaviourComponent>(prefabPath);
            var spawnedPlayer = Spawn(prefab, prefab.transform.localPosition, prefab.transform.localRotation);

            if (spawnedPlayer == null)
            {
                return null;
            }

            spawnedPlayer.Initialize(playerConfig);
            return spawnedPlayer;
        }

        /// <summary>Stops spawning once the run ends and returns what was spawned so far. A prefab
        /// load spans frames, and a ship built after the run ends resolves against a container that no
        /// longer has the game bindings.</summary>
        public async UniTask<List<IEnemySpaceship>> SpawnEnemies(WaveConfigDTO waveConfig)
        {
            var spawnedEnemies = new List<IEnemySpaceship>();

            foreach (var formation in waveConfig.WavesFormation)
            {
                if (!_shipsRepository.TryGetEnemyConfig(formation.EnemyType, out var enemyConfig))
                {
                    continue;
                }

                var prefabPath = enemyConfig.SpaceshipPrefabAddress;

                var enemyPrefab = await LoadPrefabAsync<EnemySpaceshipBehaviourComponent>(prefabPath);

                if (!_isRunActive)
                {
                    return spawnedEnemies;
                }

                var wavePosition = new Vector3(formation.GridPosition.x, 0, formation.GridPosition.y);
                var spawnedEnemy = Spawn(enemyPrefab, enemyPrefab.transform.localPosition + wavePosition, enemyPrefab.transform.localRotation);

                if (spawnedEnemy == null)
                {
                    continue;
                }

                spawnedEnemy.Initialize(enemyConfig);
                spawnedEnemies.Add(spawnedEnemy);
            }

            return spawnedEnemies;
        }

        /// <summary>One ship at a spot that is already known, for reinforcements that appear in place
        /// rather than flying a formation in.</summary>
        public async UniTask<IEnemySpaceship> SpawnEnemy(EnemyTypes enemyType, Vector3 localPosition)
        {
            if (!_shipsRepository.TryGetEnemyConfig(enemyType, out var enemyConfig))
            {
                return null;
            }

            var enemyPrefab = await LoadPrefabAsync<EnemySpaceshipBehaviourComponent>(enemyConfig.SpaceshipPrefabAddress);

            if (!_isRunActive || enemyPrefab == null)
            {
                return null;
            }

            // The caller supplies the position, the prefab supplies the plane it flies on.
            localPosition.y = enemyPrefab.transform.localPosition.y;

            var spawnedEnemy = Spawn(enemyPrefab, localPosition, enemyPrefab.transform.localRotation);

            if (spawnedEnemy == null)
            {
                return null;
            }

            spawnedEnemy.Initialize(enemyConfig);
            return spawnedEnemy;
        }

        /// <summary>Spawned on the prefab's own plane and left to place itself: where it enters is
        /// picked by the caller, but only the hazard knows its own extents.</summary>
        public BaseHazardBehaviourComponent SpawnHazard(HazardConfigSO config, Vector3 direction, float entryRatio)
        {
            var hazard = Spawn(config.HazardPrefab, config.HazardPrefab.transform.localPosition, Quaternion.identity);

            if (hazard == null)
            {
                return null;
            }

            hazard.Initialize(config, direction, entryRatio);
            _activeObjects.Add(hazard);

            return hazard;
        }

        public Vector3 GetContainerLocalPosition(Vector3 worldPosition)
        {
            return _container.InverseTransformPoint(worldPosition);
        }

        public Vector3 GetContainerWorldPosition(Vector3 localPosition)
        {
            return _container.TransformPoint(localPosition);
        }

        public void Despawn<T>(T instance) where T : MonoBehaviour, IPoolableObject
        {
            if (instance is ScreenBoundedMovingComponent transient)
            {
                _activeObjects.Remove(transient);
            }

            _objectPooling.Return(instance);
        }

        /// <summary>The shooter's tag travels with the shot: projectiles are pooled and shared between
        /// ships, so the team cannot be authored on the prefab.</summary>
        public ProjectileBehaviourComponent SpawnProjectile(ProjectileBehaviourComponent prefab,
            Vector3 muzzleWorldPosition, Vector3 direction, AttackSourceDTO source, string shooterTag)
        {
            // A muzzle sits under the ship, so its position is world and has to be converted.
            Vector3 localPosition = GetContainerLocalPosition(muzzleWorldPosition);

            var projectile = Spawn(prefab, localPosition, Quaternion.identity);

            if (projectile == null)
            {
                return null;
            }

            projectile.Initialize(source, direction, shooterTag);
            _activeObjects.Add(projectile);

            return projectile;
        }

        /// <summary>Every powerup shares one pickup prefab, told apart by the config's icon.</summary>
        public PowerupBehaviourComponent SpawnPowerup(PowerupConfigSO config, Vector3 localPosition)
        {
            var pickup = Spawn(_powerupsRepository.GetPowerupPickupPrefab(), localPosition, Quaternion.identity);

            if (pickup == null)
            {
                return null;
            }

            pickup.Initialize(config.PowerupType, config.Icon);
            _activeObjects.Add(pickup);

            return pickup;
        }

        /// <summary>Every rarity shares one pickup prefab, told apart by the rarity's icon.</summary>
        public ItemPickupBehaviourComponent SpawnItemPickup(ItemRarityConfigSO rarityConfig, InventoryItemEntry item, Vector3 localPosition)
        {
            var pickup = Spawn(_itemsRepository.GetItemPickupPrefab(), localPosition, Quaternion.identity);

            if (pickup == null)
            {
                return null;
            }

            pickup.Initialize(item, rarityConfig.Icon);
            _activeObjects.Add(pickup);

            return pickup;
        }

        public VFXBehaviourComponent SpawnVFX(VFXBehaviourComponent prefab, Vector3 localPosition)
        {
            return Spawn(prefab, localPosition, Quaternion.identity);
        }

        private T Spawn<T>(T prefab, Vector3 localPosition, Quaternion rotation) where T : MonoBehaviour, IPoolableObject
        {
            var instance = _objectPooling.Get(prefab, _container);

            if (instance == null)
            {
                this.LogError($"Exception instanciating prefab: {prefab.name}");
                return null;
            }

            instance.transform.SetLocalPositionAndRotation(localPosition, rotation);
            return instance;
        }

        private async UniTask<T> LoadPrefabAsync<T>(string prefabPath)
        {
            var prefab = await _addressablesManager.LoadPrefab(prefabPath);
            if (prefab == default || !prefab.TryGetComponent<T>(out var component))
            {
                this.LogError($"Exception instantiating prefab: {prefab.name}");
                return default;
            }

            return component;
        }
    }
}
