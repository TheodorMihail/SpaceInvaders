using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface ISpawnService : IDisposable
    {
        UniTask<PlayerSpaceshipBehaviourComponent> SpawnPlayer();
        UniTask<List<EnemySpaceshipBehaviourComponent>> SpawnEnemies(WaveConfigDTO waveConfig);
        ProjectileBehaviourComponent SpawnProjectile(ProjectileBehaviourComponent prefab, Vector3 position, Vector3 direction, int damage, float speed);
        void Despawn<T>(T instance) where T : MonoBehaviour, IPoolableObject;
    }

    public class SpawnService : ISpawnService
    {
        [Inject] private readonly IRepositoryManager _repositoryManager;
        [Inject] private readonly IAddressablesManager _addressablesManager;
        [Inject] private readonly IErrorManager _errorManager;
        [Inject] private readonly IObjectPooling _objectPooling;
        [Inject] private readonly Transform _container;

        public void Dispose()
        {
            _objectPooling.ClearAll();
        }

        public async UniTask<PlayerSpaceshipBehaviourComponent> SpawnPlayer()
        {
            var playerConfig = _repositoryManager.GetPlayerConfig(PlayerTypes.Player1);
            var prefabPath = playerConfig.SpaceshipPrefabAddress;

            var prefab = await LoadPrefabAsync<PlayerSpaceshipBehaviourComponent>(prefabPath);
            var spawnedPlayer = Spawn(prefab, prefab.transform.localPosition, prefab.transform.localRotation);
            return spawnedPlayer;
        }

        public async UniTask<List<EnemySpaceshipBehaviourComponent>> SpawnEnemies(WaveConfigDTO waveConfig)
        {
            var spawnedEnemies = new List<EnemySpaceshipBehaviourComponent>();

            foreach (var formation in waveConfig.WavesFormation)
            {
                var enemyConfig = _repositoryManager.GetEnemyConfig(formation.EnemyType);
                var prefabPath = enemyConfig.SpaceshipPrefabAddress;

                var enemyPrefab = await LoadPrefabAsync<EnemySpaceshipBehaviourComponent>(prefabPath);
                var wavePosition = new Vector3(formation.Position.x, 0, formation.Position.y);
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
            _objectPooling.Return(instance);
        }

        public ProjectileBehaviourComponent SpawnProjectile(
            ProjectileBehaviourComponent prefab,
            Vector3 position,
            Vector3 direction,
            int damage,
            float speed)
        {
            var projectile = Spawn(prefab, position, Quaternion.identity);

            if (projectile == null)
            {
                return null;
            }

            projectile.Initialize(damage, speed, direction);

            return projectile;
        }

        private T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : MonoBehaviour, IPoolableObject
        {
            var instance = _objectPooling.Get(prefab, _container);

            if (instance == null)
            {
                _errorManager.LogError<AddressablesManager>($"Exception instanciating prefab: {prefab.name}");
                return null;
            }

            instance.transform.SetLocalPositionAndRotation(position, rotation);
            return instance;
        }

        private async UniTask<T> LoadPrefabAsync<T>(string prefabPath)
        {
            var prefab = await _addressablesManager.LoadPrefab(prefabPath);
            if (prefab == default || !prefab.TryGetComponent<T>(out var component))
            {
                _errorManager.LogError<SpawnService>($"Exception instantiating prefab: {prefab.name}");
                return default;
            }

            return component;
        }
    }
}
