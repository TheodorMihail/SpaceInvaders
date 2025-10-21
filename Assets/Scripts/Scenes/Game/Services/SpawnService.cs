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
        T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : MonoBehaviour, IPoolableObject;
        UniTask<List<T>> Spawn<T>(WaveConfigDTO waveConfig) where T : MonoBehaviour, IPoolableObject;
        void Despawn<T>(T instance) where T : MonoBehaviour, IPoolableObject;
    }

    public class SpawnService : ISpawnService
    {
        [Inject] private readonly IAddressablesManager _addressablesManager;
        [Inject] private readonly IErrorManager _errorManager;
        [Inject] private readonly IObjectPooling _objectPooling;
        [Inject] private readonly Transform _container;

        private string PrefabsPath(string enemyType) => $"Prefabs/{enemyType}";

        public void Dispose()
        {
            _objectPooling.ClearAll();
        }

        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : MonoBehaviour, IPoolableObject
        {
            var instance = _objectPooling.Get(prefab, _container);

            if(instance == null)
            {
                _errorManager.LogError<AddressablesManager>($"Exception instanciating prefab: {prefab.name}");
                return null;
            }
            
            instance.transform.SetLocalPositionAndRotation(position, rotation);
            return instance;
        }

        public async UniTask<List<T>> Spawn<T>(WaveConfigDTO waveConfig) where T : MonoBehaviour, IPoolableObject
        {
            var spawnedEnemies = new List<T>();

            foreach (var formation in waveConfig.WavesFormation)
            {
                var prefab = await _addressablesManager.LoadPrefab(PrefabsPath(formation.EnemyType));

                if (prefab == null || !prefab.TryGetComponent<T>(out var enemyPrefab))
                {
                    continue;
                }

                var wavePosition = new Vector3(formation.Position.x, 0, formation.Position.y);
                T spawnedEnemy = Spawn(enemyPrefab, prefab.transform.localPosition + wavePosition, prefab.transform.localRotation);

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
    }
}
