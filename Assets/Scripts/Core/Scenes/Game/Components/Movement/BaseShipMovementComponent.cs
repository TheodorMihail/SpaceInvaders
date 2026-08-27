using BaseArchitecture.Core;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Moves a ship at its stat speed and clamps it to its allowed region. The subclass decides the
    /// direction. A ship holds exactly one, so swapping the component swaps how the ship moves.
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

        /// <summary>The hull this drives. Movement is a child object, so moving its own transform
        /// would shift the component inside a stationary ship.</summary>
        protected Transform ShipTransform => _shipTransform;

        public Renderer Renderer => _renderer;

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

        public abstract void StartMoving();

        /// <summary>Called from the owning ship's Update, so the ship decides whether movement runs.</summary>
        public abstract void Tick();

        /// <summary>Resolved on first use, not on spawn: a pooled ship is still at its old position
        /// when the pool wakes it and would measure its bounds from there.</summary>
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

            (_minBounds, _maxBounds) = _cameraManager.GetPlayfieldRegionBounds(_renderer, _region);
            _hasBounds = true;
        }
    }
}
