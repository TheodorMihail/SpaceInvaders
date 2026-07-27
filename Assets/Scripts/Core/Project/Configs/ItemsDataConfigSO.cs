using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Project
{
    /// <summary>
    /// The ship's physical equipment slots.
    /// </summary>
    public enum EquipmentSlots
    {
        Wings,
        Engine,
        Weapon,
        Core
    }

    /// <summary>
    /// Binds an equipment slot to the item slot type it accepts.
    /// </summary>
    [Serializable]
    public class EquipmentSlotConfigDTO
    {
        [SerializeField] private EquipmentSlots _slot;
        [SerializeField] private ItemSlotTypes _acceptedType;
        [SerializeField] private string _displayName;

        public EquipmentSlots Slot => _slot;
        public ItemSlotTypes AcceptedType => _acceptedType;
        public string DisplayName => _displayName;

        public EquipmentSlotConfigDTO()
        {
        }

        public EquipmentSlotConfigDTO(EquipmentSlots slot, ItemSlotTypes acceptedType, string displayName)
        {
            _slot = slot;
            _acceptedType = acceptedType;
            _displayName = displayName;
        }
    }

    [CreateAssetMenu(fileName = "ItemsDataConfig", menuName = "SpaceInvaders/Data Config/Items Data Config")]
    public class ItemsDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Items")]
        [SerializeField] private List<ItemConfigSO> _itemConfigs;
        [SerializeField] private List<ItemRarityConfigSO> _rarityConfigs;

        [Header("Equipment")]
        [SerializeField] private List<EquipmentSlotConfigDTO> _slotConfigs;

        [Header("Drop Settings")]
        [SerializeField, Range(0f, 1f)] private float _globalItemDropChance = 0.1f;

        public virtual List<ItemConfigSO> ItemConfigs => _itemConfigs;
        public virtual List<ItemRarityConfigSO> RarityConfigs => _rarityConfigs;
        public virtual IReadOnlyList<EquipmentSlotConfigDTO> SlotConfigs => _slotConfigs ?? new List<EquipmentSlotConfigDTO>();
        public virtual float GlobalItemDropChance => _globalItemDropChance;

        public string ObjectID => nameof(ItemsDataConfigSO);
    }
}
