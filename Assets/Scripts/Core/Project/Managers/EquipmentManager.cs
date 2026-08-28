using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface IEquipmentManager : IInitializable
    {
        IReadOnlyList<EquipmentSlotConfigDTO> EquipmentSlotConfigs { get; }

        InventoryItemEntry GetEquippedItemForEquipmentSlotType(EquipmentSlotTypes slot);
        EquipmentSlotTypes? GetEquipmentSlotTypeForItem(string instanceId);
        bool IsEquipped(string instanceId);
        void Equip(string instanceId);
        void Unequip(string instanceId);
        void ApplyEquipmentBonuses(ShipStats stats);
    }

    /// <summary>Holds which item occupies each equipment slot and applies their bonuses.</summary>
    public partial class EquipmentManager : IEquipmentManager
    {
        [Inject] private readonly ISaveProfileManager _saveProfileManager;

        private IPersistenceManager _persistenceManager;
        [Inject] private readonly IItemsRepository _itemsRepository;
        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly IInventoryManager _inventoryManager;

        private EquipmentSaveData _data;

        public IReadOnlyList<EquipmentSlotConfigDTO> EquipmentSlotConfigs => _itemsRepository.GetAllEquipmentSlotConfigs();

        public void Initialize()
        {
            _persistenceManager = _saveProfileManager.GetProfile(GameModeTypes.Campaign);
            _data = _persistenceManager.LoadVersioned<EquipmentSaveData>(EquipmentSaveData.SaveKey, EquipmentSaveData.CurrentVersion);
        }

        /// <summary>Removes the slot entry if the referenced item is no longer owned. Validated on
        /// read, since save load order is not fixed.</summary>
        public InventoryItemEntry GetEquippedItemForEquipmentSlotType(EquipmentSlotTypes slot)
        {
            EquippedSlotEntry slotEntry = GetEquipmentSlotEntry(slot);
            if (slotEntry == null)
            {
                return null;
            }

            InventoryItemEntry item = _inventoryManager.GetItem(slotEntry.InstanceId);
            if (item == null)
            {
                _data.Slots.Remove(slotEntry);
                SaveData();
                return null;
            }

            return item;
        }

        public EquipmentSlotTypes? GetEquipmentSlotTypeForItem(string instanceId)
        {
            InventoryItemEntry item = _inventoryManager.GetItem(instanceId);
            if (item == null)
            {
                return null;
            }

            if (!_itemsRepository.TryGetItemConfig(item.ItemId, out ItemConfigSO config))
            {
                return null;
            }

            EquipmentSlotConfigDTO slotConfig = GetEquipmentSlotConfigForItemType(config.SlotType);
            return slotConfig?.Slot;
        }

        public bool IsEquipped(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
            {
                return false;
            }

            return _data.Slots.Exists(slot => slot.InstanceId == instanceId);
        }

        public void Equip(string instanceId)
        {
            EquipmentSlotTypes? slot = GetEquipmentSlotTypeForItem(instanceId);
            if (slot == null)
            {
                return;
            }

            // Anything already in the target slot is unequipped, unless it is this same instance.
            EquippedSlotEntry previousEquippedItem = GetEquipmentSlotEntry(slot.Value);
            string previousEquippedInstanceId = previousEquippedItem != null && previousEquippedItem.InstanceId != instanceId
                ? previousEquippedItem.InstanceId
                : null;

            // The same instance can only occupy one slot, so clear the slot it currently uses.
            EquippedSlotEntry previousSlot = _data.Slots.Find(s => s.InstanceId == instanceId);
            if (previousSlot != null)
            {
                _data.Slots.Remove(previousSlot);
            }

            if (previousEquippedItem != null)
            {
                _data.Slots.Remove(previousEquippedItem);
            }

            _data.Slots.Add(new EquippedSlotEntry { Slot = slot.Value.ToString(), InstanceId = instanceId });
            SaveData();

            _messageBus.Publish(new ItemEquipChangedMessage(instanceId, previousEquippedInstanceId));
        }

        public void Unequip(string instanceId)
        {
            EquippedSlotEntry slotEntry = _data.Slots.Find(s => s.InstanceId == instanceId);
            if (slotEntry == null)
            {
                return;
            }

            _data.Slots.Remove(slotEntry);
            SaveData();

            _messageBus.Publish(new ItemEquipChangedMessage(null, instanceId));
        }

        public void ApplyEquipmentBonuses(ShipStats stats)
        {
            foreach (EquipmentSlotConfigDTO slotConfig in EquipmentSlotConfigs)
            {
                InventoryItemEntry entry = GetEquippedItemForEquipmentSlotType(slotConfig.Slot);
                if (entry == null)
                {
                    continue;
                }

                foreach (AffixEntry affix in entry.Affixes)
                {
                    if (!Enum.TryParse(affix.StatType, out ShipUpgradableStatTypes statType))
                    {
                        this.LogWarning($"Unknown stat type '{affix.StatType}' on item '{entry.ItemId}'. Skipping.");
                        continue;
                    }

                    // Empty ValueType means this affix was persisted before the field existed -
                    // default to Flat rather than dropping the bonus for every pre-existing save.
                    ShipStatValueTypes valueType = ShipStatValueTypes.Flat;
                    if (!string.IsNullOrEmpty(affix.ValueType) && !Enum.TryParse(affix.ValueType, out valueType))
                    {
                        this.LogWarning($"Unknown value type '{affix.ValueType}' on item '{entry.ItemId}'. Skipping.");
                        continue;
                    }

                    stats.ApplyStatBonus(statType, affix.Bonus, valueType);
                }
            }

            stats.RefillHealth();
            stats.RefillAmmo();
        }

        private EquippedSlotEntry GetEquipmentSlotEntry(EquipmentSlotTypes slot)
        {
            return _data.Slots.Find(s => s.Slot == slot.ToString());
        }

        private EquipmentSlotConfigDTO GetEquipmentSlotConfigForItemType(ItemSlotTypes itemType)
        {
            foreach (EquipmentSlotConfigDTO slotConfig in EquipmentSlotConfigs)
            {
                if (slotConfig.AcceptedType == itemType)
                {
                    return slotConfig;
                }
            }

            return null;
        }

        private void SaveData()
        {
            _persistenceManager.Save(EquipmentSaveData.SaveKey, _data);
        }
    }
}
