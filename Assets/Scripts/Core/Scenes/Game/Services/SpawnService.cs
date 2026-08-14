using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Project;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface ISpawnService : IDisposable
    {
        UniTask<IPlayerSpaceship> SpawnPlayer();
        UniTask<List<IEnemySpaceship>> SpawnEnemies(WaveConfigDTO waveConfig);
        ProjectileBehaviourComponent SpawnProjectile(ProjectileBehaviourComponent prefab, Vector3 localPosition, Vector3 direction, AttackSourceDTO source, string shooterTag);
        PowerupBehaviourComponent SpawnPowerup(PowerupConfigSO config, Vector3 localPosition);
        ItemPickupBehaviourComponent SpawnItemPickup(ItemRarityConfigSO rarityConfig, InventoryItemEntry item, Vector3 localPosition);
        VFXBehaviourComponent SpawnVFX(VFXBehaviourComponent prefab, Vector3 localPosition);
        void Despawn<T>(T instance) where T : MonoBehaviour, IPoolableObject;
    }

    /// <summary>
    /// Creates all runtime objects through the object pool, and tracks transient ones for cleanup on
    /// game end.
    /// </summary>
    public class SpawnService : ISpawnService, IGameEndListener
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

        public void Dispose()
        {
            _objectPooling.ClearAll();
        }

        public UniTask GameEnd()
        {
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
            return spawnedPlayer;
        }

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
                var wavePosition = new Vector3(formation.GridPosition.x, 0, formation.GridPosition.y);
                var spawnedEnemy = Spawn(enemyPrefab, enemyPrefab.transform.localPosition + wavePosition, enemyPrefab.transform.localRotation);

                if (spawnedEnemy == null)
                {
                    continue;
                }

                spawnedEnemies.Add(spawnedEnemy);
            }

            return spawnedEnemies;
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
            Vector3 localPosition, Vector3 direction, AttackSourceDTO source, string shooterTag)
        {
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
