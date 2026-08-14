using System;
using BaseArchitecture.Core;
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

        void Move(Vector3 direction);
        void Shoot();

        /// <summary>Tops the magazine up early, such as when a wave has been cleared.</summary>
        void Reload();
        void TakeDamage(AttackSourceDTO source);
        void TakeDamage(int damage);
    }

    public abstract class BaseSpaceshipBehaviourComponent : MonoBehaviour, ISpaceship
    {
        [Inject] protected ISpawnService _spawnService;
        [SerializeField] protected BaseShipMovementComponent _movement;
        [SerializeField] protected ShipWeaponComponent _weapon;
        [SerializeField] protected HealthBarUIComponent _healthBar;
        [SerializeField] protected ShipFlameComponent[] _flames;

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
        public abstract void Move(Vector3 direction);
        public abstract void Shoot();
        public abstract void Reload();
        public abstract void TakeDamage(AttackSourceDTO source);
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

        protected void RaiseDamaged(int damage, bool isCritical)
        {
            OnDamaged?.Invoke(this, damage, isCritical);
        }

        /// <summary>Ships without engines authored on them simply have nothing to drive.</summary>
        protected void SetFlamesThrusting(bool isThrusting)
        {
            if (_flames == null)
            {
                return;
            }

            foreach (ShipFlameComponent flame in _flames)
            {
                flame.SetThrusting(isThrusting);
            }
        }
    }
    

    /// <summary>
    /// Config-driven spaceship behaviour: stat creation, shooting, damage handling and bounded
    /// movement. Stats are recreated on spawn, so pooled instances start clean.
    /// </summary>
    public abstract class BaseSpaceshipBehaviourComponent<Config> : BaseSpaceshipBehaviourComponent
        where Config : SpaceshipConfigSO
    {
        [SerializeField] protected Config _shipConfig;
        protected Config ShipConfig => _shipConfig;
        public override string SpaceshipID => _shipConfig.SpaceshipID;

        public override void OnSpawned()
        {
            Stats = _shipConfig.CreateStats();
            Stats.HealthChanged += OnStatsHealthChanged;
            Stats.AmmoChanged += OnStatsAmmoChanged;

            _movement.Initialize(Stats, transform);
            _weapon.Initialize(Stats, gameObject.tag);
            _weapon.ShotFired += OnWeaponShotFired;
            _weapon.ReloadStarted += OnWeaponReloadStarted;

            // Pooled ships can come back mid-burn, so the engines restart idle.
            SetFlamesThrusting(false);

            if (_healthBar == null)
            {
                return;
            }

            _healthBar.Initialize(Stats.CurrentHealth, Stats.CurrentMaxHealth);

            RaiseHealthChanged(Stats.CurrentHealth, Stats.CurrentMaxHealth);
        }

        public override void OnDespawned()
        {
            _weapon.ShotFired -= OnWeaponShotFired;
            _weapon.ReloadStarted -= OnWeaponReloadStarted;

            _movement.Dispose();
            _weapon.Dispose();
        }

        public override void Shoot()
        {
            BaseShipAttackComponent attack = SelectAttack();

            if (_weapon.TryFire(attack))
            {
                OnAttackFired(attack);
            }
        }

        public override void Reload()
        {
            _weapon.Reload();
        }

        public override void TakeDamage(AttackSourceDTO source)
        {
            if (!CanTakeDamage())
            {
                return;
            }

            int damage = source.RollDamage(out bool isCritical);
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

        public override void Move(Vector3 direction)
        {
            _movement.Move(direction);
        }

        /// <summary>Which attack fires now. The default ship has one and always uses it; bosses
        /// override this to rotate through theirs, or override Shoot to run several at once.
        /// Called every frame a ship tries to shoot, so it must have no side effects.</summary>
        protected virtual BaseShipAttackComponent SelectAttack()
        {
            return _weapon.PrimaryAttack;
        }

        /// <summary>Called only when a volley actually left the barrel, which is where a rotation
        /// through several attacks should advance.</summary>
        protected virtual void OnAttackFired(BaseShipAttackComponent attack)
        {
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

        protected void SpawnDestroyVFX()
        {
            if (_shipConfig.DestroyVFXPrefab == null)
            {
                this.LogWarning("No destroy vfx prefab assigned!");
                return;
            }

            _spawnService.SpawnVFX(_shipConfig.DestroyVFXPrefab, LocalPosition);
        }

        private void OnWeaponShotFired()
        {
            RaiseShotFired();
        }

        private void OnWeaponReloadStarted(float duration)
        {
            RaiseReloadStarted(duration);
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
    }
}
