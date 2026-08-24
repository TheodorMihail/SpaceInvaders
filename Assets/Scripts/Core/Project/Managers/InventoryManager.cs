using System.Collections.Generic;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface IInventoryManager : IInitializable
    {
        IReadOnlyList<InventoryItemEntry> Items { get; }

        bool ContainsItem(string instanceId);
        InventoryItemEntry GetItem(string instanceId);
        void AddItems(IReadOnlyList<InventoryItemEntry> entries);
        void RemoveItem(string instanceId);

        bool TryGetSellValue(string instanceId, out int sellValue);
        bool TrySellItem(string instanceId);
    }

    /// <summary>Owns every item the player has, equipped ones included.</summary>
    public partial class InventoryManager : IInventoryManager
    {
        [Inject] private readonly IItemStorageService _itemStorage;
        [Inject] private readonly IItemSellService _itemSellService;

        public IReadOnlyList<InventoryItemEntry> Items => _itemStorage.Items;

        public void Initialize()
        {
            _itemStorage.Initialize();
        }

        public bool ContainsItem(string instanceId)
        {
            return _itemStorage.ContainsItem(instanceId);
        }

        public InventoryItemEntry GetItem(string instanceId)
        {
            return _itemStorage.GetItem(instanceId);
        }

        public void AddItems(IReadOnlyList<InventoryItemEntry> entries)
        {
            _itemStorage.AddItems(entries);
        }

        public void RemoveItem(string instanceId)
        {
            _itemStorage.RemoveItem(instanceId);
        }

        public bool TryGetSellValue(string instanceId, out int sellValue)
        {
            return _itemSellService.TryGetSellValue(instanceId, out sellValue);
        }

        public bool TrySellItem(string instanceId)
        {
            return _itemSellService.TrySellItem(instanceId);
        }
    }
}
