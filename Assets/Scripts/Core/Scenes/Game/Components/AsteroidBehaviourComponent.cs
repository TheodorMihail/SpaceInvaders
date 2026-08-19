using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>Hazard that drifts in over the top of the play area on a diagonal, tumbling as it goes.</summary>
    public class AsteroidBehaviourComponent : BaseHazardBehaviourComponent
    {
        /// <summary>The flame art streams up the screen, the same convention projectile art uses, so
        /// travel direction alone decides how far it turns.</summary>
        private static readonly Vector3 FlameFacingDirection = Vector3.forward;

        [Header("Tumble")]
        [Tooltip("The rock's own art. Only this turns, so the flame and any other children stay aligned " +
                 "to the direction of travel instead of spinning with it.")]
        [SerializeField] private Transform _artTransform;

        [Tooltip("Degrees per second. Rolled within this range on every spawn, in either direction, so " +
                 "no two asteroids turn alike.")]
        [SerializeField] private float _minRotationSpeed = 8f;
        [SerializeField] private float _maxRotationSpeed = 25f;

        [Header("Flame")]
        [Tooltip("Optional trail, turned to stream out behind the rock. Point this at a flat pivot in " +
                 "the play plane, not the sprite itself, so the sprite keeps its own tilt.")]
        [SerializeField] private Transform _flameTransform;

        private float _rotationSpeed;

        /// <summary>The flame is aimed here rather than in PlaceOnEntry, since it needs the travel
        /// direction the base has just taken, not the entry position.</summary>
        public override void Initialize(HazardConfigSO config, Vector3 direction, float entryRatio)
        {
            base.Initialize(config, direction, entryRatio);

            AlignFlameToTravel();
        }

        public override void OnSpawned()
        {
            base.OnSpawned();

            float direction = Random.value < 0.5f ? -1f : 1f;
            _rotationSpeed = Random.Range(_minRotationSpeed, _maxRotationSpeed) * direction;

            if (_artTransform == null)
            {
                this.LogWarning("No art transform assigned! The asteroid will not tumble.");
            }
        }

        /// <summary>Lands fully above the visible area, so it drifts into view rather than popping in.</summary>
        protected override void PlaceOnEntry(float entryRatio)
        {
            (Vector3 minBounds, Vector3 maxBounds) = _cameraManager.GetVisibleBounds(_renderer);

            Vector3 position = transform.position;
            position.x = Mathf.Lerp(minBounds.x, maxBounds.x, Mathf.Clamp01(entryRatio));
            position.z = maxBounds.z;
            transform.position = position;
        }

        /// <summary>Turns around world up rather than its own, so art tilted to lie flat still spins in
        /// the plane the camera sees. Runs before the base, which may despawn it this frame.</summary>
        protected override void Update()
        {
            if (_artTransform != null)
            {
                _artTransform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.World);
            }

            base.Update();
        }

        /// <summary>Points the trail back along the asteroid's travel, so it always streams out behind.
        /// Both vectors lie in the play plane, so this is a turn about up and never tips the flame over.</summary>
        private void AlignFlameToTravel()
        {
            if (_flameTransform == null)
            {
                return;
            }

            _flameTransform.rotation = Quaternion.FromToRotation(FlameFacingDirection, -_direction);
        }
    }
}
