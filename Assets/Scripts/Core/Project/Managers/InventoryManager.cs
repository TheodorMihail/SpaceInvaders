using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;
using UnityEngine.InputSystem;
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

    public class InventoryManager : IInventoryManager, ITickable
    {
        [Inject] private readonly IPersistenceManager _persistenceManager;
        [Inject] private readonly IRepositoryManager _repositoryManager;

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
            if (string.IsNullOrEmpty(itemId))
            {
                return null;
            }

            return _repositoryManager.GetItemConfig(itemId);
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

        #region Debugging

        public void Tick()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.f4Key.wasPressedThisFrame)
            {
                AddRandomItem();
            }

            if (Keyboard.current.f11Key.wasPressedThisFrame)
            {
                _data.Items.Clear();
                SaveData();
                this.LogWarning("Debug: Inventory cleared.");
            }
#endif
        }

        private void AddRandomItem()
        {
            IReadOnlyList<ItemConfigSO> configs = _repositoryManager.GetAllItemConfigs();
            if (configs.Count == 0)
            {
                this.LogWarning("Debug: No item configs authored, nothing to add.");
                return;
            }

            ItemConfigSO config = configs[Random.Range(0, configs.Count)];
            ItemRarityConfigSO rarityConfig = _repositoryManager.GetItemRarityConfig(config.Rarity);
            int affixCount = rarityConfig != null ? rarityConfig.AffixCount : 1;
            InventoryItemEntry entry = config.RollEntry(affixCount);

            AddItems(new List<InventoryItemEntry> { entry });
            this.LogWarning($"Debug: Added '{config.ItemId}' ({config.Rarity}) to inventory.");
        }

        #endregion
    }
}
