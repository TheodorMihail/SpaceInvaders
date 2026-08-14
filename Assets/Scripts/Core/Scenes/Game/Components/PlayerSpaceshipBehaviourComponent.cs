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
    
    public class PlayerSpaceshipBehaviourComponent : BaseSpaceshipBehaviourComponent<PlayerSpaceshipConfigSO>, IPlayerSpaceship
    {
        [Inject] private readonly IInputService _inputService;

        /// <summary>Input only reports movement, never the lack of it, so each frame starts unset and
        /// is resolved once every tick has been delivered.</summary>
        private bool _isThrustingThisFrame;

        public new event Action<IPlayerSpaceship> OnDestroyed;

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

        protected override void Destroy()
        {
            SpawnDestroyVFX();
            OnDestroyed?.Invoke(this);
        }

        /// <summary>Runs after the input tick, so the flag reflects the whole frame's input.</summary>
        private void LateUpdate()
        {
            SetFlamesThrusting(_isThrustingThisFrame);
            _isThrustingThisFrame = false;
        }

        private void OnPlayerShoot()
        {
            Shoot();
        }

        #region Movement

        /// <summary>Only forward travel lights the engines; strafing and reversing leave them idle.</summary>
        private void OnPlayerMove(Vector3 direction)
        {
            if (direction.z > 0f)
            {
                _isThrustingThisFrame = true;
            }

            Move(direction);
        }

        #endregion
    }
}
