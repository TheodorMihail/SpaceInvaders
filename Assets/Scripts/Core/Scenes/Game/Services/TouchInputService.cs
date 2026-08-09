using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Reads from the virtual Gamepad/Touchscreen devices that MobileControlsHUD's
    /// OnScreenStick/OnScreenButton components drive - it never talks to that UI directly.
    /// </summary>
    public class TouchInputService : IInputService
    {
        public event Action OnShoot;
        public event Action<Vector3> OnMove;
        public event Action OnAnyKeyPress;
        public event Action OnPause;

        public void Tick()
        {
            HandleAnyTouchPress();
            HandlePauseInput();
            HandleMovementInput();
            HandleShootInput();
        }

        /// <summary>The Android back button surfaces as the escape key.</summary>
        private void HandlePauseInput()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                OnPause?.Invoke();
            }
        }

        private void HandleAnyTouchPress()
        {
            bool pressed = (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

            if (pressed)
            {
                OnAnyKeyPress?.Invoke();
            }
        }

        private void HandleMovementInput()
        {
            if (Gamepad.current == null)
            {
                return;
            }

            Vector2 stick = Gamepad.current.leftStick.ReadValue();

            if (stick != Vector2.zero)
            {
                OnMove?.Invoke(new Vector3(stick.x, 0f, stick.y));
            }
        }

        private void HandleShootInput()
        {
            if (Gamepad.current != null && Gamepad.current.buttonSouth.isPressed)
            {
                OnShoot?.Invoke();
            }
        }
    }
}
