using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.MainMenu
{
    /// <summary>
    /// Confirm-tooltip shown near a clicked inventory item or ship slot. Uses a center pivot so
    /// its rect always stays fully inside the parent Canvas after clamping.
    /// </summary>
    public class ItemTooltipComponent : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _rarityText;
        [SerializeField] private TextMeshProUGUI _bodyText;
        [SerializeField] private Button _actionButton;
        [SerializeField] private TextMeshProUGUI _actionButtonText;

        [Header("Placement")]
        [SerializeField] private Vector2 _localOffset = new Vector2(24f, -24f);

        private RectTransform _parentRect;
        private Canvas _canvas;
        private RectTransform _canvasRect;

        public event Action OnActionClicked;

        private void Awake()
        {
            _parentRect = transform.parent as RectTransform;
            _canvas = GetComponentInParent<Canvas>().rootCanvas;
            _canvasRect = _canvas.GetComponent<RectTransform>();

            _actionButton.onClick.AddListener(() => OnActionClicked?.Invoke());
        }

        public void Show(RectTransform anchor, string title, string rarityText, string body, string actionLabel, bool showAction)
        {
            gameObject.SetActive(true);

            _titleText.text = title;
            _rarityText.gameObject.SetActive(!string.IsNullOrEmpty(rarityText));
            _rarityText.text = rarityText;
            _bodyText.text = body;
            _actionButtonText.text = actionLabel;
            _actionButton.gameObject.SetActive(showAction);

            // A freshly (re)activated/resized rect hasn't rebuilt layout yet this frame - force it
            // now so the rect.size read during clamping below isn't stale from before this Show().
            Canvas.ForceUpdateCanvases();

            PositionNear(anchor);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void PositionNear(RectTransform anchor)
        {
            Camera eventCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

            Vector3[] anchorCorners = new Vector3[4];
            anchor.GetWorldCorners(anchorCorners); // order: bottom-left, top-left, top-right, bottom-right
            Vector3 anchorWorldCenter = (anchorCorners[0] + anchorCorners[2]) * 0.5f;
            Vector2 anchorInParent = ToParentLocalPoint(anchorWorldCenter, eventCamera);

            Vector2 desired = anchorInParent + _localOffset;
            _rectTransform.anchoredPosition = ClampToCanvasBounds(desired, eventCamera);
        }

        private Vector2 ToParentLocalPoint(Vector3 worldPoint, Camera eventCamera)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(eventCamera, worldPoint);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, screenPoint, eventCamera, out Vector2 localPoint);
            return localPoint;
        }

        /// <summary>
        /// Converts the canvas' own corners into the tooltip's parent space so the clamp bounds
        /// and the desired position live in the same coordinate frame, regardless of nesting.
        /// </summary>
        private Vector2 ClampToCanvasBounds(Vector2 desiredAnchoredPosition, Camera eventCamera)
        {
            Vector3[] canvasCorners = new Vector3[4];
            _canvasRect.GetWorldCorners(canvasCorners);
            Vector2 canvasMin = ToParentLocalPoint(canvasCorners[0], eventCamera); // bottom-left
            Vector2 canvasMax = ToParentLocalPoint(canvasCorners[2], eventCamera); // top-right

            // Center pivot (0.5, 0.5) means anchoredPosition is the rect's center, so half-size is
            // exactly how far the rect extends on each side - any other pivot needs per-edge math.
            Vector2 halfSize = _rectTransform.rect.size * 0.5f;

            float minX = canvasMin.x + halfSize.x;
            float maxX = canvasMax.x - halfSize.x;
            float minY = canvasMin.y + halfSize.y;
            float maxY = canvasMax.y - halfSize.y;

            float clampedX = minX <= maxX ? Mathf.Clamp(desiredAnchoredPosition.x, minX, maxX) : (minX + maxX) * 0.5f;
            float clampedY = minY <= maxY ? Mathf.Clamp(desiredAnchoredPosition.y, minY, maxY) : (minY + maxY) * 0.5f;

            return new Vector2(clampedX, clampedY);
        }
    }
}
