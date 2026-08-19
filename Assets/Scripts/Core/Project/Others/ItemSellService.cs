using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface IItemSellService
    {
        bool TryGetSellValue(string instanceId, out int sellValue);
        bool TrySellItem(string instanceId);
    }

    /// <summary>
    /// Turns an owned item back into currency. Owns nothing itself: it only sequences the three
    /// managers that hold the pieces, so none of them has to know about the others.
    /// </summary>
    public class ItemSellService : IItemSellService
    {
        [Inject] private readonly IInventoryManager _inventoryManager;
        [Inject] private readonly IEquipmentManager _equipmentManager;
        [Inject] private readonly ICurrencyManager _currencyManager;
        [Inject] private readonly IItemsRepository _itemsRepository;
        [Inject] private readonly IMessageBus _messageBus;

        /// <summary>What the item is worth, which its rarity alone decides.</summary>
        public bool TryGetSellValue(string instanceId, out int sellValue)
        {
            sellValue = 0;

            InventoryItemEntry entry = _inventoryManager.GetItem(instanceId);
            if (entry == null)
            {
                return false;
            }

            ItemConfigSO itemConfig = _inventoryManager.GetItemConfig(entry.ItemId);
            if (itemConfig == null)
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

        /// <summary>Unequips before removing, since the slot can only be cleared while the item is
        /// still resolvable through the inventory.</summary>
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

            _inventoryManager.RemoveItem(instanceId);
            _currencyManager.AddCurrency(sellValue);

            _messageBus.Publish(new ItemSoldMessage(instanceId, sellValue));

            return true;
        }
    }
}
