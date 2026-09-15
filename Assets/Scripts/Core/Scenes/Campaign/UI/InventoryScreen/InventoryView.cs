using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Campaign
{
    [AddressablePath("Screens/InventoryScreenView")]
    public class InventoryView : View<InventoryModel>
    {
        [Header("Ship Slots (fixed layout)")]
        [SerializeField] private ItemSlotUIComponent _weaponSlot;
        [SerializeField] private ItemSlotUIComponent _coreSlot;
        [SerializeField] private ItemSlotUIComponent _wingLeftSlot;
        [SerializeField] private ItemSlotUIComponent _wingRightSlot;
        [SerializeField] private ItemSlotUIComponent _engineSlot;

        [Header("Item Grid")]
        [SerializeField] private ItemsContainerUIComponent _itemsContainer;

        [Header("Tooltip")]
        [SerializeField] private ItemTooltipUIComponent _tooltip;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI _statSheetText;
        [SerializeField] private Button _backButton;

        [Header("Currency")]
        [SerializeField] private CurrencyUIComponent _currency;

        private readonly Dictionary<EquipmentSlotTypes, List<ItemSlotUIComponent>> _equipmentItemsDic = new();

        private string _lastSelectedInstanceId;

        public event Action<RectTransform, string> OnItemClicked;
        public event Action OnBackClicked;

        private void Awake()
        {
            _tooltip.OnHide += OnTooltipHide;
            _backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
            _itemsContainer.OnItemClicked += HandleItemClicked;
            RegisterEquipmentSlots();
        }

        private void OnDestroy()
        {
            _tooltip.OnHide -= OnTooltipHide;
            _itemsContainer.OnItemClicked -= HandleItemClicked;
        }

        public void Setup()
        {
            InitializeInventory();
            _currency.Initialize(_model.Currency);
            _tooltip.Hide();
        }

        public void OpenTooltip(RectTransform anchor, string instanceId)
        {
            _tooltip.Show(anchor, instanceId);
            UpdateInventoryItemSelected(instanceId, true);
            _lastSelectedInstanceId = instanceId;
        }

        public void RefreshStatsPanel(string stats)
        {
            _statSheetText.text = stats;
        }

        public void RefreshCurrencyDisplay()
        {
            _currency.UpdateCurrency(_model.Currency);
        }

        public void ApplyEquipChange(string equippedInstanceId, string unequippedInstanceId)
        {
            if (unequippedInstanceId != null)
            {
                UpdateItemToEquipmentSlots(unequippedInstanceId, isNowEquipped: false);
            }

            if (equippedInstanceId != null)
            {
                UpdateItemToEquipmentSlots(equippedInstanceId, isNowEquipped: true);
            }
        }

        private void RegisterEquipmentSlots()
        {
            RegisterEquipmentSlotComponent(EquipmentSlotTypes.Weapon, _weaponSlot);
            RegisterEquipmentSlotComponent(EquipmentSlotTypes.Core, _coreSlot);
            RegisterEquipmentSlotComponent(EquipmentSlotTypes.Wings, _wingLeftSlot);
            RegisterEquipmentSlotComponent(EquipmentSlotTypes.Wings, _wingRightSlot);
            RegisterEquipmentSlotComponent(EquipmentSlotTypes.Engine, _engineSlot);
        }

        private void RegisterEquipmentSlotComponent(EquipmentSlotTypes slot, ItemSlotUIComponent component)
        {
            if (!_equipmentItemsDic.TryGetValue(slot, out List<ItemSlotUIComponent> components))
            {
                components = new List<ItemSlotUIComponent>();
                _equipmentItemsDic[slot] = components;
            }

            components.Add(component);
            component.OnClicked += () => HandleEquipmentSlotClicked(slot, component);
        }

        private void InitializeInventory()
        {
            _itemsContainer.Clear();

            IEnumerable<(InventoryItemEntry, ItemConfigSO)> inventoryItems = _model.GetInventoryItems();

            foreach ((InventoryItemEntry entry, ItemConfigSO config) item in inventoryItems)
            {
                _itemsContainer.AddItem(item.entry, item.config, _model.GetItemRarity(item.config.Rarity));

                string instanceId = item.entry.InstanceId;

                if (_model.IsItemEquipped(instanceId))
                {
                    ApplyEquipChange(instanceId, null);
                }
            }
        }

        public void RemoveItem(string instanceId)
        {
            _itemsContainer.RemoveItem(instanceId);
        }

        private void HandleEquipmentSlotClicked(EquipmentSlotTypes slot, ItemSlotUIComponent component)
        {
            if(!_model.TryGetEquippedItemForEquipmentSlotType(slot, out InventoryItemEntry equipped))
            {
                return;
            }

            OnItemClicked?.Invoke(component.RectTransform, equipped.InstanceId);
        }

        private void HandleItemClicked(RectTransform anchor, string instanceId)
        {
            OnItemClicked?.Invoke(anchor, instanceId);
        }

        private void UpdateItemToEquipmentSlots(string instanceId, bool isNowEquipped)
        {
            UpdateInventoryItemEquipped(instanceId, isNowEquipped);
            UpdateInventoryItemSelected(instanceId, false);

            if(!_model.TryGetInventoryItem(instanceId, out (InventoryItemEntry entry, ItemConfigSO config) inventoryItem))
            {
                return;
            }

            if(!_model.TryGetEquipmentSlotTypeForItem(inventoryItem.entry, out EquipmentSlotTypes? slot))
            {
                return;
            }

            if (!_equipmentItemsDic.TryGetValue(slot.Value, out List<ItemSlotUIComponent> components))
            {
                return;
            }

            if (isNowEquipped)
            {
                ItemRarityConfigSO rarity = _model.GetItemRarity(inventoryItem.config.Rarity);

                foreach (ItemSlotUIComponent component in components)
                {
                    component.SetItem(inventoryItem.config, rarity);
                }
            }
            else
            {
                foreach (ItemSlotUIComponent component in components)
                {
                    component.RemoveItem();
                }
            }
        }

        private void UpdateInventoryItemEquipped(string instanceId, bool isEquipped)
        {
            _itemsContainer.SetEquipped(instanceId, isEquipped);
        }

        private void UpdateInventoryItemSelected(string instanceId, bool isSelected)
        {
            _itemsContainer.SetSelected(instanceId, isSelected);
        }

        private void OnTooltipHide()
        {
            UpdateInventoryItemSelected(_lastSelectedInstanceId, false);
            _lastSelectedInstanceId = null;
        }
    }
}
