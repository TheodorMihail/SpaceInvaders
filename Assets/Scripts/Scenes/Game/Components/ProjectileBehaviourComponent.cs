using System;
using BaseArchitecture.Core;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class ProjectileBehaviourComponent : MonoBehaviour, IPoolableObject
    {
        [Inject] private ICameraManager _cameraManager;

        [SerializeField] private CollisionDetectionComponent _collisionDetection;
        [SerializeField] private Renderer _renderer;

        private int _damage;
        private float _speed;
        private Vector3 _direction;
        private Vector3 _minBounds;
        private Vector3 _maxBounds;

        public event Action<ProjectileBehaviourComponent> OnProjectileDestroyed;

        public void Initialize(int damage, float speed, Vector3 direction)
        {
            _damage = damage;
            _speed = speed;
            _direction = direction.normalized;
        }

        public void OnSpawned()
        {
            _collisionDetection.OnTriggerEntered += HandleTriggerEnter;
            (_minBounds, _maxBounds) = _cameraManager.GetScreenBounds(_renderer, ScreenRegionType.Full, buffer: 2f);
        }

        public void OnDespawned()
        {
            _collisionDetection.OnTriggerEntered -= HandleTriggerEnter;
            OnProjectileDestroyed = null;
        }

        private void Update()
        {
            transform.position += _direction * (_speed * Time.deltaTime);

            // Check if projectile left screen bounds
            if (IsOutOfBounds())
            {
                TriggerDestroy();
            }
        }

        private void HandleTriggerEnter(Collider other)
        {
            // Check if hit something with a different tag
            if (other.CompareTag(gameObject.tag))
            {
                return; // Same team, ignore
            }

            // Try to get spaceship component and apply damage
            if (other.TryGetComponent<SpaceshipBehaviourComponent>(out var target))
            {
                target.TakeDamage(_damage);
            }

            // Trigger destroy event
            TriggerDestroy();
        }

        private bool IsOutOfBounds()
        {
            Vector3 pos = transform.position;
            return pos.x < _minBounds.x || pos.x > _maxBounds.x ||
                   pos.z < _minBounds.z || pos.z > _maxBounds.z;
        }

        private void TriggerDestroy()
        {
            OnProjectileDestroyed?.Invoke(this);
        }
    }
}
