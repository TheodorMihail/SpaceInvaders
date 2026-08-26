using BaseArchitecture.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// On-screen stick that claims any pointer inside its area, whenever and wherever that pointer
    /// was first pressed, then follows it until release. Feeds the same virtual gamepad control as
    /// the built-in stick.
    /// </summary>
    public class JoystickUIComponent : BaseTouchControlUIComponent
    {
        private const int NoPointer = int.MinValue;

        /// <summary>Touch ids are never negative, so the mouse can share the same field.</summary>
        private const int MousePointerId = -1;

        [InputControl(layout = "Vector2")]
        [SerializeField] private string _controlPath = "<Gamepad>/leftStick";

        [SerializeField] private RectTransform _handle;

        [Tooltip("Distance from the centre at which the stick reads fully tilted.")]
        [Min(1f)]
        [SerializeField] private float _movementRange = 100f;

        private int _activePointerId = NoPointer;

        protected override string controlPathInternal
        {
            get => _controlPath;
            set => _controlPath = value;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_handle == null)
            {
                this.LogError("No joystick handle assigned!");
            }
        }

        protected override void OnDisable()
        {
            Release();
            base.OnDisable();
        }

        private void Update()
        {
            if (!TryGetPointerScreenPosition(out Vector2 screenPosition))
            {
                Release();
                return;
            }

            MoveHandleTo(screenPosition);
        }

        private bool TryGetPointerScreenPosition(out Vector2 screenPosition)
        {
            if (_activePointerId != NoPointer)
            {
                return TryReadPointer(_activePointerId, out screenPosition);
            }

            return TryClaimPointer(out screenPosition);
        }

        /// <summary>Follows the claimed pointer wherever it goes, including outside the area.</summary>
        private bool TryReadPointer(int pointerId, out Vector2 screenPosition)
        {
            screenPosition = Vector2.zero;

            if (pointerId == MousePointerId)
            {
                if (Mouse.current == null || !Mouse.current.leftButton.isPressed)
                {
                    return false;
                }

                screenPosition = Mouse.current.position.ReadValue();
                return true;
            }

            if (Touchscreen.current == null)
            {
                return false;
            }

            foreach (TouchControl touch in Touchscreen.current.touches)
            {
                if (!touch.press.isPressed || touch.touchId.ReadValue() != pointerId)
                {
                    continue;
                }

                screenPosition = touch.position.ReadValue();
                return true;
            }

            return false;
        }

        /// <summary>Takes the first pointer currently inside the area, whenever it was pressed.</summary>
        private bool TryClaimPointer(out Vector2 screenPosition)
        {
            screenPosition = Vector2.zero;

            if (Touchscreen.current != null)
            {
                foreach (TouchControl touch in Touchscreen.current.touches)
                {
                    if (!touch.press.isPressed)
                    {
                        continue;
                    }

                    Vector2 touchPosition = touch.position.ReadValue();

                    if (!ContainsScreenPoint(touchPosition))
                    {
                        continue;
                    }

                    _activePointerId = touch.touchId.ReadValue();
                    screenPosition = touchPosition;

                    return true;
                }
            }

            if (Mouse.current == null || !Mouse.current.leftButton.isPressed)
            {
                return false;
            }

            Vector2 mousePosition = Mouse.current.position.ReadValue();

            if (!ContainsScreenPoint(mousePosition))
            {
                return false;
            }

            _activePointerId = MousePointerId;
            screenPosition = mousePosition;

            return true;
        }

        private void MoveHandleTo(Vector2 screenPosition)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_area, screenPosition, _canvasCamera, out Vector2 localPoint))
            {
                return;
            }

            // The area's pivot is not necessarily its centre, so the offset is measured from the rect.
            Vector2 offset = Vector2.ClampMagnitude(localPoint - _area.rect.center, _movementRange);

            if (_handle != null)
            {
                _handle.anchoredPosition = offset;
            }

            SendValueToControl(offset / _movementRange);
        }

        private void Release()
        {
            if (_activePointerId == NoPointer)
            {
                return;
            }

            _activePointerId = NoPointer;

            if (_handle != null)
            {
                _handle.anchoredPosition = Vector2.zero;
            }

            SendValueToControl(Vector2.zero);
        }
    }
}
