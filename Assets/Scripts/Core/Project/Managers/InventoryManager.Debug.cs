#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Project
{
    public partial class InventoryManager : IDebugCommandProvider
    {
        public IReadOnlyList<DebugCommand> GetDebugCommands()
        {
            return new[]
            {
                new DebugCommand(DebugKeys.AddRandomItem, "Add random item", DebugAddRandomItem),
                new DebugCommand(DebugKeys.ClearInventory, "Clear inventory", DebugClearInventory)
            };
        }

        private void DebugAddRandomItem()
        {
            IReadOnlyList<ItemConfigSO> configs = _itemsRepository.GetAllItemConfigs();
            if (configs.Count == 0)
            {
                this.LogWarning("Debug: No item configs authored, nothing to add.");
                return;
            }

            ItemConfigSO config = configs[Random.Range(0, configs.Count)];
            bool hasRarityConfig = _itemsRepository.TryGetItemRarityConfig(config.Rarity, out ItemRarityConfigSO rarityConfig);
            int affixCount = hasRarityConfig ? rarityConfig.AffixCount : 1;
            InventoryItemEntry entry = config.RollEntry(affixCount);

            AddItems(new List<InventoryItemEntry> { entry });
            this.LogWarning($"Debug: Added '{config.ItemId}' ({config.Rarity}) to inventory.");
        }

        private void DebugClearInventory()
        {
            _data.Items.Clear();
            SaveData();
            this.LogWarning("Debug: Inventory cleared.");
        }
    }
}
#endif
