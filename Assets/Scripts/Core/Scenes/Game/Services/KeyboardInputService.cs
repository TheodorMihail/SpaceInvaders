using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceInvaders.Scenes.Game
{
    public class KeyboardInputService : IInputService
    {
        public bool AnyKeyPressed => Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        public bool PausePressed => Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        public bool ShootPressed => Keyboard.current != null && Keyboard.current.spaceKey.isPressed;

        public Vector3 MoveDirection => GetMoveDirection();

        private Vector3 GetMoveDirection()
        {
            if (Keyboard.current == null)
            {
                return Vector3.zero;
            }

            Vector3 direction = Vector3.zero;

            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            {
                direction.x = -1f;
            }
            else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            {
                direction.x = 1f;
            }

            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
            {
                direction.z = 1f;
            }
            else if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
            {
                direction.z = -1f;
            }

            return direction;
        }
    }
}
