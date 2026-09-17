using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Expedition
{
    public class ExpeditionShopScreenModel : Model
    {
        [Inject] private readonly IExpeditionRunManager _expeditionRunManager;
        [Inject] private readonly IEquipmentManager _equipmentManager;
        [Inject] private readonly IItemsRepository _itemsRepository;
        [Inject] private readonly ICurrencyManager _currencyManager;

        public int Currency => _currencyManager.Currency;

        public int RepairCost => _expeditionRunManager.CurrentExpedition.RepairCost;

        /// <summary>Tells a whole ship from an unaffordable one, which the button reads differently.</summary>
        public bool NeedsRepair => RepairCost > 0;

        public bool CanAffordRepair => NeedsRepair && RepairCost <= Currency;

        /// <summary>Paired with its template and tier, so a card presents itself without looking
        /// anything up.</summary>
        public IEnumerable<ExpeditionShopOfferDTO> GetOffers()
        {
            foreach (ExpeditionShopOfferEntry offer in _expeditionRunManager.CurrentExpedition.ShopOffers)
            {
                if (offer.Item == null || !_itemsRepository.TryGetItemConfig(offer.Item.ItemId, out ItemConfigSO config))
                {
                    continue;
                }

                yield return new ExpeditionShopOfferDTO(offer.Item, config, GetItemRarity(config.Rarity),
                    offer.Price, offer.IsSold);
            }
        }

        public bool CanAfford(string instanceId)
        {
            ExpeditionShopOfferEntry offer = GetOffer(instanceId);
            return offer != null && offer.Price <= Currency;
        }

        public bool TryGetEquippedItemForEquipmentSlotType(EquipmentSlotTypes slot, out InventoryItemEntry item)
        {
            item = _equipmentManager.GetEquippedItemForEquipmentSlotType(slot);
            return item != null;
        }

        public bool TryGetEquippedItemConfig(EquipmentSlotTypes slot, out ItemConfigSO config, out ItemRarityConfigSO rarity)
        {
            config = null;
            rarity = null;

            if (!TryGetEquippedItemForEquipmentSlotType(slot, out InventoryItemEntry item)
                || !_itemsRepository.TryGetItemConfig(item.ItemId, out config))
            {
                return false;
            }

            rarity = GetItemRarity(config.Rarity);
            return true;
        }

        private ItemRarityConfigSO GetItemRarity(ItemRarityTypes rarity)
        {
            _itemsRepository.TryGetItemRarityConfig(rarity, out ItemRarityConfigSO config);
            return config;
        }

        private ExpeditionShopOfferEntry GetOffer(string instanceId)
        {
            foreach (ExpeditionShopOfferEntry offer in _expeditionRunManager.CurrentExpedition.ShopOffers)
            {
                if (offer.Item?.InstanceId == instanceId)
                {
                    return offer;
                }
            }

            return null;
        }
    }
}
