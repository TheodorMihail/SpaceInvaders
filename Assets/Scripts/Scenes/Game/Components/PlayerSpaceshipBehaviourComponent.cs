using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class PlayerSpaceshipBehaviourComponent : SpaceshipBehaviourComponent
    {
        [Inject] private readonly IInputService _inputService;
        [Inject] private readonly ICameraManager _cameraManager;

        private Vector3 _minBounds;
        private Vector3 _maxBounds;

        public override void OnSpawned()
        {
            base.OnSpawned();
            (_minBounds, _maxBounds) = _cameraManager.GetScreenBounds(_renderer, ScreenRegionType.BottomHalf);
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

        #endregion
    }
}
