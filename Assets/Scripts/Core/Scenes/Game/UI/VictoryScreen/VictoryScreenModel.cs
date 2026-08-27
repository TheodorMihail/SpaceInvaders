using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;
using static SpaceInvaders.Scenes.Game.VictoryScreen;

namespace SpaceInvaders.Scenes.Game
{
    public class VictoryScreenModel : Model, IModelWithParams<VictoryScreenParams>
    {
        [Inject] private readonly ILevelProgressManager _levelProgressManager;
        [Inject] private readonly ILevelSessionManager _levelSessionManager;
        [Inject] private readonly ILootManager _lootManager;
        [Inject] private readonly IItemsRepository _itemsRepository;

        public GameOverOptionTypes Options { get; set; } = GameOverOptionTypes.None;

        public int StarsEarned => _levelProgressManager.LastPlayedLevelStarsEarned;
        public int TotalScore => _levelSessionManager.TotalScore;

        public void InitializeWithParameters(VictoryScreenParams parameters)
        {
            Options = parameters.Options;
        }

        public IEnumerable<(InventoryItemEntry entry, ItemConfigSO config, ItemRarityConfigSO rarity)> GetCollectedItems()
        {
            foreach (InventoryItemEntry entry in _lootManager.LastBankedLoot)
            {
                if (!_itemsRepository.TryGetItemConfig(entry.ItemId, out ItemConfigSO config))
                {
                    continue;
                }

                _itemsRepository.TryGetItemRarityConfig(config.Rarity, out ItemRarityConfigSO rarity);
                yield return (entry, config, rarity);
            }
        }
    }
}
