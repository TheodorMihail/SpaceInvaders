using System.Collections.Generic;
using System.Text;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class InventoryModel : Model
    {
        [Inject] private readonly IInventoryManager _inventoryManager;
        [Inject] private readonly IEquipmentManager _equipmentManager;
        [Inject] private readonly ITalentManager _talentManager;
        [Inject] private readonly IItemsRepository _itemsRepository;
        [Inject] private readonly IShipsRepository _shipsRepository;
        [Inject] private readonly ICurrencyManager _currencyManager;
        
        public int Currency => _currencyManager.Currency;

        public IEnumerable<(InventoryItemEntry entry, ItemConfigSO config)> GetInventoryItems()
        {
            foreach (InventoryItemEntry entry in _inventoryManager.Items)
            {
                if (!_itemsRepository.TryGetItemConfig(entry.ItemId, out ItemConfigSO config))
                {
                    continue;
                }

                yield return (entry, config);
            }
        }

        public bool TryGetInventoryItem(string instanceId, out (InventoryItemEntry entry, ItemConfigSO config) item)
        {
            item.entry = _inventoryManager.GetItem(instanceId);
            item.config = null;

            if (item.entry == null)
            {
                return false;
            }

            _itemsRepository.TryGetItemConfig(item.entry.ItemId, out ItemConfigSO config);
            item.config = config;

            return item.config != null;
        }

        public bool TryGetEquippedItemForEquipmentSlotType(EquipmentSlotTypes slot, out InventoryItemEntry item)
        {
            item = _equipmentManager.GetEquippedItemForEquipmentSlotType(slot);
            return item != null;
        }

        public bool TryGetEquipmentSlotTypeForItem(InventoryItemEntry entry, out EquipmentSlotTypes? slot)
        {
            slot = _equipmentManager.GetEquipmentSlotTypeForItem(entry.InstanceId);
            return slot != null;
        }

        public ItemRarityConfigSO GetItemRarity(ItemRarityTypes rarity)
        {
            _itemsRepository.TryGetItemRarityConfig(rarity, out ItemRarityConfigSO config);
            return config;
        }

        public bool IsItemEquipped(string instanceId)
        {
            return _equipmentManager.IsEquipped(instanceId);
        }

        /// <summary>
        /// "Health: 100 +20", with the pre-equipment value in white and the equipped items'
        /// contribution in green.
        /// </summary>
        public string StatRowText(ShipUpgradableStatTypes statType, float baseValue, float withEquipmentValue)
        {
            float delta = withEquipmentValue - baseValue;
            string baseText = ShipStats.FormatStatValue(statType, baseValue);
            string deltaText = delta == 0 ? "" : ShipStats.FormatStatDelta(statType, delta);
            return $"• {ShipStats.StatDisplayName(statType)}: <color=white>{baseText}</color> <color=green>{deltaText}</color>";
        }

        public string GetStatsPanel()
        {
            (ShipStats withoutEquipment, ShipStats withEquipment) = BuildComparableStats();
            var builder = new StringBuilder();

            builder.AppendLine(StatRowText(ShipUpgradableStatTypes.Health, withoutEquipment.CurrentMaxHealth, withEquipment.CurrentMaxHealth));
            builder.AppendLine(StatRowText(ShipUpgradableStatTypes.MoveSpeed, withoutEquipment.CurrentMoveSpeed, withEquipment.CurrentMoveSpeed));
            builder.AppendLine(StatRowText(ShipUpgradableStatTypes.FireRate, withoutEquipment.CurrentFireRate, withEquipment.CurrentFireRate));
            builder.AppendLine(StatRowText(ShipUpgradableStatTypes.Damage, withoutEquipment.CurrentProjectileDamage, withEquipment.CurrentProjectileDamage));
            builder.AppendLine(StatRowText(ShipUpgradableStatTypes.ProjectileSpeed, withoutEquipment.CurrentProjectileSpeed, withEquipment.CurrentProjectileSpeed));
            builder.AppendLine(StatRowText(ShipUpgradableStatTypes.CritChance, withoutEquipment.CurrentCritChance, withEquipment.CurrentCritChance));
            builder.AppendLine(StatRowText(ShipUpgradableStatTypes.CritDamage, withoutEquipment.CurrentCritDamage, withEquipment.CurrentCritDamage));
            builder.AppendLine(StatRowText(ShipUpgradableStatTypes.MagazineSize, withoutEquipment.CurrentMaxAmmo, withEquipment.CurrentMaxAmmo));
            builder.AppendLine(StatRowText(ShipUpgradableStatTypes.ReloadSpeed, withoutEquipment.CurrentReloadDuration, withEquipment.CurrentReloadDuration));
            builder.Append(StatRowText(ShipUpgradableStatTypes.PowerupDuration, withoutEquipment.CurrentPowerupDuration, withEquipment.CurrentPowerupDuration));

            return builder.ToString();
        }

        /// <summary>
        /// Builds two stat snapshots, one with talents only and one with talents and equipment, so
        /// the panel can show what the equipped gear adds.
        /// </summary>
        private (ShipStats withoutEquipment, ShipStats withEquipment) BuildComparableStats()
        {
            if (!_shipsRepository.TryGetPlayerConfig(PlayerTypes.Player1, out PlayerSpaceshipConfigSO config))
            {
                var fallbackStats = new ShipStats(new ShipBaseStats());
                return (fallbackStats, fallbackStats);
            }

            ShipStats withoutEquipment = config.CreateStats();
            _talentManager.ApplyTalentBonuses(withoutEquipment);

            ShipStats withEquipment = config.CreateStats();
            _talentManager.ApplyTalentBonuses(withEquipment);
            _equipmentManager.ApplyEquipmentBonuses(withEquipment);

            return (withoutEquipment, withEquipment);
        }
    }
}
