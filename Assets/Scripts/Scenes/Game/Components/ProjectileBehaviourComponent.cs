using System;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    public class ProjectileBehaviourComponent : MonoBehaviour, IPoolableObject
    {
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

            CalculateScreenBounds();
        }

        public void OnSpawned()
        {
        }

        public void OnDespawned()
        {
            OnProjectileDestroyed = null;
        }

        private void Update()
        {
            // Move forward
            transform.position += _direction * (_speed * Time.deltaTime);

            // Check if projectile left screen bounds
            if (IsOutOfBounds())
            {
                TriggerDestroy();
            }
        }

        private void OnTriggerEnter(Collider other)
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

        private void CalculateScreenBounds()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            // Calculate screen bounds with buffer zone
            Vector3 screenBottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, transform.position.y));
            Vector3 screenTopRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, transform.position.y));

            // Add buffer beyond screen edges
            float buffer = 2f;
            _minBounds = new Vector3(screenBottomLeft.x - buffer, transform.position.y, screenBottomLeft.z - buffer);
            _maxBounds = new Vector3(screenTopRight.x + buffer, transform.position.y, screenTopRight.z + buffer);
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
