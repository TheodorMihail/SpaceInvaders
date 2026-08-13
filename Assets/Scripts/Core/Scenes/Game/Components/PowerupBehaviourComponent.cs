using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>Powerup pickup that falls down the screen and is activated on player contact.</summary>
    public class PowerupBehaviourComponent : ScreenBoundedMovingComponent
    {
        [Inject] private readonly IPowerupManager _powerupManager;

        [SerializeField] private SpriteRenderer _iconRenderer;
        [SerializeField] private CollisionDetectionComponent _collisionDetection;
        [SerializeField] private float _fallSpeed = 30f;

        private PowerupTypes _powerupType;

        /// <summary>One prefab serves every powerup, so the look comes in with the icon.</summary>
        public void Initialize(PowerupTypes powerupType, Sprite icon)
        {
            _powerupType = powerupType;
            _direction = Vector3.back;
            _speed = _fallSpeed;

            if (_iconRenderer != null)
            {
                _iconRenderer.sprite = icon;
            }
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
        }

        private void HandleTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<IPlayerSpaceship>(out _))
            {
                return;
            }

            _powerupManager.ActivatePowerup(_powerupType);
            Despawn();
        }
    }
}
