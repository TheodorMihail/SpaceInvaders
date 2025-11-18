using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface ISpaceship : IPoolableObject
    {
        void Move(Vector3 direction, Vector3 minBounds, Vector3 maxBounds);
        void Shoot();
        void TakeDamage(int damage);
        int CurrentHealth { get;}
        string SpaceshipID { get; }
        event Action<ISpaceship> OnDestroyed;
    }
    
    public abstract class BaseSpaceshipBehaviourComponent : MonoBehaviour, ISpaceship
    {
        [Inject] protected ISpawnService _spawnService;
        [SerializeField] protected Renderer _renderer;
        [SerializeField] protected Vector3 _projectileOffset;
        [SerializeField] protected HealthBarComponent _healthBar;

        protected float _lastShotTime;
        protected readonly List<ProjectileBehaviourComponent> _activeProjectiles = new();

        public virtual int CurrentHealth { get; protected set; }
        public virtual string SpaceshipID {get; protected set; }
        public virtual event Action<ISpaceship> OnDestroyed;

        public abstract void OnSpawned();
        public abstract void OnDespawned();
        public abstract void Move(Vector3 direction, Vector3 minBounds, Vector3 maxBounds);
        public abstract void Shoot();
        public abstract void TakeDamage(int damage);
        
        protected void Destroy()
        {
            OnDestroyed?.Invoke(this);
        }
    } 
    

    public abstract class BaseSpaceshipBehaviourComponent<T, Config> : BaseSpaceshipBehaviourComponent
        where T : BaseSpaceshipBehaviourComponent<T, Config>
        where Config : SpaceshipConfigSO
    {
        [SerializeField] private Config _shipConfig;
        public override string SpaceshipID => _shipConfig.SpaceshipID;

        public override void OnSpawned()
        {
            CurrentHealth = _shipConfig.Health;
            _lastShotTime = 0f;
            _healthBar.Initialize(CurrentHealth, CurrentHealth);
        }

        public override void OnDespawned()
        {
            foreach (var projectile in _activeProjectiles)
            {
                if (projectile != null)
                {
                    projectile.OnProjectileDestroyed -= OnProjectileDestroyed;
                }
            }
            _activeProjectiles.Clear();
        }

        public override void Shoot()
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

        public override void TakeDamage(int damage)
        {
            if (CurrentHealth <= 0)
            {
                return;
            }

            CurrentHealth = Math.Clamp(CurrentHealth - damage, 0, Int32.MaxValue);
            _healthBar.UpdateHealth(CurrentHealth);

            if (CurrentHealth == 0)
            {
                Destroy();
            }
        }

        public override void Move(Vector3 direction, Vector3 minBounds, Vector3 maxBounds)
        {
            direction.Normalize();

            Vector3 movement = direction * (_shipConfig.MoveSpeed * Time.deltaTime);
            Vector3 newPosition = transform.position + movement;

            newPosition.x = Mathf.Clamp(newPosition.x, minBounds.x, maxBounds.x);
            newPosition.y = transform.position.y;
            newPosition.z = Mathf.Clamp(newPosition.z, minBounds.z, maxBounds.z);

            transform.position = newPosition;
        }
        
        protected virtual Vector3 GetProjectileDirection()
        {
            return Vector3.forward;
        }

        protected void OnProjectileDestroyed(ProjectileBehaviourComponent projectile)
        {
            projectile.OnProjectileDestroyed -= OnProjectileDestroyed;
            _activeProjectiles.Remove(projectile);
            _spawnService.Despawn(projectile);
        }
    }
}
