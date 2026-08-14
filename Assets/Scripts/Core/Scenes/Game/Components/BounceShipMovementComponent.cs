using UnityEngine;
using Random = UnityEngine.Random;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Movement that steers itself: travels in a straight line until it reaches the edge of its
    /// bounds, then reflects off with a random angle variation so a formation never settles into a
    /// pattern. Idle until told to start, since enemies fly a scripted entry first.
    /// </summary>
    public class BounceShipMovementComponent : BaseShipMovementComponent
    {
        /// <summary>Bounds are tested with a tolerance: ships are parented under an offset container,
        /// so the world position does not round trip exactly and an equality test can miss the wall,
        /// leaving the ship clamped against it and sliding along instead of bouncing.</summary>
        private const float BoundsTolerance = 0.01f;

        [SerializeField] private float _bounceAngleVariation = 30f;

        private Vector3 _currentDirection;

        public override void Dispose()
        {
            base.Dispose();
            _currentDirection = Vector3.zero;
        }

        /// <summary>Picks the opening direction. Separate from Initialize because the ship only starts
        /// bouncing once its entry animation lands, not when it spawns.</summary>
        public override void StartMoving()
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

        public override void Tick()
        {
            if (_currentDirection == Vector3.zero)
            {
                return;
            }

            Move(_currentDirection);
            DetectAndBounce();
        }

        private void DetectAndBounce()
        {
            Vector3 currentPosition = transform.position;
            Vector3 minBounds = MinBounds;
            Vector3 maxBounds = MaxBounds;

            bool hitXMin = currentPosition.x <= minBounds.x + BoundsTolerance && _currentDirection.x < 0;
            bool hitXMax = currentPosition.x >= maxBounds.x - BoundsTolerance && _currentDirection.x > 0;

            if (hitXMin || hitXMax)
            {
                _currentDirection.x *= -1;
                ApplyRandomBounce();
            }

            bool hitZMin = currentPosition.z <= minBounds.z + BoundsTolerance && _currentDirection.z < 0;
            bool hitZMax = currentPosition.z >= maxBounds.z - BoundsTolerance && _currentDirection.z > 0;

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
            float randomAngleVariation = Random.Range(-_bounceAngleVariation, _bounceAngleVariation);

            _currentDirection = Quaternion.Euler(0f, randomAngleVariation, 0f) * _currentDirection;
        }
    }
}
