using BaseArchitecture.Core;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Base class for objects that move in a fixed direction and despawn when leaving the screen.
    /// Subclasses set _direction and _speed on initialization.
    /// </summary>
    public abstract class ScreenBoundedMovingComponent : MonoBehaviour, IPoolableObject
    {
        [Inject] protected ICameraManager _cameraManager;
        [Inject] protected ISpawnManager _spawnManager;

        [SerializeField] protected Renderer _renderer;

        protected Vector3 _direction;
        protected float _speed;

        private Vector3 _minBounds;
        private Vector3 _maxBounds;

        public virtual void OnSpawned()
        {
            (_minBounds, _maxBounds) = _cameraManager.GetPlayfieldBounds(_renderer, buffer: 2f);
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
            _spawnManager.Despawn(this);
        }
    }
}
