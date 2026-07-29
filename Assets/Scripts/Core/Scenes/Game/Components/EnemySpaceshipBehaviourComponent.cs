using System;
using DG.Tweening;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace SpaceInvaders.Scenes.Game
{
    public interface IEnemySpaceship : ISpaceship
    {
        new event Action<IEnemySpaceship> OnDestroyed;
        EnemyTypes EnemyType { get; }
        EnemyCategoryTypes Category { get; }
        void StartEntryAnimation(float entrySpeed);
    }

    public class EnemySpaceshipBehaviourComponent : BaseSpaceshipBehaviourComponent<EnemySpaceshipBehaviourComponent, EnemySpaceshipConfigSO>, IEnemySpaceship
    {
        private enum EnemyState { Entering, Bouncing }

        public EnemyTypes EnemyType => ShipConfig.EnemyType;
        public EnemyCategoryTypes Category => ShipConfig.Category;

        [Inject] private readonly ICameraManager _cameraManager;

        [SerializeField] private float _bounceAngleVariation = 30;

        private EnemyState _currentState = EnemyState.Entering;
        private Vector3 _currentDirection;
        private Vector3 _minBounds;
        private Vector3 _maxBounds;
        private Tween _entryTween;

        public new event Action<IEnemySpaceship> OnDestroyed;

        protected override void Destroy()
        {
            SpawnDestroyVFX();
            OnDestroyed?.Invoke(this);
        }

        public override void OnSpawned()
        {
            base.OnSpawned();
            (_minBounds, _maxBounds) = _cameraManager.GetScreenBounds(_renderer, ScreenRegionTypes.TopHalf);
        }

        public override void OnDespawned()
        {
            base.OnDespawned();

            _entryTween?.Kill();
            _entryTween = null;

            _currentState = EnemyState.Entering;
            _currentDirection = Vector3.zero;
        }

        public void StartEntryAnimation(float entrySpeed)
        {
            // Get screen positions
            float yPos = transform.position.y;
            Vector3 screenTop = _cameraManager.GetViewportWorldPoint(0.5f, 1f, yPos);
            Vector3 screenBottom = _cameraManager.GetViewportWorldPoint(0.5f, 0f, yPos);
            float screenHeight = screenTop.z - screenBottom.z;

            // Calculate target position: current X/Y, but Z at 10% below screen top
            Vector3 targetPosition = transform.position;
            float desiredZ = screenTop.z - (screenHeight * 0.1f);
            float maxZ = screenTop.z - _renderer.bounds.extents.z;
            targetPosition.z = Mathf.Min(desiredZ, maxZ);

            float distance = Vector3.Distance(transform.position, targetPosition);
            float duration = distance / entrySpeed;

            _currentState = EnemyState.Entering;
            _entryTween = transform.DOMove(targetPosition, duration)
                .SetEase(Ease.Linear)
                .OnComplete(OnEntryComplete);
        }

        private void OnEntryComplete()
        {
            _currentState = EnemyState.Bouncing;
            InitializeRandomDirection();
        }

        private void Update()
        {
            if (_currentState == EnemyState.Bouncing)
            {
                UpdateBouncingMovement();
                Shoot(); // Continuously attempt to shoot (fire rate cooldown handled in base)
            }
        }

        protected override Vector3 GetProjectileDirection()
        {
            return Vector3.back; // Enemies shoot downward (toward player)
        }

        private void UpdateBouncingMovement()
        {
            if (_currentDirection == Vector3.zero)
            {
                return;
            }

            Move(_currentDirection, _minBounds, _maxBounds);
            DetectAndBounce();
        }

        private void InitializeRandomDirection()
        {
            float randomAngle = Random.Range(0f, 360f);
            float angleInRadians = randomAngle * Mathf.Deg2Rad;

            // Convert angle to direction vector (XZ plane, Y stays 0)
            _currentDirection = new Vector3(
                Mathf.Cos(angleInRadians),
                0f,
                Mathf.Sin(angleInRadians)
            );

            _currentDirection.Normalize();
        }

        private void DetectAndBounce()
        {
            Vector3 currentPosition = transform.position;

            // Check if we hit X bounds (left/right walls)
            bool hitXMin = currentPosition.x <= _minBounds.x && _currentDirection.x < 0;
            bool hitXMax = currentPosition.x >= _maxBounds.x && _currentDirection.x > 0;

            if (hitXMin || hitXMax)
            {
                // Reflect X direction with random angle variation
                _currentDirection.x *= -1;
                ApplyRandomBounce();
            }

            // Check if we hit Z bounds (top/bottom walls)
            bool hitZMin = currentPosition.z <= _minBounds.z && _currentDirection.z < 0;
            bool hitZMax = currentPosition.z >= _maxBounds.z && _currentDirection.z > 0;

            if (hitZMin || hitZMax)
            {
                // Reflect Z direction with random angle variation
                _currentDirection.z *= -1;
                ApplyRandomBounce();
            }

            // Normalize to maintain consistent speed
            _currentDirection.Normalize();
        }

        private void ApplyRandomBounce()
        {
            float randomAngleVariation = UnityEngine.Random.Range(-_bounceAngleVariation, _bounceAngleVariation);
            float angleInRadians = randomAngleVariation * Mathf.Deg2Rad;

            // Rotate the direction vector by the random angle around Y axis
            float cos = Mathf.Cos(angleInRadians);
            float sin = Mathf.Sin(angleInRadians);

            float newX = _currentDirection.x * cos - _currentDirection.z * sin;
            float newZ = _currentDirection.x * sin + _currentDirection.z * cos;

            _currentDirection.x = newX;
            _currentDirection.z = newZ;
        }

    }
}
