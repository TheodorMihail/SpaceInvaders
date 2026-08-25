using UnityEngine;
using UnityEngine.InputSystem.OnScreen;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Base for on-screen controls that poll the input devices instead of using pointer events.
    /// Pointer events only reach the object that received the press, and only on that frame, so a
    /// touch held from before the control appeared or first pressed elsewhere never arrives.
    /// Subclasses read the devices each frame and decide what counts as their own pointer.
    /// </summary>
    public abstract class BaseTouchControlUIComponent : OnScreenControl
    {
        /// <summary>The control's own rect. The whole area is active, not just a child handle.</summary>
        protected RectTransform _area;

        /// <summary>Null for a screen space overlay canvas, which is what the rect checks expect.</summary>
        protected Camera _canvasCamera;

        protected override void OnEnable()
        {
            base.OnEnable();

            _area = (RectTransform)transform;
            _canvasCamera = GetCanvasCamera();
        }

        protected bool ContainsScreenPoint(Vector2 screenPosition)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(_area, screenPosition, _canvasCamera);
        }

        private Camera GetCanvasCamera()
        {
            Canvas canvas = GetComponentInParent<Canvas>();

            if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }

            return canvas.worldCamera;
        }
    }
}
