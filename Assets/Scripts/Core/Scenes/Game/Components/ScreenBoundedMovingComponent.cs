using BaseArchitecture.Core;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public abstract class ScreenBoundedMovingComponent : MonoBehaviour, IPoolableObject
    {
        [Inject] protected ICameraManager _cameraManager;
        [Inject] protected ISpawnService _spawnService;

        [SerializeField] protected Renderer _renderer;

        protected Vector3 _direction;
        protected float _speed;

        private Vector3 _minBounds;
        private Vector3 _maxBounds;

        public virtual void OnSpawned()
        {
            (_minBounds, _maxBounds) = _cameraManager.GetScreenBounds(_renderer, ScreenRegionTypes.Full, buffer: 2f);
        }

        public virtual void OnDespawned()
        {
        }

        protected virtual void Update()
        {
            transform.position += _direction * (_speed * Time.deltaTime);

            if (IsOutOfBounds())
            {
                Despawn();
            }
        }

        protected bool IsOutOfBounds()
        {
            Vector3 pos = transform.position;
            return pos.x < _minBounds.x || pos.x > _maxBounds.x ||
                   pos.z < _minBounds.z || pos.z > _maxBounds.z;
        }

        protected virtual void Despawn()
        {
            _spawnService.Despawn(this);
        }
    }
}
