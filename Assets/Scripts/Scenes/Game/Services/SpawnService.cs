using System;
using BaseArchitecture.Core;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface ISpawnService : IDisposable
    {
        T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : MonoBehaviour, IPoolableObject;
        void Despawn<T>(T instance) where T : MonoBehaviour, IPoolableObject;
    }

    public class SpawnService : ISpawnService
    {
        [Inject] private readonly IObjectPooling _objectPooling;
        [Inject] private readonly Transform _container;

        public void Dispose()
        {
            _objectPooling.ClearAll();
        }

        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : MonoBehaviour, IPoolableObject
        {
            var instance = _objectPooling.Get(prefab, _container);
            instance.transform.SetLocalPositionAndRotation(position, rotation);
            return instance;
        }

        public void Despawn<T>(T instance) where T : MonoBehaviour, IPoolableObject
        {
            _objectPooling.Return(instance);
        }
    }
}
