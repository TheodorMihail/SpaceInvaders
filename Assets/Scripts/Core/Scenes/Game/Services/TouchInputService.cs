using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Reads the virtual Gamepad and Touchscreen devices that the on-screen controls drive. It never
    /// talks to that UI directly.
    /// </summary>
    public class TouchInputService : IInputService
    {
        public bool AnyKeyPressed =>
            (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        /// <summary>The Android back button surfaces as the escape key.</summary>
        public bool PausePressed => Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

        public bool ShootPressed => Gamepad.current != null && Gamepad.current.buttonSouth.isPressed;

        public Vector3 MoveDirection => GetMoveDirection();

        private Vector3 GetMoveDirection()
        {
            if (Gamepad.current == null)
            {
                return Vector3.zero;
            }

            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            return new Vector3(stick.x, 0f, stick.y);
        }
    }
}
