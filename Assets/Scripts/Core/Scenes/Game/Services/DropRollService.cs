using System;
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

        /// <summary>The payout for a destructible object. Never returns nothing.</summary>
        DropCategoryTypes RollGuaranteedCategory();

        /// <summary>A rolled item and its rarity. False means the tables are unauthored, not that the
        /// roll failed.</summary>
        bool TryRollItem(out ItemRarityConfigSO rarityConfig, out InventoryItemEntry item);

        PowerupConfigSO RollPowerup();
    }

    /// <summary>
    /// Decides what drops, and nothing else: no state, no spawning, no messages. Bound only for the
    /// loot manager, which handles where the drop goes.
    /// </summary>
    public class DropRollService : IDropRollService
    {
        [Inject] private readonly IItemsRepository _itemsRepository;
        [Inject] private readonly IPowerupsRepository _powerupsRepository;
        [Inject] private readonly IGameModeManager _gameModeManager;
        [Inject] private readonly IPlayerManager _playerManager;

        public DropCategoryTypes RollKillCategory()
        {
            DropCategoryWeightDTO winner = GameUtils.RollWeighted(GetCategoryWeights(), weight => weight.Weight);

            return winner?.Category ?? DropCategoryTypes.None;
        }

        /// <summary>The same table with the "nothing" entry left out, so a category always wins.</summary>
        public DropCategoryTypes RollGuaranteedCategory()
        {
            var candidates = new List<DropCategoryWeightDTO>();

            foreach (DropCategoryWeightDTO weight in GetCategoryWeights())
            {
                if (weight.Category != DropCategoryTypes.None)
                {
                    candidates.Add(weight);
                }
            }

            DropCategoryWeightDTO winner = GameUtils.RollWeighted(candidates, weight => weight.Weight);

            return winner?.Category ?? DropCategoryTypes.None;
        }

        /// <summary>Rarity decides how many affixes are drawn. The magnitudes come from each affix's
        /// own range on the item template.</summary>
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

        /// <summary>A powerup the ship would gain nothing from is left out of the roll.</summary>
        public PowerupConfigSO RollPowerup()
        {
            var candidates = new List<PowerupConfigSO>();

            foreach (PowerupConfigSO config in _powerupsRepository.GetAllPowerupConfigs())
            {
                if (!IsRedundant(config))
                {
                    candidates.Add(config);
                }
            }

            return GameUtils.RollWeighted(candidates, candidate => candidate.DropWeight);
        }

        private bool IsRedundant(PowerupConfigSO config)
        {
            return config.PowerupType == PowerupTypes.UnlimitedAmmo
                && (_playerManager.PlayerStats?.HasUnlimitedAmmo ?? false);
        }

        /// <summary>The running mode carries its own weights, so what drops is authored rather than
        /// branched on. A mode that authored none drops nothing.</summary>
        private IReadOnlyList<DropCategoryWeightDTO> GetCategoryWeights()
        {
            return _gameModeManager.DropWeights ?? Array.Empty<DropCategoryWeightDTO>();
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
