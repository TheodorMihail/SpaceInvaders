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

        /// <summary>Rolls a fresh owned instance of this item: a new instance id and up to
        /// affixCount distinct affixes drawn from the pool. Shared by real drops (LootManager)
        /// and the debug "add random item" cheat (InventoryManager), so both stay in sync.</summary>
        public InventoryItemEntry RollEntry(int affixCount)
        {
            var entry = new InventoryItemEntry
            {
                InstanceId = Guid.NewGuid().ToString(),
                ItemId = _itemId
            };

            var pool = new List<ItemAffixDTO>(PossibleAffixes ?? new List<ItemAffixDTO>());
            int rollCount = Mathf.Min(affixCount, pool.Count);

            if (pool.Count < affixCount)
            {
                this.LogWarning($"Item '{_itemId}' has {pool.Count} affixes but rolled for {affixCount}. Rolling what is available.");
            }

            for (int i = 0; i < rollCount; i++)
            {
                int index = UnityEngine.Random.Range(0, pool.Count);
                ItemAffixDTO affix = pool[index];
                pool.RemoveAt(index);

                entry.Affixes.Add(new RolledAffixEntry
                {
                    StatType = affix.StatType.ToString(),
                    Bonus = UnityEngine.Random.Range(affix.MinBonus, affix.MaxBonus)
                });
            }

            return entry;
        }
    }
}
