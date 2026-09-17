using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Expedition
{
    [AddressablePath("Screens/ExpeditionShopScreenView")]
    public class ExpeditionShopScreenView : View<ExpeditionShopScreenModel>
    {
        [Header("Ship Slots (fixed layout)")]
        [SerializeField] private ItemSlotUIComponent _weaponSlot;
        [SerializeField] private ItemSlotUIComponent _coreSlot;
        [SerializeField] private ItemSlotUIComponent _wingLeftSlot;
        [SerializeField] private ItemSlotUIComponent _wingRightSlot;
        [SerializeField] private ItemSlotUIComponent _engineSlot;

        [Header("Shelves")]
        [Tooltip("The stock is dealt across these in order, so how many there are is a layout choice.")]
        [SerializeField] private List<ExpeditionShopShelfUIComponent> _shelves = new();

        [SerializeField] private ItemTooltipUIComponent _tooltip;

        [Header("Repair")]
        [SerializeField] private Button _repairButton;
        [SerializeField] private TextMeshProUGUI _repairButtonText;
        [SerializeField] private string _repairString = "REPAIR ({0})";

        [Tooltip("Shown instead of a price once there is nothing left to mend.")]
        [SerializeField] private string _repairedString = "REPAIRED";

        [Header("Currency")]
        [SerializeField] private CurrencyUIComponent _currency;

        [SerializeField] private Button _doneButton;

        private readonly Dictionary<EquipmentSlotTypes, List<ItemSlotUIComponent>> _equipmentItemsDic = new();

        private ItemSlotUIComponent _selectedSlot;

        public event Action<string> OnBuyClicked;
        public event Action OnRepairClicked;
        public event Action OnDoneClicked;

        private void Awake()
        {
            _doneButton.onClick.AddListener(() => OnDoneClicked?.Invoke());
            _repairButton.onClick.AddListener(() => OnRepairClicked?.Invoke());
            _tooltip.OnHide += HandleTooltipHidden;

            foreach (ExpeditionShopShelfUIComponent shelf in _shelves)
            {
                shelf.OnSlotClicked += HandleOfferSlotClicked;
                shelf.OnBuyClicked += HandleBuyClicked;
            }

            RegisterEquipmentSlots();
        }

        private void OnDestroy()
        {
            _tooltip.OnHide -= HandleTooltipHidden;

            foreach (ExpeditionShopShelfUIComponent shelf in _shelves)
            {
                shelf.OnSlotClicked -= HandleOfferSlotClicked;
                shelf.OnBuyClicked -= HandleBuyClicked;
            }
        }

        public void Setup()
        {
            BuildShelves();
            _currency.Initialize(_model.Currency);
            _tooltip.Hide();

            RefreshEquipmentSlots();
            RefreshAffordability();
        }

        /// <summary>The ship slots are redrawn wholesale, since a purchase equips itself and may
        /// displace whatever was worn.</summary>
        public void ApplyPurchase(string instanceId)
        {
            foreach (ExpeditionShopShelfUIComponent shelf in _shelves)
            {
                shelf.SetSold(instanceId);
            }

            _currency.UpdateCurrency(_model.Currency);
            _tooltip.Hide();

            RefreshEquipmentSlots();
            RefreshAffordability();
        }

        /// <summary>Spending on a repair is what the shelf is weighed against next, so the cards are
        /// re-read alongside the button.</summary>
        public void ApplyRepair()
        {
            _currency.UpdateCurrency(_model.Currency);

            RefreshAffordability();
        }

        /// <summary>Dealt evenly in order, so a shelf count changed in the prefab needs nothing here
        /// and a stock that does not divide evenly fills the earlier shelves first.</summary>
        private void BuildShelves()
        {
            var stock = new List<ExpeditionShopOfferDTO>(_model.GetOffers());

            if (_shelves.Count == 0)
            {
                return;
            }

            int perShelf = Mathf.CeilToInt(stock.Count / (float)_shelves.Count);
            int dealt = 0;

            foreach (ExpeditionShopShelfUIComponent shelf in _shelves)
            {
                int count = Mathf.Min(perShelf, stock.Count - dealt);

                shelf.Build(stock.GetRange(dealt, count));
                dealt += count;
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

        private void RefreshEquipmentSlots()
        {
            foreach (KeyValuePair<EquipmentSlotTypes, List<ItemSlotUIComponent>> slot in _equipmentItemsDic)
            {
                bool hasItem = _model.TryGetEquippedItemConfig(slot.Key, out ItemConfigSO config,
                    out ItemRarityConfigSO rarity);

                foreach (ItemSlotUIComponent component in slot.Value)
                {
                    if (hasItem)
                    {
                        component.SetItem(config, rarity);
                    }
                    else
                    {
                        component.RemoveItem();
                    }
                }
            }
        }

        /// <summary>Everything priced is re-read together, since one balance gates all of it.</summary>
        private void RefreshAffordability()
        {
            foreach (ExpeditionShopShelfUIComponent shelf in _shelves)
            {
                shelf.RefreshAffordability(_model.CanAfford);
            }

            RefreshRepair();
        }

        private void RefreshRepair()
        {
            _repairButton.interactable = _model.CanAffordRepair;
            _repairButtonText.text = _model.NeedsRepair
                ? string.Format(_repairString, _model.RepairCost)
                : _repairedString;
        }

        /// <summary>Read-only throughout: what is worn and what is offered are both inspected here,
        /// and the buy button is the only thing that changes either.</summary>
        private void HandleOfferSlotClicked(ItemSlotUIComponent slot, InventoryItemEntry item)
        {
            ShowTooltip(slot, item);
        }

        private void HandleEquipmentSlotClicked(EquipmentSlotTypes slot, ItemSlotUIComponent component)
        {
            if (!_model.TryGetEquippedItemForEquipmentSlotType(slot, out InventoryItemEntry equipped))
            {
                return;
            }

            ShowTooltip(component, equipped);
        }

        private void ShowTooltip(ItemSlotUIComponent slot, InventoryItemEntry item)
        {
            _tooltip.ShowReadOnly(slot.RectTransform, item);

            slot.SetSelected(true);
            _selectedSlot = slot;
        }

        private void HandleTooltipHidden()
        {
            if (_selectedSlot != null)
            {
                _selectedSlot.SetSelected(false);
                _selectedSlot = null;
            }
        }

        private void HandleBuyClicked(string instanceId)
        {
            OnBuyClicked?.Invoke(instanceId);
        }
    }
}
