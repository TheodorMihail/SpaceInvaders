using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class SpaceshipBehaviourComponent : MonoBehaviour, IPoolableObject
    {
        [Inject] protected ISpawnService _spawnService;

        [SerializeField] private SpaceshipConfigSO _shipConfig;
        [SerializeField] protected Renderer _renderer;
        [SerializeField] protected Vector3 _projectileOffset;

        protected int _currentHealth;
        private float _lastShotTime;
        private readonly List<ProjectileBehaviourComponent> _activeProjectiles = new();

        public event Action<SpaceshipBehaviourComponent> OnDestroyed;

        public virtual void OnSpawned()
        {
            _currentHealth = _shipConfig.Health;
            _lastShotTime = 0f;
        }

        public virtual void OnDespawned()
        {
            // Despawn all active projectiles
            foreach (var projectile in _activeProjectiles)
            {
                if (projectile != null)
                {
                    projectile.OnProjectileDestroyed -= OnProjectileDestroyed;
                    _spawnService.Despawn(projectile);
                }
            }
            _activeProjectiles.Clear();
        }

        public virtual void Shoot()
        {
            // Check fire rate cooldown
            if (Time.time - _lastShotTime < _shipConfig.FireRate)
            {
                return;
            }

            _lastShotTime = Time.time;

            // Calculate spawn position and direction
            Vector3 spawnPosition = transform.localPosition + _projectileOffset;
            Vector3 direction = GetProjectileDirection();

            // Spawn projectile
            var projectile = _spawnService.SpawnProjectile(
                _shipConfig.ProjectilePrefab,
                spawnPosition,
                direction,
                _shipConfig.ProjectileDamage,
                _shipConfig.ProjectileSpeed
            );

            if (projectile != null)
            {
                // Track projectile
                _activeProjectiles.Add(projectile);
                projectile.OnProjectileDestroyed += OnProjectileDestroyed;
            }
        }

        protected virtual Vector3 GetProjectileDirection()
        {
            return Vector3.forward;
        }

        private void OnProjectileDestroyed(ProjectileBehaviourComponent projectile)
        {
            projectile.OnProjectileDestroyed -= OnProjectileDestroyed;
            _activeProjectiles.Remove(projectile);
            _spawnService.Despawn(projectile);
        }

        public void TakeDamage(int damage)
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
