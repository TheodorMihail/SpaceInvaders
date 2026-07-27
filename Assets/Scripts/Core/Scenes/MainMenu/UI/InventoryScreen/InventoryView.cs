using System;
using System.Collections.Generic;
using System.Text;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SpaceInvaders.Scenes.MainMenu
{
    [AddressablePath("Screens/InventoryScreenView")]
    public class InventoryView : View
    {
        [Header("Ship Slots (fixed layout)")]
        [SerializeField] private EquipmentSlotComponent _weaponSlot;
        [SerializeField] private EquipmentSlotComponent _coreSlot;
        [SerializeField] private EquipmentSlotComponent _wingLeftSlot;
        [SerializeField] private EquipmentSlotComponent _wingRightSlot;
        [SerializeField] private EquipmentSlotComponent _engineSlot;

        [Header("Item Grid")]
        [SerializeField] private InventoryItemComponent _itemPrefab;
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private TextMeshProUGUI _emptyInventoryText;

        [Header("Tooltip")]
        [SerializeField] private ItemTooltipComponent _tooltip;

        [Header("Background")]
        [Tooltip("Full-screen, invisible button behind every slot/item/tooltip. Closes the open " +
                 "tooltip when the player clicks anywhere that isn't a slot, item, or the tooltip itself.")]
        [SerializeField] private Button _backgroundCloseButton;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI _statSheetText;
        [SerializeField] private Button _backButton;

        [Inject] private readonly IInventoryManager _inventoryManager;
        [Inject] private readonly IEquipmentManager _equipmentManager;
        [Inject] private readonly ITalentManager _talentManager;
        [Inject] private readonly IRepositoryManager _repositoryManager;
        [Inject] private readonly ICustomFactory _factory;

        private readonly Dictionary<EquipmentSlots, List<EquipmentSlotComponent>> _slotComponents = new();
        private readonly List<InventoryItemComponent> _itemCells = new();

        private InventoryModel _model;
        private RectTransform _activeAnchor;

        public event Action<EquipmentSlots> OnSlotClicked;
        public event Action<string> OnItemClicked;
        public event Action OnTooltipActionClicked;
        public event Action OnTooltipCloseClicked;
        public event Action OnBackClicked;

        private void Awake()
        {
            _backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
            _backgroundCloseButton.onClick.AddListener(() => OnTooltipCloseClicked?.Invoke());
            _tooltip.OnActionClicked += () => OnTooltipActionClicked?.Invoke();

            BuildSlotMap();
        }

        public void Setup(InventoryModel model)
        {
            _model = model;
            Refresh();
        }

        public void Refresh()
        {
            RefreshSlots();
            RefreshItems();
            RefreshStatsPanel();
            RefreshTooltip();
        }

        /// <summary>
        /// Registers the 5 hand-placed slot visuals against their logical EquipmentSlots. Both
        /// wing boxes map to the same Wings value, so they're refreshed together and both fire
        /// the same logical slot on click; only the anchor differs per physical box.
        /// </summary>
        private void BuildSlotMap()
        {
            RegisterSlotComponent(EquipmentSlots.Weapon, _weaponSlot);
            RegisterSlotComponent(EquipmentSlots.Core, _coreSlot);
            RegisterSlotComponent(EquipmentSlots.Wings, _wingLeftSlot);
            RegisterSlotComponent(EquipmentSlots.Wings, _wingRightSlot);
            RegisterSlotComponent(EquipmentSlots.Engine, _engineSlot);
        }

        private void RegisterSlotComponent(EquipmentSlots slot, EquipmentSlotComponent component)
        {
            if (!_slotComponents.TryGetValue(slot, out List<EquipmentSlotComponent> components))
            {
                components = new List<EquipmentSlotComponent>();
                _slotComponents[slot] = components;
            }

            components.Add(component);
            component.OnSlotClicked += _ => HandleSlotClicked(slot, component);
        }

        private void RefreshSlots()
        {
            foreach (EquipmentSlotConfigDTO slotConfig in _equipmentManager.SlotConfigs)
            {
                if (!_slotComponents.TryGetValue(slotConfig.Slot, out List<EquipmentSlotComponent> components))
                {
                    continue;
                }

                InventoryItemEntry equipped = _equipmentManager.GetEquippedItem(slotConfig.Slot);
                ItemConfigSO config = _inventoryManager.GetItemConfig(equipped);
                ItemRarityConfigSO rarityConfig = config != null ? _repositoryManager.GetItemRarityConfig(config.Rarity) : null;
                bool isTooltipOpen = _model.OpenSlot == slotConfig.Slot;

                foreach (EquipmentSlotComponent component in components)
                {
                    component.Setup(slotConfig, config, rarityConfig, isTooltipOpen);
                }
            }
        }

        private void RefreshItems()
        {
            foreach (InventoryItemComponent cell in _itemCells)
            {
                cell.OnItemClicked -= HandleItemClicked;
                Destroy(cell.gameObject);
            }

            _itemCells.Clear();

            foreach (InventoryItemEntry entry in _inventoryManager.Items)
            {
                ItemConfigSO config = _inventoryManager.GetItemConfig(entry);
                if (config == null)
                {
                    continue;
                }

                InventoryItemComponent cell = _factory.CreateFromPrefab(_itemPrefab, _itemsContainer);
                cell.OnItemClicked += HandleItemClicked;

                ItemRarityConfigSO rarityConfig = _repositoryManager.GetItemRarityConfig(config.Rarity);
                bool isSelected = _model.OpenItemInstanceId == entry.InstanceId;
                cell.Setup(entry, config, rarityConfig, _equipmentManager.IsEquipped(entry.InstanceId), isSelected);

                _itemCells.Add(cell);
            }

            _emptyInventoryText.gameObject.SetActive(_itemCells.Count == 0);
            _emptyInventoryText.text = _model.EmptyInventoryText;
        }

        private void RefreshStatsPanel()
        {
            (ShipStats withoutEquipment, ShipStats withEquipment) = BuildComparableStats();
            var builder = new StringBuilder();

            builder.AppendLine(_model.StatRowText(ShipUpgradableStatTypes.Health, withoutEquipment.CurrentMaxHealth, withEquipment.CurrentMaxHealth));
            builder.AppendLine(_model.StatRowText(ShipUpgradableStatTypes.MoveSpeed, withoutEquipment.CurrentMoveSpeed, withEquipment.CurrentMoveSpeed));
            builder.AppendLine(_model.StatRowText(ShipUpgradableStatTypes.FireRate, withoutEquipment.CurrentFireRate, withEquipment.CurrentFireRate));
            builder.AppendLine(_model.StatRowText(ShipUpgradableStatTypes.Damage, withoutEquipment.CurrentProjectileDamage, withEquipment.CurrentProjectileDamage));
            builder.Append(_model.StatRowText(ShipUpgradableStatTypes.ProjectileSpeed, withoutEquipment.CurrentProjectileSpeed, withEquipment.CurrentProjectileSpeed));

            _statSheetText.text = builder.ToString();
        }

        /// <summary>
        /// Builds two independent stat snapshots - one with only talents applied, one with
        /// talents and equipment - so the stat sheet can show the base value and isolate exactly
        /// what the currently equipped gear contributes on top of it.
        /// </summary>
        private (ShipStats withoutEquipment, ShipStats withEquipment) BuildComparableStats()
        {
            PlayerSpaceshipConfigSO config = _repositoryManager.GetPlayerConfig(PlayerTypes.Player1);

            ShipStats withoutEquipment = config.CreateStats();
            _talentManager.ApplyTalentBonuses(withoutEquipment);

            ShipStats withEquipment = config.CreateStats();
            _talentManager.ApplyTalentBonuses(withEquipment);
            _equipmentManager.ApplyEquipmentBonuses(withEquipment);

            return (withoutEquipment, withEquipment);
        }

        private void RefreshTooltip()
        {
            if (_activeAnchor == null)
            {
                _tooltip.Hide();
                return;
            }

            if (_model.OpenItemInstanceId != null)
            {
                ShowItemTooltip(_model.OpenItemInstanceId);
                return;
            }

            if (_model.OpenSlot != null)
            {
                ShowSlotTooltip(_model.OpenSlot.Value);
                return;
            }

            _tooltip.Hide();
        }

        private void ShowItemTooltip(string instanceId)
        {
            InventoryItemEntry entry = _inventoryManager.GetItem(instanceId);
            ItemConfigSO config = entry != null ? _inventoryManager.GetItemConfig(entry) : null;
            if (entry == null || config == null)
            {
                _tooltip.Hide();
                return;
            }

            ItemRarityConfigSO rarityConfig = _repositoryManager.GetItemRarityConfig(config.Rarity);
            string rarityText = rarityConfig != null ? rarityConfig.DisplayName : config.Rarity.ToString();
            _tooltip.Show(_activeAnchor, config.DisplayName, rarityText, BuildAffixesText(entry), _model.EquipActionLabel, showAction: true);
        }

        /// <summary>
        /// Empty slots aren't clickable (EquipmentSlotComponent disables their button), so OpenSlot
        /// can only ever hold a slot that has an equipped item.
        /// </summary>
        private void ShowSlotTooltip(EquipmentSlots slot)
        {
            InventoryItemEntry equipped = _equipmentManager.GetEquippedItem(slot);
            if (equipped == null)
            {
                _tooltip.Hide();
                return;
            }

            ItemConfigSO config = _inventoryManager.GetItemConfig(equipped);
            ItemRarityConfigSO rarityConfig = _repositoryManager.GetItemRarityConfig(config.Rarity);
            string rarityText = rarityConfig != null ? rarityConfig.DisplayName : config.Rarity.ToString();
            _tooltip.Show(_activeAnchor, config.DisplayName, rarityText, BuildAffixesText(equipped), _model.UnequipActionLabel, showAction: true);
        }

        private string BuildAffixesText(InventoryItemEntry entry)
        {
            var builder = new StringBuilder();

            foreach (RolledAffixEntry affix in entry.Affixes)
            {
                if (!Enum.TryParse(affix.StatType, out ShipUpgradableStatTypes statType))
                {
                    continue;
                }

                builder.AppendLine(_model.AffixFormat(statType, affix.Bonus));
            }

            return builder.ToString().TrimEnd();
        }

        private void HandleSlotClicked(EquipmentSlots slot, EquipmentSlotComponent clickedComponent)
        {
            _activeAnchor = clickedComponent.RectTransform;
            OnSlotClicked?.Invoke(slot);
        }

        private void HandleItemClicked(string instanceId)
        {
            InventoryItemComponent cell = _itemCells.Find(c => c.InstanceId == instanceId);
            _activeAnchor = cell != null ? cell.RectTransform : null;
            OnItemClicked?.Invoke(instanceId);
        }
    }
}
