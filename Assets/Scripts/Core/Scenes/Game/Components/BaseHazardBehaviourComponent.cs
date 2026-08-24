using System;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Neutral obstacle that crosses the play area and hurts the player on contact. A destructible one
    /// pays out when shot down, so it is a risk worth taking rather than one to only dodge; an
    /// indestructible one can only ever be avoided. What differs between hazards is where they come
    /// in from, which is left to the subclass.
    /// </summary>
    public abstract class BaseHazardBehaviourComponent : ScreenBoundedMovingComponent, IDamageableTarget
    {
        [SerializeField] private CollisionDetectionComponent _collisionDetection;

        [Tooltip("Optional. Hazards without one simply do not react visually to being hit.")]
        [SerializeField] private HitFlashComponent _hitFlash;

        private HazardConfigSO _config;

        /// <summary>Raised only when the player shot it down.</summary>
        public event Action<HazardTypes, Vector3> OnDestroyed;

        /// <summary>Recreated on every spawn, so a pooled hazard comes back whole.</summary>
        public HazardStats Stats { get; private set; }

        public virtual void Initialize(HazardConfigSO config, Vector3 direction, float entryRatio)
        {
            _config = config;
            Stats = config.CreateStats();

            _direction = direction.normalized;
            _speed = Stats.Speed;

            PlaceOnEntry(entryRatio);
        }

        public override void OnSpawned()
        {
            base.OnSpawned();
            _collisionDetection.OnTriggerEntered += HandleTriggerEnter;
        }

        public override void OnDespawned()
        {
            base.OnDespawned();
            _collisionDetection.OnTriggerEntered -= HandleTriggerEnter;

            // Cleared here too: one that drifts off screen is never destroyed, so the spawner would
            // otherwise stack a listener on every reuse of this instance.
            OnDestroyed = null;

            _config = null;
            Stats = null;
        }

        public void TakeDamage(AttackSourceDTO source)
        {
            if (Stats == null)
            {
                return;
            }

            // Shots still land on an indestructible hazard, they simply achieve nothing. The hit
            // feedback is what tells the player that shooting it is not the answer.
            if (!Stats.IsDestructible)
            {
                SpawnVFX(_config.HitVFXPrefab);
                Flash();
                return;
            }

            if (Stats.IsDestroyed)
            {
                return;
            }

            Stats.ApplyDamage(source.RollDamage(out _));
            SpawnVFX(_config.HitVFXPrefab);
            Flash();

            if (Stats.IsDestroyed)
            {
                Explode(paysOut: true);
            }
        }

        /// <summary>Where this hazard enters from, given a ratio across the spawn edge. Only the
        /// hazard knows its own extents, so the spawner cannot place it.</summary>
        protected abstract void PlaceOnEntry(float entryRatio);

        private void HandleTriggerEnter(Collider other)
        {
            if (Stats == null || !other.TryGetComponent(out ShipHitboxComponent hitbox) || hitbox.Ship is not IPlayerSpaceship)
            {
                return;
            }

            hitbox.Ship.TakeDamage(Stats.ContactDamage);

            // Something breakable comes apart on the player; something that shrugs off gunfire
            // carries on through, so it cannot be cleared by ramming it either.
            if (Stats.IsDestructible)
            {
                Explode(paysOut: false);
            }
        }

        /// <summary>Only a hazard the player shot down pays out. One that landed on them was never
        /// destroyed by them.</summary>
        private void Explode(bool paysOut)
        {
            SpawnVFX(_config.DestroyVFXPrefab);

            if (paysOut)
            {
                OnDestroyed?.Invoke(_config.HazardType, transform.localPosition);
            }

            Despawn();
        }

        /// <summary>Hazards without a flash authored just skip it, so no prefab has to carry one.</summary>
        private void Flash()
        {
            if (_hitFlash == null)
            {
                return;
            }

            _hitFlash.Flash();
        }

        private void SpawnVFX(VFXBehaviourComponent prefab)
        {
            if (prefab == null)
            {
                return;
            }

            _spawnManager.SpawnVFX(prefab, transform.localPosition);
        }
    }
}
