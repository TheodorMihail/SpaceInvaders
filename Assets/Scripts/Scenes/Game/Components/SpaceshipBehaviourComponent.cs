using System;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    public class SpaceshipBehaviourComponent : MonoBehaviour, IPoolableObject
    {
        [SerializeField] private SpaceshipConfigSO _shipConfig;
        protected int _currentHealth;

        public event Action<SpaceshipBehaviourComponent> OnDestroyed;

        public virtual void OnSpawned()
        {
            _currentHealth = _shipConfig.Health;
        }

        public virtual void OnDespawned()
        {
        }

        public virtual void Shoot()
        {
            // Implement shooting logic here
        }

        private void TakeDamage(int damage)
        {
            _currentHealth = Math.Clamp(_currentHealth - damage, 0, Int32.MaxValue);
            if(_currentHealth == 0)
            {
                OnDestroyed?.Invoke(this);
            }
        }

        public virtual void Move(Vector3 direction, Vector3 minBounds, Vector3 maxBounds)
        {
            direction.Normalize();
            
            Vector3 movement = direction * (_shipConfig.MoveSpeed * Time.deltaTime);
            Vector3 newPosition = transform.position + movement;

            newPosition.x = Mathf.Clamp(newPosition.x, minBounds.x, maxBounds.x);
            newPosition.y = transform.position.y;
            newPosition.z = Mathf.Clamp(newPosition.z, minBounds.z, maxBounds.z);

            transform.position = newPosition;
        }
    }
}
