using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Fills the space left between the play area and the screen. Only one axis ever has room, so the
    /// borders on the other collapse and switch themselves off.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PlayAreaFrameUIComponent : MonoBehaviour
    {
        [Inject] private readonly ICameraManager _cameraManager;

        [SerializeField] private Image _leftEdgeImage;
        [SerializeField] private Image _rightEdgeImage;
        [SerializeField] private Image _topEdgeImage;
        [SerializeField] private Image _bottomEdgeImage;

        /// <summary>Start rather than Awake: the play area is not answerable until injection has run.</summary>
        private void Start()
        {
            StretchToScreen();

            Rect playAreaRect = _cameraManager.PlayfieldViewportRect;

            SetPlayfieldOuterBorder(_leftEdgeImage, new Vector2(0f, 0f), new Vector2(playAreaRect.xMin, 1f));
            SetPlayfieldOuterBorder(_rightEdgeImage, new Vector2(playAreaRect.xMax, 0f), new Vector2(1f, 1f));
            SetPlayfieldOuterBorder(_topEdgeImage, new Vector2(0f, playAreaRect.yMax), new Vector2(1f, 1f));
            SetPlayfieldOuterBorder(_bottomEdgeImage, new Vector2(0f, 0f), new Vector2(1f, playAreaRect.yMin));
        }

        private void StretchToScreen()
        {
            var rectTransform = (RectTransform)transform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private void SetPlayfieldOuterBorder(Image borderImage, Vector2 anchorMin, Vector2 anchorMax)
        {
            if (borderImage == null)
            {
                return;
            }

            // Nothing is left on this axis, so it would draw at zero size every frame.
            if (anchorMax.x <= anchorMin.x || anchorMax.y <= anchorMin.y)
            {
                borderImage.gameObject.SetActive(false);
                return;
            }

            borderImage.gameObject.SetActive(true);

            // Never take a press meant for the ship.
            borderImage.raycastTarget = false;

            var rectTransform = (RectTransform)borderImage.transform;
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
