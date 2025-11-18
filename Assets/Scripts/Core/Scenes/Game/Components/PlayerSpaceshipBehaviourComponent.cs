using System;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface IPlayerSpaceship : ISpaceship
    {
        new event Action<IPlayerSpaceship> OnDestroyed;
        void EnableControls();
        void DisableControls();
    }
    
    public class PlayerSpaceshipBehaviourComponent : BaseSpaceshipBehaviourComponent<PlayerSpaceshipBehaviourComponent, PlayerSpaceshipConfigSO>, IPlayerSpaceship
    {
        [Inject] private readonly IInputService _inputService;
        [Inject] private readonly ICameraManager _cameraManager;

        private Vector3 _minBounds;
        private Vector3 _maxBounds;
        
        public new event Action<IPlayerSpaceship> OnDestroyed;

        public override void OnSpawned()
        {
            base.OnSpawned();
            (_minBounds, _maxBounds) = _cameraManager.GetScreenBounds(_renderer, ScreenRegionType.BottomHalf);
        }

        public override void OnDespawned()
        {
            DisableControls();
            base.OnDespawned();
        }

        public void EnableControls()
        {
            _inputService.OnShoot += OnPlayerShoot;
            _inputService.OnMove += OnPlayerMove;
        }

        public void DisableControls()
        {
            _inputService.OnShoot -= OnPlayerShoot;
            _inputService.OnMove -= OnPlayerMove;
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
