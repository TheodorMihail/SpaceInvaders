using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceInvaders.Scenes.Game
{
    public class KeyboardInputService : IInputService
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
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                OnAnyKeyPress?.Invoke();
            }
        }

        private void HandleMovementInput()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            Vector3 direction = Vector3.zero;

            // Horizontal input
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            {
                direction.x = -1f;
            }
            else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            {
                direction.x = 1f;
            }

            // Vertical input
            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
            {
                direction.z = 1f;
            }
            else if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
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
            if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
            {
                OnShoot?.Invoke();
            }
        }
    }
}
