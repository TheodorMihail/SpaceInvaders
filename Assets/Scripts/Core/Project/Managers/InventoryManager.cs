using System.Collections.Generic;
using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface IInventoryManager : IInitializable
    {
        IReadOnlyList<InventoryItemEntry> Items { get; }
        bool ContainsItem(string instanceId);
        InventoryItemEntry GetItem(string instanceId);
        ItemConfigSO GetItemConfig(string itemId);
        void AddItems(IReadOnlyList<InventoryItemEntry> entries);
        void RemoveItem(string instanceId);
    }

    public partial class InventoryManager : IInventoryManager
    {
        [Inject] private readonly IPersistenceManager _persistenceManager;
        [Inject] private readonly IItemsRepository _itemsRepository;

        private InventorySaveData _data;

        public IReadOnlyList<InventoryItemEntry> Items => _data.Items;

        public void Initialize()
        {
            _data = _persistenceManager.Load<InventorySaveData>(InventorySaveData.SaveKey);
        }

        public bool ContainsItem(string instanceId)
        {
            return GetItem(instanceId) != null;
        }

        public InventoryItemEntry GetItem(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
            {
                return null;
            }

            return _data.Items.Find(item => item.InstanceId == instanceId);
        }

        public ItemConfigSO GetItemConfig(string itemId)
        {
            _itemsRepository.TryGetItemConfig(itemId, out ItemConfigSO config);
            return config;
        }

        public void AddItems(IReadOnlyList<InventoryItemEntry> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return;
            }

            _data.Items.AddRange(entries);
            SaveData();
        }

        public void RemoveItem(string instanceId)
        {
            InventoryItemEntry entry = GetItem(instanceId);
            if (entry == null)
            {
                return;
            }

            _data.Items.Remove(entry);
            SaveData();
        }

        private void SaveData()
        {
            _persistenceManager.Save(InventorySaveData.SaveKey, _data);
        }
    }
}
