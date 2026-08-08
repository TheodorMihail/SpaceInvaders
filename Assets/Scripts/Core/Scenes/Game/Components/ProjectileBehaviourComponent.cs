using System;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    public class ProjectileBehaviourComponent : ScreenBoundedMovingComponent
    {
        [SerializeField] private CollisionDetectionComponent _collisionDetection;
        [SerializeField] private Vector3 _defaultFacingDirection = Vector3.back;

        private int _damage;

        public event Action<ProjectileBehaviourComponent> OnProjectileDestroyed;

        public void Initialize(int damage, float speed, Vector3 direction)
        {
            _damage = damage;
            _speed = speed;
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
                target.TakeDamage(_damage);
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
