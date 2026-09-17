using System.Collections.Generic;
using SpaceInvaders.Scenes.Expedition;
using SpaceInvaders.Scenes.Game;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface IExpeditionShopService
    {
        /// <summary>One shop's stock. Empty when nothing can be stocked.</summary>
        List<ExpeditionShopOfferEntry> RollOffers();
    }

    /// <summary>
    /// Fills a shelf: a tier per slot by weight, then an item of that tier rolled as a drop would be.
    /// Items may repeat across slots, since each one rolls its own affixes.
    /// </summary>
    public class ExpeditionShopService : IExpeditionShopService
    {
        [Inject] private readonly IItemsRepository _itemsRepository;
        [Inject] private readonly IExpeditionRepository _expeditionRepository;

        public List<ExpeditionShopOfferEntry> RollOffers()
        {
            ExpeditionShopDataConfigSO config = _expeditionRepository.GetShopDataConfig();
            var offers = new List<ExpeditionShopOfferEntry>();

            if (config == null)
            {
                return offers;
            }

            List<ExpeditionShopRarityDTO> stockable = GetStockableRarities(config);

            for (int i = 0; i < config.OfferCount && stockable.Count > 0; i++)
            {
                ExpeditionShopOfferEntry offer = RollOffer(stockable);

                if (offer != null)
                {
                    offers.Add(offer);
                }
            }

            return offers;
        }

        /// <summary>A tier with nothing authored behind it is dropped up front, so a shelf slot is
        /// never spent on one that cannot be filled.</summary>
        private List<ExpeditionShopRarityDTO> GetStockableRarities(ExpeditionShopDataConfigSO config)
        {
            var stockable = new List<ExpeditionShopRarityDTO>();

            foreach (ExpeditionShopRarityDTO rarity in config.Rarities)
            {
                if (HasItemOfRarity(rarity.Rarity))
                {
                    stockable.Add(rarity);
                }
            }

            return stockable;
        }

        /// <summary>Unauthored or zero-weight tiers leave nothing to weigh, so the shelf fills out flat.</summary>
        private ExpeditionShopOfferEntry RollOffer(List<ExpeditionShopRarityDTO> stockable)
        {
            ExpeditionShopRarityDTO rolled = GameUtils.RollWeighted(stockable, candidate => candidate.StockWeight)
                ?? stockable[Random.Range(0, stockable.Count)];

            ItemConfigSO item = RollItemOfRarity(rolled.Rarity);

            if (item == null)
            {
                return null;
            }

            _itemsRepository.TryGetItemRarityConfig(rolled.Rarity, out ItemRarityConfigSO rarityConfig);

            return new ExpeditionShopOfferEntry
            {
                Item = item.RollEntry(rarityConfig == null ? 1 : rarityConfig.AffixCount),
                Price = rolled.Price
            };
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

            return GameUtils.RollWeighted(candidates, candidate => candidate.DropWeight);
        }

        private bool HasItemOfRarity(ItemRarityTypes rarity)
        {
            foreach (ItemConfigSO config in _itemsRepository.GetAllItemConfigs())
            {
                if (config.Rarity == rarity)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
