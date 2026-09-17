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
    /// Confirm tooltip shown near a clicked item. The caller hands the item over, since one on a shelf
    /// is not owned yet and could not be looked up.
    /// </summary>
    public class ItemTooltipUIComponent : MonoBehaviour
    {
        [Inject] private readonly IInventoryManager _inventoryManager;
        [Inject] private readonly IEquipmentManager _equipmentManager;
        [Inject] private readonly IItemsRepository _itemsRepository;
        
        [Header("References")]
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _rarityText;

        [Tooltip("Which ship slot the item goes in.")]
        [SerializeField] private TextMeshProUGUI _slotTypeText;
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
        [SerializeField] private string _slotTypeString = "{0}";

        [Header("Placement")]
        [SerializeField] private Vector2 _localOffset = new Vector2(-50f, 0);

        [Tooltip("Kept clear of the canvas edges, so a clamped tooltip never sits flush against them.")]
        [SerializeField] private Vector2 _canvasPadding = new Vector2(12f, 12f);

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

        public void Show(RectTransform anchor, InventoryItemEntry entry)
        {
            ShowInternal(anchor, entry, showActions: true);
        }

        /// <summary>Info-only: name, rarity and affixes, never an action. The only variant an item the
        /// player does not own can be shown through.</summary>
        public void ShowReadOnly(RectTransform anchor, InventoryItemEntry entry)
        {
            ShowInternal(anchor, entry, showActions: false);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            OnHide?.Invoke();
        }

        private void ShowInternal(RectTransform anchor, InventoryItemEntry entry, bool showActions)
        {
            Hide();

            if (entry == null || !_itemsRepository.TryGetItemConfig(entry.ItemId, out ItemConfigSO config))
            {
                return;
            }

            string instanceId = entry.InstanceId;

            bool hasRarityConfig = _itemsRepository.TryGetItemRarityConfig(config.Rarity, out ItemRarityConfigSO rarityConfig);
            string rarityText = hasRarityConfig ? rarityConfig.DisplayName : config.Rarity.ToString();
            _rarityText.color = hasRarityConfig ? rarityConfig.DisplayColor : Color.white;

            _slotTypeText.text = string.Format(_slotTypeString, GetSlotDisplayName(config.SlotType));

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

            // The rect is sized from its content, so it has to be rebuilt before it can be measured.
            // Waiting for the normal layout pass would place this frame's tooltip on the last one's size.
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);

            PositionNear(anchor);
        }

        /// <summary>Read off the equipment slot that accepts the item, so the tooltip and the ship slot
        /// it belongs in are never labelled differently.</summary>
        private string GetSlotDisplayName(ItemSlotTypes slotType)
        {
            foreach (EquipmentSlotConfigDTO slotConfig in _itemsRepository.GetAllEquipmentSlotConfigs())
            {
                if (slotConfig.AcceptedType == slotType)
                {
                    return slotConfig.DisplayName;
                }
            }

            return slotType.ToString();
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
        /// Converts the canvas corners into the tooltip's parent space, so the clamp bounds and the
        /// desired position are in the same coordinate space.
        /// </summary>
        private Vector2 ClampToCanvasBounds(Vector2 desiredAnchoredPosition, Camera eventCamera)
        {
            Vector3[] canvasCorners = new Vector3[4];
            _canvasRect.GetWorldCorners(canvasCorners);
            Vector2 canvasMin = ToParentLocalPoint(canvasCorners[0], eventCamera); // bottom-left
            Vector2 canvasMax = ToParentLocalPoint(canvasCorners[2], eventCamera); // top-right

            // anchoredPosition sits at the pivot, so the rect reaches pivot x size one way and the
            // remainder the other. Assuming a centred pivot let a right-pivoted rect, which is placed
            // by its right edge, hang a full width off the near side.
            Vector2 size = _rectTransform.rect.size;
            Vector2 pivot = _rectTransform.pivot;

            float minX = canvasMin.x + _canvasPadding.x + pivot.x * size.x;
            float maxX = canvasMax.x - _canvasPadding.x - (1f - pivot.x) * size.x;
            float minY = canvasMin.y + _canvasPadding.y + pivot.y * size.y;
            float maxY = canvasMax.y - _canvasPadding.y - (1f - pivot.y) * size.y;

            return new Vector2(ClampToRange(desiredAnchoredPosition.x, minX, maxX),
                ClampToRange(desiredAnchoredPosition.y, minY, maxY));
        }

        /// <summary>A rect too big for the canvas leaves no valid range, so it is centred rather than
        /// pinned to whichever edge the clamp happened to reach first.</summary>
        private static float ClampToRange(float value, float min, float max)
        {
            return min <= max ? Mathf.Clamp(value, min, max) : (min + max) * 0.5f;
        }
    }
}
