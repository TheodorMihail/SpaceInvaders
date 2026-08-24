using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface IItemSellService
    {
        bool TryGetSellValue(string instanceId, out int sellValue);
        bool TrySellItem(string instanceId);
    }

    /// <summary>Turns an owned item back into currency.</summary>
    public class ItemSellService : IItemSellService
    {
        [Inject] private readonly IItemStorageService _itemStorage;
        [Inject] private readonly IEquipmentManager _equipmentManager;
        [Inject] private readonly ICurrencyManager _currencyManager;
        [Inject] private readonly IItemsRepository _itemsRepository;
        [Inject] private readonly IMessageBus _messageBus;

        /// <summary>What the item is worth, which its rarity alone decides.</summary>
        public bool TryGetSellValue(string instanceId, out int sellValue)
        {
            sellValue = 0;

            InventoryItemEntry entry = _itemStorage.GetItem(instanceId);
            if (entry == null)
            {
                return false;
            }

            if (!_itemsRepository.TryGetItemConfig(entry.ItemId, out ItemConfigSO itemConfig))
            {
                return false;
            }

            if (!_itemsRepository.TryGetItemRarityConfig(itemConfig.Rarity, out ItemRarityConfigSO rarityConfig))
            {
                return false;
            }

            sellValue = rarityConfig.SellValue;
            return true;
        }

        /// <summary>Unequips before removing, so the slot is cleared while the item is still
        /// resolvable and the equip change is announced rather than silently pruned.</summary>
        public bool TrySellItem(string instanceId)
        {
            if (!TryGetSellValue(instanceId, out int sellValue))
            {
                return false;
            }

            if (_equipmentManager.IsEquipped(instanceId))
            {
                _equipmentManager.Unequip(instanceId);
            }

            _itemStorage.RemoveItem(instanceId);
            _currencyManager.AddCurrency(sellValue);

            _messageBus.Publish(new ItemSoldMessage(instanceId, sellValue));

            return true;
        }
    }
}
