using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class LevelFinishedModel : Model
    {
        [Inject] private readonly ILevelManager _levelManager;
        [Inject] private readonly IScoreService _scoreService;
        [Inject] private readonly ILootManager _lootManager;
        [Inject] private readonly IInventoryManager _inventoryManager;
        [Inject] private readonly IRepositoryManager _repositoryManager;

        public bool AllLevelsComplete => _levelManager.CurrentLevelNumber >= _levelManager.MaxLevelNumber;
        public int StarsEarned => _levelManager.LastPlayedLevelStarsEarned;
        public int TotalScore => _scoreService.TotalScore;

        public IEnumerable<(InventoryItemEntry entry, ItemConfigSO config, ItemRarityConfigSO rarity)> GetCollectedItems()
        {
            foreach (InventoryItemEntry entry in _lootManager.LastBankedLoot)
            {
                ItemConfigSO config = _inventoryManager.GetItemConfig(entry.ItemId);
                if (config == null)
                {
                    continue;
                }

                ItemRarityConfigSO rarity = _repositoryManager.GetItemRarityConfig(config.Rarity);
                yield return (entry, config, rarity);
            }
        }
    }
}
