using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class PlayerSpaceshipBehaviourComponent : SpaceshipBehaviourComponent
    {
        [Inject] private readonly IInputService _inputService;

        private Vector3 _minBounds;
        private Vector3 _maxBounds;
        private Vector3 _extents;

        public override void OnSpawned()
        {
            base.OnSpawned();
            CalculateBounds();
        }

        public override void OnDespawned()
        {
            base.OnDespawned();
            _inputService.OnShoot -= OnPlayerShoot;
            _inputService.OnMove -= OnPlayerMove;
        }

        public void EnableControls()
        {
            _inputService.OnShoot += OnPlayerShoot;
            _inputService.OnMove += OnPlayerMove;
        }

        private void OnPlayerShoot()
        {
            Shoot();
        }

        #region Movement

        private void OnPlayerMove(Vector3 direction)
        {
            Move(direction, _minBounds, _maxBounds);
        }

        private void CalculateBounds()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            _extents = _renderer.bounds.extents;
            Vector3 screenBottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, transform.position.y));
            Vector3 screenTopRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, transform.position.y));
            Vector3 screenCenter = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, transform.position.y));

            _minBounds = new Vector3(screenBottomLeft.x + _extents.x, transform.position.y, screenBottomLeft.z + _extents.z);
            _maxBounds = new Vector3(screenTopRight.x - _extents.x, transform.position.y, screenCenter.z - _extents.z);
        }

        #endregion
    }
}
