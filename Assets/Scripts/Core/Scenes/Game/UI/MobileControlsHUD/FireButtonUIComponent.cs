using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// On-screen button that reads pressed while any pointer is inside its area, whenever and
    /// wherever that pointer was first pressed. Feeds the same virtual gamepad button as the
    /// built-in on-screen button.
    /// </summary>
    public class FireButtonUIComponent : BaseTouchControlUIComponent
    {
        [InputControl(layout = "Button")]
        [SerializeField] private string _controlPath = "<Gamepad>/buttonSouth";

        private bool _isPressed;

        protected override string controlPathInternal
        {
            get => _controlPath;
            set => _controlPath = value;
        }

        protected override void OnDisable()
        {
            SetPressed(false);
            base.OnDisable();
        }

        private void Update()
        {
            SetPressed(IsAnyPointerInside());
        }

        /// <summary>Any pointer counts, so a second finger works while the first holds the stick.</summary>
        private bool IsAnyPointerInside()
        {
            if (Touchscreen.current != null)
            {
                foreach (TouchControl touch in Touchscreen.current.touches)
                {
                    if (touch.press.isPressed && ContainsScreenPoint(touch.position.ReadValue()))
                    {
                        return true;
                    }
                }
            }

            return Mouse.current != null
                && Mouse.current.leftButton.isPressed
                && ContainsScreenPoint(Mouse.current.position.ReadValue());
        }

        /// <summary>Sends only on a change, so holding does not queue an event every frame.</summary>
        private void SetPressed(bool isPressed)
        {
            if (_isPressed == isPressed)
            {
                return;
            }

            _isPressed = isPressed;
            SendValueToControl(isPressed ? 1f : 0f);
        }
    }
}
