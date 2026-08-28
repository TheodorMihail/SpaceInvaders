using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface IItemStorageService
    {
        IReadOnlyList<InventoryItemEntry> Items { get; }

        void Initialize();
        bool ContainsItem(string instanceId);
        InventoryItemEntry GetItem(string instanceId);
        void AddItems(IReadOnlyList<InventoryItemEntry> entries);
        void RemoveItem(string instanceId);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        void ClearAll();
#endif
    }

    /// <summary>Holds the owned items and persists them.</summary>
    public class ItemStorageService : IItemStorageService
    {
        [Inject] private readonly ISaveProfileManager _saveProfileManager;

        private IPersistenceManager _persistenceManager;
        private InventorySaveData _data;

        public IReadOnlyList<InventoryItemEntry> Items => _data.Items;

        public void Initialize()
        {
            _persistenceManager = _saveProfileManager.GetProfile(GameModeTypes.Campaign);
            _data = _persistenceManager.LoadVersioned<InventorySaveData>(InventorySaveData.SaveKey, InventorySaveData.CurrentVersion);
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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void ClearAll()
        {
            _data.Items.Clear();
            SaveData();
        }
#endif

        private void SaveData()
        {
            _persistenceManager.Save(InventorySaveData.SaveKey, _data);
        }
    }
}
