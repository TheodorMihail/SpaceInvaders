using System;
using System.Collections.Generic;
using System.Threading;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface ISpaceship : IPoolableObject
    {
        ShipStats Stats { get; }
        string SpaceshipID { get; }

        /// <summary>Local to the spawn container, which is what every spawn call expects.</summary>
        Vector3 LocalPosition { get; }

        /// <summary>For anything leaving the spawn container's space, such as projecting to screen.</summary>
        Vector3 WorldPosition { get; }
        event Action<ISpaceship> OnDestroyed;
        event Action<int, int> OnHealthChanged;
        event Action<int, int> OnAmmoChanged;
        event Action<float> OnReloadStarted;
        event Action<ISpaceship> OnShotFired;
        event Action<ISpaceship, int, bool> OnDamaged;

        void Move(Vector3 direction, Vector3 minBounds, Vector3 maxBounds);
        void Shoot();
        void TakeDamage(ShipStats attackerStats);
        void TakeDamage(int damage);
    }

    public abstract class BaseSpaceshipBehaviourComponent : MonoBehaviour, ISpaceship
    {
        [Inject] protected ISpawnService _spawnService;
        [SerializeField] protected Renderer _renderer;
        [SerializeField] protected Vector3 _projectileOffset;
        [SerializeField] protected HealthBarUIComponent _healthBar;

        protected float _lastShotTime;

        /// <summary>Kept only to unsubscribe from projectile events on despawn.</summary>
        protected readonly List<ProjectileBehaviourComponent> _activeProjectiles = new();

        public virtual ShipStats Stats { get; protected set; }
        public virtual string SpaceshipID {get; protected set; }
        public virtual Vector3 LocalPosition => transform.localPosition;
        public virtual Vector3 WorldPosition => transform.position;
        public virtual event Action<ISpaceship> OnDestroyed;
        public virtual event Action<int, int> OnHealthChanged;
        public virtual event Action<int, int> OnAmmoChanged;
        public virtual event Action<float> OnReloadStarted;
        public virtual event Action<ISpaceship> OnShotFired;
        public virtual event Action<ISpaceship, int, bool> OnDamaged;

        public abstract void OnSpawned();
        public abstract void OnDespawned();
        public abstract void Move(Vector3 direction, Vector3 minBounds, Vector3 maxBounds);
        public abstract void Shoot();
        public abstract void TakeDamage(ShipStats attackerStats);
        public abstract void TakeDamage(int damage);

        protected virtual void Destroy()
        {
            OnDestroyed?.Invoke(this);
        }

        protected void RaiseHealthChanged(int currentHealth, int maxHealth)
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        protected void RaiseShotFired()
        {
            OnShotFired?.Invoke(this);
        }

        protected void RaiseAmmoChanged(int currentAmmo, int maxAmmo)
        {
            OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
        }

        protected void RaiseReloadStarted(float duration)
        {
            OnReloadStarted?.Invoke(duration);
        }

        /// <summary>Blocks shooting for the given delay, after which the usual fire rate cadence
        /// resumes. Backdating the last shot keeps the cooldown check in Shoot untouched.</summary>
        protected void DelayNextShot(float delay)
        {
            _lastShotTime = Time.time + delay - Stats.CurrentFireRate;
        }

        protected void RaiseDamaged(int damage, bool isCritical)
        {
            OnDamaged?.Invoke(this, damage, isCritical);
        }
    }
    

    /// <summary>
    /// Config-driven spaceship behaviour: stat creation, shooting, damage handling and bounded
    /// movement. Stats are recreated on spawn, so pooled instances start clean.
    /// </summary>
    public abstract class BaseSpaceshipBehaviourComponent<T, Config> : BaseSpaceshipBehaviourComponent
        where T : BaseSpaceshipBehaviourComponent<T, Config>
        where Config : SpaceshipConfigSO
    {
        [SerializeField] protected Config _shipConfig;
        protected Config ShipConfig => _shipConfig;
        public override string SpaceshipID => _shipConfig.SpaceshipID;

        private CancellationTokenSource _reloadCancellationTokenSource;

        public override void OnSpawned()
        {
            CancelReload();

            Stats = _shipConfig.CreateStats();
            Stats.HealthChanged += OnStatsHealthChanged;
            Stats.AmmoChanged += OnStatsAmmoChanged;
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
        }

        private void OnStatsAmmoChanged(int currentAmmo, int maxAmmo)
        {
            RaiseAmmoChanged(currentAmmo, maxAmmo);
        }

        public override void OnDespawned()
        {
            CancelReload();

            foreach (var projectile in _activeProjectiles)
            {
                if (projectile != null)
                {
                    projectile.OnProjectileDestroyed -= OnProjectileDestroyed;
                }
            }
            _activeProjectiles.Clear();
        }

        protected void SpawnDestroyVFX()
        {
            if (_shipConfig.DestroyVFXPrefab == null)
            {
                this.LogWarning("No destroy vfx prefab assigned!");
                return;
            }

            _spawnService.SpawnVFX(_shipConfig.DestroyVFXPrefab, LocalPosition);
        }

        protected void SpawnHitVFX()
        {
            if (_shipConfig.HitVFXPrefab == null)
            {
                this.LogWarning("No hit vfx prefab assigned!");
                return;
            }

            _spawnService.SpawnVFX(_shipConfig.HitVFXPrefab, LocalPosition);
        }

        public override void Shoot()
        {
            if (Time.time - _lastShotTime < Stats.CurrentFireRate)
            {
                return;
            }

            if (!Stats.TryConsumeAmmo())
            {
                return;
            }

            _lastShotTime = Time.time;

            foreach (Vector3 direction in ApplyStatsShotSpread(GetShotDirections()))
            {
                FireProjectile(direction);
            }

            RaiseShotFired();

            if (Stats.IsOutOfAmmo)
            {
                StartReload();
            }
        }

        /// <summary>The delay is time scaled, so a reload halts with the rest of the game while paused.</summary>
        private void StartReload()
        {
            CancelReload();

            _reloadCancellationTokenSource = new CancellationTokenSource();
            RaiseReloadStarted(Stats.CurrentReloadDuration);
            RunReload(Stats.CurrentReloadDuration, _reloadCancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid RunReload(float duration, CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);

            if (token.IsCancellationRequested)
            {
                return;
            }

            CancelReload();
            Stats.RefillAmmo();
        }

        private void CancelReload()
        {
            _reloadCancellationTokenSource?.CancelAndDispose();
            _reloadCancellationTokenSource = null;
        }

        public override void TakeDamage(ShipStats attackerStats)
        {
            if (!CanTakeDamage())
            {
                return;
            }

            int damage = attackerStats.RollOutgoingDamage(out bool isCritical);
            ApplyDamage(damage, isCritical);
        }

        /// <summary>Unattributed damage, which cannot crit as it has no attacker to roll against.</summary>
        public override void TakeDamage(int damage)
        {
            if (!CanTakeDamage())
            {
                return;
            }

            ApplyDamage(damage, false);
        }

        private bool CanTakeDamage()
        {
            return !Stats.IsInvincible && Stats.CurrentHealth > 0;
        }

        /// <summary>Destruction comes last, since it despawns the ship and drops the listeners that
        /// the hit feedback above still needs.</summary>
        private void ApplyDamage(int damage, bool isCritical)
        {
            Stats.ApplyDamage(damage);
            SpawnHitVFX();
            RaiseDamaged(damage, isCritical);

            if (Stats.CurrentHealth == 0)
            {
                Destroy();
            }
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

        /// <summary>Expands each direction into an evenly spread shot pattern centered on it.</summary>
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
            if(_shipConfig.ProjectilePrefab == null)
            {
                this.LogWarning("No projectile prefab assigned!");
                return;
            }

            Vector3 spawnPosition = LocalPosition + _projectileOffset;

            var projectile = _spawnService.SpawnProjectile(
                _shipConfig.ProjectilePrefab,
                spawnPosition,
                direction,
                Stats
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
