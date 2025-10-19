using System;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface IInputService :  ITickable
    {
        event Action OnShoot;
        event Action<Vector3> OnMove;
        event Action OnAnyKeyPress;
    }

    public class InputService : IInputService
    {
        public event Action OnShoot;
        public event Action<Vector3> OnMove;
        public event Action OnAnyKeyPress;

        public void Tick()
        {
            HandleAnyKeyPress();
            HandleMovementInput();
            HandleShootInput();
        }

        private void HandleAnyKeyPress()
        {
            if (Input.anyKeyDown)
            {
                OnAnyKeyPress?.Invoke();
            }
        }

        private void HandleMovementInput()
        {
            Vector3 direction = Vector3.zero;

            // Horizontal input
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                direction.x = -1f;
            }
            else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                direction.x = 1f;
            }

            // Vertical input
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                direction.z = 1f;
            }
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                direction.z = -1f;
            }

            if (direction != Vector3.zero)
            {
                OnMove?.Invoke(direction);
            }
        }

        private void HandleShootInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnShoot?.Invoke();
            }
        }
    }
}
