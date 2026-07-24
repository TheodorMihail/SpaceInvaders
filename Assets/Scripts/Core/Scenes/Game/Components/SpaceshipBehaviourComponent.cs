using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface ISpaceship : IPoolableObject
    {
        ShipStats Stats { get; }
        string SpaceshipID { get; }
        Vector3 Position { get; }
        event Action<ISpaceship> OnDestroyed;
        event Action<int, int> OnHealthChanged;

        void Move(Vector3 direction, Vector3 minBounds, Vector3 maxBounds);
        void Shoot();
        void TakeDamage(int damage);
    }

    public abstract class BaseSpaceshipBehaviourComponent : MonoBehaviour, ISpaceship
    {
        [Inject] protected ISpawnService _spawnService;
        [SerializeField] protected Renderer _renderer;
        [SerializeField] protected Vector3 _projectileOffset;
        [SerializeField] protected HealthBarUIComponent _healthBar;

        protected float _lastShotTime;
        protected readonly List<ProjectileBehaviourComponent> _activeProjectiles = new();

        public virtual ShipStats Stats { get; protected set; }
        public virtual string SpaceshipID {get; protected set; }
        public virtual Vector3 Position => transform.localPosition;
        public virtual event Action<ISpaceship> OnDestroyed;
        public virtual event Action<int, int> OnHealthChanged;

        public abstract void OnSpawned();
        public abstract void OnDespawned();
        public abstract void Move(Vector3 direction, Vector3 minBounds, Vector3 maxBounds);
        public abstract void Shoot();
        public abstract void TakeDamage(int damage);

        protected virtual void Destroy()
        {
            OnDestroyed?.Invoke(this);
        }

        protected void RaiseHealthChanged(int currentHealth, int maxHealth)
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
    

    public abstract class BaseSpaceshipBehaviourComponent<T, Config> : BaseSpaceshipBehaviourComponent
        where T : BaseSpaceshipBehaviourComponent<T, Config>
        where Config : SpaceshipConfigSO
    {
        [SerializeField] protected Config _shipConfig;
        protected Config ShipConfig => _shipConfig;
        public override string SpaceshipID => _shipConfig.SpaceshipID;

        public override void OnSpawned()
        {
            Stats = _shipConfig.CreateStats();
            Stats.HealthChanged += OnStatsHealthChanged;
            _lastShotTime = 0f;

            if (_healthBar == null)
            {
                return;
            }

            _healthBar.Initialize(Stats.CurrentHealth, Stats.CurrentMaxHealth);

            RaiseHealthChanged(Stats.CurrentHealth, Stats.CurrentMaxHealth);
        }

        private void OnStatsHealthChanged(int currentHealth, int maxHealth)
        {
            if (_healthBar != null)
            {
                _healthBar.Initialize(currentHealth, maxHealth, false);
            }

            RaiseHealthChanged(currentHealth, maxHealth);

            if (currentHealth == 0)
            {
                Destroy();
            }
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
            if (Time.time - _lastShotTime < Stats.CurrentFireRate)
            {
                return;
            }

            _lastShotTime = Time.time;

            foreach (Vector3 direction in ApplyStatsShotSpread(GetShotDirections()))
            {
                FireProjectile(direction);
            }
        }

        public override void TakeDamage(int damage)
        {
            if (Stats.IsInvincible)
            {
                return;
            }

            if (Stats.CurrentHealth <= 0)
            {
                return;
            }

            Stats.ApplyDamage(damage);
        }

        public override void Move(Vector3 direction, Vector3 minBounds, Vector3 maxBounds)
        {
            direction.Normalize();

            Vector3 movement = direction * (Stats.CurrentMoveSpeed * Time.deltaTime);
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

        protected virtual IEnumerable<Vector3> GetShotDirections()
        {
            yield return GetProjectileDirection();
        }

        private IEnumerable<Vector3> ApplyStatsShotSpread(IEnumerable<Vector3> baseDirections)
        {
            if (Stats.ExtraShotCount <= 0)
            {
                foreach (var direction in baseDirections)
                {
                    yield return direction;
                }
                yield break;
            }

            int totalShots = Stats.ExtraShotCount + 1;
            float startAngle = -(Stats.SpreadAngleDegrees * (totalShots - 1)) / 2f;

            foreach (var baseDirection in baseDirections)
            {
                for (int i = 0; i < totalShots; i++)
                {
                    float angle = startAngle + i * Stats.SpreadAngleDegrees;
                    yield return Quaternion.Euler(0f, angle, 0f) * baseDirection;
                }
            }
        }

        protected void FireProjectile(Vector3 direction)
        {
            Vector3 spawnPosition = transform.localPosition + _projectileOffset;

            var projectile = _spawnService.SpawnProjectile(
                _shipConfig.ProjectilePrefab,
                spawnPosition,
                direction,
                Stats.CurrentProjectileDamage,
                Stats.CurrentProjectileSpeed
            );

            if (projectile != null)
            {
                _activeProjectiles.Add(projectile);
                projectile.OnProjectileDestroyed += OnProjectileDestroyed;
            }
        }

        protected void OnProjectileDestroyed(ProjectileBehaviourComponent projectile)
        {
            projectile.OnProjectileDestroyed -= OnProjectileDestroyed;
            _activeProjectiles.Remove(projectile);
        }
    }
}
