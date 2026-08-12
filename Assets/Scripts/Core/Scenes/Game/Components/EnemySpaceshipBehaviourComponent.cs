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

        /// <summary>Works out where this ship lands and returns how far it has to travel. Depth ratio
        /// places it within the formation: 0 lands at the top of the play area, 1 lands deepest.</summary>
        float PrepareEntry(float formationDepthRatio);

        /// <summary>Flies to the prepared spot. The whole wave shares one duration, so the formation
        /// arrives together instead of trickling in.</summary>
        void StartEntryAnimation(float duration);
    }

    /// <summary>
    /// Enemy ship with two movement phases: a tweened entry towards the top of the screen, followed
    /// by bouncing movement within the top half bounds.
    /// </summary>
    public class EnemySpaceshipBehaviourComponent : BaseSpaceshipBehaviourComponent<EnemySpaceshipBehaviourComponent, EnemySpaceshipConfigSO>, IEnemySpaceship
    {
        private enum EnemyState { Entering, Bouncing }

        public EnemyTypes EnemyType => ShipConfig.EnemyType;
        public EnemyCategoryTypes Category => ShipConfig.Category;

        [Inject] private readonly ICameraManager _cameraManager;

        [SerializeField] private float _bounceAngleVariation = 30;

        [Tooltip("How much of the play area a formation may occupy in depth. Lower keeps waves nearer the top.")]
        [SerializeField, Range(0f, 1f)] private float _formationDepthFactor = 0.6f;

        [Tooltip("Upper bound of the random wait before the first shot, so a wave does not fire in unison.")]
        [SerializeField] private float _maxFirstShotDelay = 3f;

        /// <summary>Blocks damage until the entry animation completes.</summary>
        protected virtual bool IsInvulnerableWhileEntering => false;

        private EnemyState _currentState = EnemyState.Entering;
        private Vector3 _currentDirection;
        private Vector3 _minBounds;
        private Vector3 _maxBounds;
        private Vector3 _entryTargetPosition;
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

        public float PrepareEntry(float formationDepthRatio)
        {
            float yPos = transform.position.y;
            Vector3 screenTop = _cameraManager.GetViewportWorldPoint(0.5f, 1f, yPos);
            Vector3 screenBottom = _cameraManager.GetViewportWorldPoint(0.5f, 0f, yPos);
            float screenHeight = screenTop.z - screenBottom.z;

            // Target 10% below the screen top, clamped so the ship stays fully visible.
            _entryTargetPosition = transform.position;
            float desiredZ = screenTop.z - (screenHeight * 0.1f);
            float maxZ = screenTop.z - _renderer.bounds.extents.z;
            float frontZ = Mathf.Min(desiredZ, maxZ);

            // Ships further back in the formation land deeper, so the shape survives the entry.
            // Lerping into the bounds keeps the whole formation inside the enemy play area.
            float depth = Mathf.Clamp01(formationDepthRatio) * _formationDepthFactor;
            _entryTargetPosition.z = Mathf.Clamp(Mathf.Lerp(frontZ, _minBounds.z, depth), _minBounds.z, frontZ);

            _currentState = EnemyState.Entering;

            if (IsInvulnerableWhileEntering)
            {
                Stats.SetInvincible(true);
            }

            return Vector3.Distance(transform.position, _entryTargetPosition);
        }

        /// <summary>Speed is whatever covers this ship's distance in the shared wave duration, so
        /// ships further out simply fly faster and the formation lands as one.</summary>
        public void StartEntryAnimation(float duration)
        {
            if (duration <= 0f)
            {
                transform.position = _entryTargetPosition;
                OnEntryComplete();
                return;
            }

            _entryTween = transform.DOMove(_entryTargetPosition, duration)
                .SetEase(Ease.Linear)
                .OnComplete(OnEntryComplete);
        }

        private void OnEntryComplete()
        {
            _currentState = EnemyState.Bouncing;

            if (IsInvulnerableWhileEntering)
            {
                Stats.SetInvincible(false);
            }

            DelayNextShot(Random.Range(0f, _maxFirstShotDelay));
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

            bool hitXMin = currentPosition.x <= _minBounds.x && _currentDirection.x < 0;
            bool hitXMax = currentPosition.x >= _maxBounds.x && _currentDirection.x > 0;

            if (hitXMin || hitXMax)
            {
                _currentDirection.x *= -1;
                ApplyRandomBounce();
            }

            bool hitZMin = currentPosition.z <= _minBounds.z && _currentDirection.z < 0;
            bool hitZMax = currentPosition.z >= _maxBounds.z && _currentDirection.z > 0;

            if (hitZMin || hitZMax)
            {
                _currentDirection.z *= -1;
                ApplyRandomBounce();
            }

            _currentDirection.Normalize();
        }

        /// <summary>Applies a random angle variation to the reflected direction.</summary>
        private void ApplyRandomBounce()
        {
            float randomAngleVariation = UnityEngine.Random.Range(-_bounceAngleVariation, _bounceAngleVariation);
            float angleInRadians = randomAngleVariation * Mathf.Deg2Rad;

            float cos = Mathf.Cos(angleInRadians);
            float sin = Mathf.Sin(angleInRadians);

            float newX = _currentDirection.x * cos - _currentDirection.z * sin;
            float newZ = _currentDirection.x * sin + _currentDirection.z * cos;

            _currentDirection.x = newX;
            _currentDirection.z = newZ;
        }

    }
}
