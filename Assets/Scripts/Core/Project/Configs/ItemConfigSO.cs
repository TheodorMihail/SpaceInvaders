using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using UnityEngine;

namespace SpaceInvaders.Project
{
    public enum ItemSlotTypes
    {
        Wings,
        Engine,
        Weapon,
        Core
    }

    public enum ItemRarities
    {
        Normal,
        Rare,
        Legendary
    }

    /// <summary>
    /// A stat bonus an item can roll, with the range it rolls within.
    /// </summary>
    [Serializable]
    public class ItemAffixDTO
    {
        [SerializeField] private ShipUpgradableStatTypes _statType;
        [SerializeField] private float _minBonus;
        [SerializeField] private float _maxBonus;

        public ShipUpgradableStatTypes StatType => _statType;
        public float MinBonus => _minBonus;
        public float MaxBonus => _maxBonus;

        public ItemAffixDTO()
        {
        }

        public ItemAffixDTO(ShipUpgradableStatTypes statType, float minBonus, float maxBonus)
        {
            _statType = statType;
            _minBonus = minBonus;
            _maxBonus = maxBonus;
        }
    }

    /// <summary>
    /// Template for a droppable item. The affixes here are the pool a drop rolls from;
    /// the item's rarity decides how many are drawn.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemConfig", menuName = "SpaceInvaders/Items/Item Config")]
    public class ItemConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Item Settings")]
        [SerializeField] private string _itemId;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private ItemSlotTypes _slotType;
        [SerializeField] private ItemRarities _rarity;
        [SerializeField] private int _dropWeight = 1;

        [Header("Affix Pool")]
        [SerializeField] private List<ItemAffixDTO> _possibleAffixes;

        public virtual string ItemId => _itemId;
        public virtual string DisplayName => _displayName;
        public virtual Sprite Icon => _icon;
        public virtual ItemSlotTypes SlotType => _slotType;
        public virtual ItemRarities Rarity => _rarity;
        public virtual int DropWeight => _dropWeight;
        public virtual IReadOnlyList<ItemAffixDTO> PossibleAffixes => _possibleAffixes;

        public virtual string ObjectID => _itemId;
    }
}
