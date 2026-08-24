using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface IDropRollService
    {
        /// <summary>What a kill pays out, which is most often nothing.</summary>
        DropCategoryTypes RollKillCategory();

        /// <summary>What something worth breaking pays out. Never nothing.</summary>
        DropCategoryTypes RollGuaranteedCategory();

        /// <summary>A freshly rolled item and the rarity it came from. False when nothing could be
        /// rolled, which means the tables are unauthored rather than that the player was unlucky.</summary>
        bool TryRollItem(out ItemRarityConfigSO rarityConfig, out InventoryItemEntry item);

        PowerupConfigSO RollPowerup();
    }

    /// <summary>
    /// Decides what drops, and nothing else: no state, no spawning, no messages. Bound only for the
    /// loot manager, which owns where the drop goes and what the run has collected so far.
    /// </summary>
    public class DropRollService : IDropRollService
    {
        [Inject] private readonly IItemsRepository _itemsRepository;
        [Inject] private readonly IPowerupsRepository _powerupsRepository;
        [Inject] private readonly IDropsRepository _dropsRepository;

        public DropCategoryTypes RollKillCategory()
        {
            IReadOnlyList<DropCategoryWeightDTO> weights = _dropsRepository.GetAllDropCategoryWeights();
            DropCategoryWeightDTO winner = GameUtils.RollWeighted(weights, weight => weight.Weight);

            return winner?.Category ?? DropCategoryTypes.None;
        }

        /// <summary>The same table with the "nothing" entry left out, so a category always wins.</summary>
        public DropCategoryTypes RollGuaranteedCategory()
        {
            var candidates = new List<DropCategoryWeightDTO>();

            foreach (DropCategoryWeightDTO weight in _dropsRepository.GetAllDropCategoryWeights())
            {
                if (weight.Category != DropCategoryTypes.None)
                {
                    candidates.Add(weight);
                }
            }

            DropCategoryWeightDTO winner = GameUtils.RollWeighted(candidates, weight => weight.Weight);

            return winner?.Category ?? DropCategoryTypes.None;
        }

        /// <summary>Rarity decides how many affixes are drawn, not how strong they are: the magnitudes
        /// come from each affix's own range on the item template.</summary>
        public bool TryRollItem(out ItemRarityConfigSO rarityConfig, out InventoryItemEntry item)
        {
            item = null;
            rarityConfig = GameUtils.RollWeighted(_itemsRepository.GetAllItemRarityConfigs(), candidate => candidate.DropWeight);

            if (rarityConfig == null)
            {
                return false;
            }

            ItemConfigSO itemConfig = RollItemOfRarity(rarityConfig.Rarity);

            if (itemConfig == null)
            {
                return false;
            }

            item = itemConfig.RollEntry(rarityConfig.AffixCount);
            return true;
        }

        public PowerupConfigSO RollPowerup()
        {
            return GameUtils.RollWeighted(_powerupsRepository.GetAllPowerupConfigs(), candidate => candidate.DropWeight);
        }

        private ItemConfigSO RollItemOfRarity(ItemRarityTypes rarity)
        {
            var candidates = new List<ItemConfigSO>();

            foreach (ItemConfigSO config in _itemsRepository.GetAllItemConfigs())
            {
                if (config.Rarity == rarity)
                {
                    candidates.Add(config);
                }
            }

            if (candidates.Count == 0)
            {
                this.LogWarning($"No item configs authored for rarity '{rarity}'. Skipping drop.");
                return null;
            }

            return GameUtils.RollWeighted(candidates, candidate => candidate.DropWeight);
        }
    }
}
