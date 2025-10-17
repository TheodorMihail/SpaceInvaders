using System;
using BaseArchitecture.Core;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface ISpawnService : IInitializable, IDisposable
    {
        GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation);
        void Despawn(GameObject obj);
    }

    public class SpawnService : ISpawnService
    {
        [Inject] private readonly ICustomFactory _factory;
        [Inject] private readonly DiContainer _diContainer;

        private Transform _container;

        public void Initialize()
        {
            _container = _diContainer.TryResolveId<Transform>(GameplayState.GameplayContainerID);
        }

        public void Dispose()
        {
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject obj = _factory.CreateFromPrefab(prefab, _container);
            obj.transform.SetLocalPositionAndRotation(position, rotation);
            return obj;
        }

        public void Despawn(GameObject obj)
        {
            // TODO: Return to pool
        }
    }
}
