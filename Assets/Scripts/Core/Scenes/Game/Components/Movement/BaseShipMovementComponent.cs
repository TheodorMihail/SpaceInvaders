using BaseArchitecture.Core;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Moves a ship at its stat speed and keeps it inside the region it is allowed to occupy. What
    /// decides the direction is left to the subclass. A ship holds exactly one of these, so swapping
    /// the component on the prefab swaps how the ship moves without touching the ship itself.
    /// </summary>
    public abstract class BaseShipMovementComponent : MonoBehaviour
    {
        [Inject] private readonly ICameraManager _cameraManager;

        public Vector3 MinBounds
        {
            get
            {
                CalculateBounds();
                return _minBounds;
            }
        }

        public Vector3 MaxBounds
        {
            get
            {
                CalculateBounds();
                return _maxBounds;
            }
        }

        /// <summary>The hull this drives. Movement is authored as a child object, so it must never
        /// move its own transform: that would slide the component around inside a stationary ship.</summary>
        protected Transform ShipTransform => _shipTransform;

        [Tooltip("Measured for the bounds query, so it must be the renderer that defines the hull's size.")]
        [SerializeField] private Renderer _renderer;

        [Tooltip("Bottom half of the play area for the player, top half for enemies.")]
        [SerializeField] private ScreenRegionTypes _region = ScreenRegionTypes.BottomRegion;

        private ShipStats _stats;
        private Transform _shipTransform;
        private Vector3 _minBounds;
        private Vector3 _maxBounds;
        private bool _hasBounds;

        public virtual void Initialize(ShipStats stats, Transform shipTransform)
        {
            _stats = stats;
            _shipTransform = shipTransform;

            // Pooling calls OnSpawned before the ship is positioned, so bounds wait for first use.
            _hasBounds = false;
        }

        public virtual void Dispose()
        {
            _stats = null;
            _shipTransform = null;
            _hasBounds = false;
        }

        /// <summary>Travels one frame's worth along the given direction, clamped to the bounds.</summary>
        public void Move(Vector3 direction)
        {
            if (_stats == null || _shipTransform == null)
            {
                return;
            }

            CalculateBounds();

            direction.Normalize();

            Vector3 movement = direction * (_stats.CurrentMoveSpeed * Time.deltaTime);
            Vector3 newPosition = _shipTransform.position + movement;

            newPosition.x = Mathf.Clamp(newPosition.x, _minBounds.x, _maxBounds.x);
            newPosition.y = _shipTransform.position.y;
            newPosition.z = Mathf.Clamp(newPosition.z, _minBounds.z, _maxBounds.z);

            _shipTransform.position = newPosition;
        }

        /// <summary>Hands control over to whatever steers this ship, once it is free to move.</summary>
        public abstract void StartMoving();

        /// <summary>One frame of movement, driven by the owning ship's Update so the ship stays the
        /// only thing deciding whether movement should run at all.</summary>
        public abstract void Tick();

        /// <summary>Resolved on first use rather than on spawn, because a pooled ship is still sitting
        /// where it died when the pool wakes it and would measure its bounds from there.</summary>
        private void CalculateBounds()
        {
            if (_hasBounds)
            {
                return;
            }

            if (_renderer == null)
            {
                this.LogError("No renderer assigned! Movement bounds will collapse to the origin.");
                return;
            }

            (_minBounds, _maxBounds) = _cameraManager.GetPlayableBounds(_renderer, _region);
            _hasBounds = true;
        }
    }
}
