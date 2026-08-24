using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SpaceInvaders.Scenes.Game;
using Zenject;
using System.Text;
using System.Collections.Generic;

namespace SpaceInvaders.Project
{
    /// <summary>
    /// Confirm-tooltip shown near a clicked inventory item or ship slot. Uses a center pivot so
    /// its rect always stays fully inside the parent Canvas after clamping.
    /// </summary>
    public class ItemTooltipComponent : MonoBehaviour
    {
        [Inject] private readonly IInventoryManager _inventoryManager;
        [Inject] private readonly IEquipmentManager _equipmentManager;
        [Inject] private readonly IItemsRepository _itemsRepository;
        
        [Header("References")]
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _rarityText;
        [SerializeField] private TextMeshProUGUI _bodyText;
        [Tooltip("Equips or unequips depending on what the shown item currently is.")]
        [SerializeField] private Button _equipButton;
        [SerializeField] private TextMeshProUGUI _equipButtonText;
        [SerializeField] private Button _sellButton;
        [SerializeField] private TextMeshProUGUI _sellButtonText;
        [SerializeField] private List<Button> _closeBackgroundBtnList;

        [Header("Strings")]
        [SerializeField] private string _equipString = "EQUIP";
        [SerializeField] private string _unequipString = "UNEQUIP";
        [SerializeField] private string _sellButtonString = "Sell ({0})";

        [Header("Placement")]
        [SerializeField] private Vector2 _localOffset = new Vector2(-50f, 0);

        private RectTransform _parentRect;
        private Canvas _canvas;
        private RectTransform _canvasRect;
        private string _currentInstanceId;

        public event Action OnHide;

        private void Awake()
        {
            _parentRect = transform.parent as RectTransform;
            _canvas = GetComponentInParent<Canvas>().rootCanvas;
            _canvasRect = _canvas.GetComponent<RectTransform>();

            _equipButton.onClick.AddListener(EquipButtonClicked);
            _sellButton.onClick.AddListener(SellButtonClicked);

            foreach (var btn in _closeBackgroundBtnList)
            {
                btn.onClick.AddListener(Hide);
            }
        }

        public void Show(RectTransform anchor, string instanceId)
        {
            ShowInternal(anchor, instanceId, showActions: true);
        }

        /// <summary>Info-only variant for non-interactive contexts (e.g. the Level Finished
        /// screen) - shows name/rarity/affixes but never an Equip/Unequip button.</summary>
        public void ShowReadOnly(RectTransform anchor, string instanceId)
        {
            ShowInternal(anchor, instanceId, showActions: false);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            OnHide?.Invoke();
        }

        private void ShowInternal(RectTransform anchor, string instanceId, bool showActions)
        {
            Hide();

            InventoryItemEntry entry = _inventoryManager.GetItem(instanceId);
            if (entry == null || !_itemsRepository.TryGetItemConfig(entry.ItemId, out ItemConfigSO config))
            {
                return;
            }

            bool hasRarityConfig = _itemsRepository.TryGetItemRarityConfig(config.Rarity, out ItemRarityConfigSO rarityConfig);
            string rarityText = hasRarityConfig ? rarityConfig.DisplayName : config.Rarity.ToString();
            _rarityText.color = hasRarityConfig ? rarityConfig.DisplayColor : Color.white;

            if (showActions)
            {
                _currentInstanceId = instanceId;
                _equipButton.gameObject.SetActive(true);
                _equipButtonText.text = _equipmentManager.IsEquipped(instanceId) ? _unequipString : _equipString;

                bool canSell = _inventoryManager.TryGetSellValue(instanceId, out int sellValue);
                _sellButton.gameObject.SetActive(canSell);

                if (canSell)
                {
                    _sellButtonText.text = string.Format(_sellButtonString, sellValue);
                }
            }
            else
            {
                _currentInstanceId = null;
                _equipButton.gameObject.SetActive(false);
                _sellButton.gameObject.SetActive(false);
            }

            Show(anchor, config.DisplayName, rarityText, BuildAffixesText(entry));
        }

        /// <summary>Reads the equipped state when clicked rather than when wired, so the action can
        /// never disagree with the label the button is showing.</summary>
        private void EquipButtonClicked()
        {
            if (_currentInstanceId == null)
            {
                return;
            }

            if (_equipmentManager.IsEquipped(_currentInstanceId))
            {
                _equipmentManager.Unequip(_currentInstanceId);
            }
            else
            {
                _equipmentManager.Equip(_currentInstanceId);
            }

            Hide();
        }

        /// <summary>Selling unequips first, so a worn item can go without being taken off by hand.</summary>
        private void SellButtonClicked()
        {
            if (_currentInstanceId == null)
            {
                return;
            }

            _inventoryManager.TrySellItem(_currentInstanceId);
            Hide();
        }

        private void Show(RectTransform anchor, string title, string rarityText, string body)
        {
            gameObject.SetActive(true);

            _titleText.text = title;
            _rarityText.gameObject.SetActive(!string.IsNullOrEmpty(rarityText));
            _rarityText.text = rarityText;
            _bodyText.text = body;

            Canvas.ForceUpdateCanvases();

            PositionNear(anchor);
        }

        private string BuildAffixesText(InventoryItemEntry entry)
        {
            var builder = new StringBuilder();

            foreach (AffixEntry affix in entry.Affixes)
            {
                if (!Enum.TryParse(affix.StatType, out ShipUpgradableStatTypes statType))
                {
                    continue;
                }

                // Empty ValueType means this affix was persisted before the field existed -
                // default to Flat instead of dropping the line for every pre-existing save.
                ShipStatValueTypes valueType = ShipStatValueTypes.Flat;
                if (!string.IsNullOrEmpty(affix.ValueType) && !Enum.TryParse(affix.ValueType, out valueType))
                {
                    continue;
                }

                builder.AppendLine(ShipStats.AffixFormat(statType, affix.Bonus, valueType));
            }

            return builder.ToString().TrimEnd();
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
