using System;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    public class ProjectileBehaviourComponent : ScreenBoundedMovingComponent
    {
        [SerializeField] private CollisionDetectionComponent _collisionDetection;
        [SerializeField] private Vector3 _defaultFacingDirection = Vector3.back;

        private ShipStats _attackerStats;

        public event Action<ProjectileBehaviourComponent> OnProjectileDestroyed;

        /// <summary>Damage is resolved on impact from the attacker's stats, speed is snapshotted here
        /// so a projectile already in flight keeps its velocity.</summary>
        public void Initialize(ShipStats attackerStats, Vector3 direction)
        {
            _attackerStats = attackerStats;
            _speed = attackerStats.CurrentProjectileSpeed;
            _direction = direction.normalized;
            transform.rotation = Quaternion.FromToRotation(_defaultFacingDirection, _direction);
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
            OnProjectileDestroyed = null;
            _attackerStats = null;
        }

        private void HandleTriggerEnter(Collider other)
        {
            // Matching tags mean the same team, so friendly ships are ignored.
            if (other.CompareTag(gameObject.tag))
            {
                return;
            }

            if (other.TryGetComponent<BaseSpaceshipBehaviourComponent>(out var target))
            {
                target.TakeDamage(_attackerStats);
            }

            TriggerDestroy();
        }

        /// <summary>Raises the destroy event before despawning, so the owner always releases its
        /// reference.</summary>
        protected override void Despawn()
        {
            TriggerDestroy();
        }

        private void TriggerDestroy()
        {
            OnProjectileDestroyed?.Invoke(this);
            base.Despawn();
        }
    }
}
