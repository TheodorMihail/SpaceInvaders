using System;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    public class ProjectileBehaviourComponent : ScreenBoundedMovingComponent
    {
        /// <summary>Every projectile is drawn pointing up the screen, whoever fires it, so travel
        /// direction alone decides the rotation. Enemy shots simply come out rotated 180.</summary>
        private static readonly Vector3 ArtFacingDirection = Vector3.forward;

        [SerializeField] private CollisionDetectionComponent _collisionDetection;

        private ShipStats _attackerStats;

        public event Action<ProjectileBehaviourComponent> OnProjectileDestroyed;

        /// <summary>Damage is resolved on impact from the attacker's stats, speed is snapshotted here
        /// so a projectile already in flight keeps its velocity, and the tag is restamped every time
        /// because pooled projectiles get reissued to the other team.</summary>
        public void Initialize(ShipStats attackerStats, Vector3 direction, string shooterTag)
        {
            _attackerStats = attackerStats;
            _speed = attackerStats.CurrentProjectileSpeed;
            _direction = direction.normalized;

            gameObject.tag = shooterTag;
            transform.rotation = Quaternion.FromToRotation(ArtFacingDirection, _direction);
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

        /// <summary>Raises the destroy event before despawning, so the owner always releases its
        /// reference.</summary>
        protected override void Despawn()
        {
            TriggerDestroy();
        }

        private void HandleTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out ShipHitboxComponent hitbox) && hitbox.Ship != null)
            {
                // The hitbox answers for its ship, whose tag decides the team.
                if (hitbox.IsSameTeamAs(gameObject))
                {
                    return;
                }

                hitbox.Ship.TakeDamage(_attackerStats);
                TriggerDestroy();

                return;
            }

            // Anything else that carries a collider, other projectiles included, still stops this one
            // unless it flies the same colours.
            if (other.CompareTag(gameObject.tag))
            {
                return;
            }

            TriggerDestroy();
        }

        private void TriggerDestroy()
        {
            OnProjectileDestroyed?.Invoke(this);
            base.Despawn();
        }
    }
}
