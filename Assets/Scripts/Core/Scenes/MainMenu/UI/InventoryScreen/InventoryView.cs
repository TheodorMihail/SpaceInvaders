using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SpaceInvaders.Scenes.MainMenu
{
    [AddressablePath("Screens/InventoryScreenView")]
    public class InventoryView : View<InventoryModel>
    {
        [Inject] private readonly ICustomFactory _factory;

        [Header("Ship Slots (fixed layout)")]
        [SerializeField] private ItemSlotComponent _weaponSlot;
        [SerializeField] private ItemSlotComponent _coreSlot;
        [SerializeField] private ItemSlotComponent _wingLeftSlot;
        [SerializeField] private ItemSlotComponent _wingRightSlot;
        [SerializeField] private ItemSlotComponent _engineSlot;

        [Header("Item Grid")]
        [SerializeField] private ItemSlotComponent _itemCellPrefab;
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private TextMeshProUGUI _emptyInventoryText;

        [Header("Tooltip")]
        [SerializeField] private ItemTooltipComponent _tooltip;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI _statSheetText;
        [SerializeField] private Button _backButton;

        private readonly Dictionary<EquipmentSlotTypes, List<ItemSlotComponent>> _equipmentItemsDic = new();
        private readonly Dictionary<string, ItemSlotComponent> _inventoryItemsDic = new();

        private string _lastSelectedInstanceId;

        public event Action<RectTransform, string> OnItemClicked;
        public event Action OnBackClicked;

        private void Awake()
        {
            _tooltip.OnHide += OnTooltipHide;
            _backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
            RegisterEquipmentSlots();
        }

        public void Setup()
        {
            InitializeInventory();
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

        private void RegisterEquipmentSlotComponent(EquipmentSlotTypes slot, ItemSlotComponent component)
        {
            if (!_equipmentItemsDic.TryGetValue(slot, out List<ItemSlotComponent> components))
            {
                components = new List<ItemSlotComponent>();
                _equipmentItemsDic[slot] = components;
            }

            components.Add(component);
            component.OnClicked += () => HandleEquipmentSlotClicked(slot, component);
        }

        private void InitializeInventory()
        {
            IEnumerable<(InventoryItemEntry, ItemConfigSO)> inventoryItems = _model.GetInventoryItems();

            foreach ((InventoryItemEntry entry, ItemConfigSO config) item in inventoryItems)
            {
                ItemSlotComponent cell = _factory.CreateFromPrefab(_itemCellPrefab, _itemsContainer);
                string instanceId = item.entry.InstanceId;
                cell.OnClicked += () => HandleItemClicked(instanceId, cell);

                cell.SetItem(item.config, _model.GetItemRarity(item.config.Rarity));
                _inventoryItemsDic[instanceId] = cell;

                bool isEquipped = _model.IsItemEquipped(instanceId);
                ApplyEquipChange(isEquipped ? instanceId : null, null);
            }

            _emptyInventoryText.gameObject.SetActive(_inventoryItemsDic.Count == 0);
            _emptyInventoryText.text = _model.EmptyInventoryText;
        }

        private void HandleEquipmentSlotClicked(EquipmentSlotTypes slot, ItemSlotComponent component)
        {
            if(!_model.TryGetEquippedItemForEquipmentSlotType(slot, out InventoryItemEntry equipped))
            {
                return;
            }

            OnItemClicked?.Invoke(component.RectTransform, equipped.InstanceId);
        }

        private void HandleItemClicked(string instanceId, ItemSlotComponent cell)
        {
            OnItemClicked?.Invoke(cell.RectTransform, instanceId);
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

            if (!_equipmentItemsDic.TryGetValue(slot.Value, out List<ItemSlotComponent> components))
            {
                return;
            }

            if (isNowEquipped)
            {
                ItemRarityConfigSO rarity = _model.GetItemRarity(inventoryItem.config.Rarity);

                foreach (ItemSlotComponent component in components)
                {
                    component.SetItem(inventoryItem.config, rarity);
                }
            }
            else
            {
                foreach (ItemSlotComponent component in components)
                {
                    component.RemoveItem();
                }
            }
        }

        private void UpdateInventoryItemEquipped(string instanceId, bool isEquipped)
        {
            if (instanceId == null)
            {
                return;
            }

            if (_inventoryItemsDic.TryGetValue(instanceId, out ItemSlotComponent itemSlot))
            {
                itemSlot.SetEquipped(isEquipped);
            }
        }

        private void UpdateInventoryItemSelected(string instanceId, bool isSelected)
        {
            if(instanceId == null)
            {
                return;
            }

            if (_inventoryItemsDic.TryGetValue(instanceId, out ItemSlotComponent itemSlot))
            {
                itemSlot.SetSelected(isSelected);
            }
        }

        private void OnTooltipHide()
        {
            UpdateInventoryItemSelected(_lastSelectedInstanceId, false);
            _lastSelectedInstanceId = null;
        }
    }
}
