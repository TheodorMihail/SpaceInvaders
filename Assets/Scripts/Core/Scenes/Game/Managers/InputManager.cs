using System;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface IInputManager : ITickable
    {
        event Action OnShoot;
        event Action<Vector3> OnMove;
        event Action OnAnyKeyPress;
        event Action OnPause;
    }

    /// <summary>Single input entry point. Which device is read is decided at bind time.</summary>
    public class InputManager : IInputManager
    {
        [Inject] private readonly IInputService _inputService;

        public event Action OnShoot;
        public event Action<Vector3> OnMove;
        public event Action OnAnyKeyPress;
        public event Action OnPause;

        public void Tick()
        {
            if (_inputService.AnyKeyPressed)
            {
                OnAnyKeyPress?.Invoke();
            }

            if (_inputService.PausePressed)
            {
                OnPause?.Invoke();
            }

            Vector3 direction = _inputService.MoveDirection;
            if (direction != Vector3.zero)
            {
                OnMove?.Invoke(direction);
            }

            if (_inputService.ShootPressed)
            {
                OnShoot?.Invoke();
            }
        }
    }
}
