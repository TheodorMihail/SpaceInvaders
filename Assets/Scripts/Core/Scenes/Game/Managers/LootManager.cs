using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Project;
using Zenject;
using Random = UnityEngine.Random;

namespace SpaceInvaders.Scenes.Game
{
    public interface ILootManager : IDisposable, IInitializable, IGameEndListener
    {
        void CollectItem(InventoryItemEntry item);
    }

    /// <summary>
    /// Rolls item drops on enemy kills and holds what the player picked up this run.
    /// Pending loot is only committed to the inventory when the level is completed.
    /// </summary>
    public class LootManager : ILootManager
    {
        [Inject] private readonly IRepositoryManager _repositoryManager;
        [Inject] private readonly IInventoryManager _inventoryManager;
        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly ISpawnService _spawnService;

        private readonly List<InventoryItemEntry> _pendingLoot = new();

        public void Initialize()
        {
            _messageBus.Subscribe<EnemyDestroyedMessage>(OnEnemyDestroyed);
            _messageBus.Subscribe<LevelCompletedMessage>(OnLevelCompleted);
        }

        public void Dispose()
        {
            _messageBus.Unsubscribe<EnemyDestroyedMessage>(OnEnemyDestroyed);
            _messageBus.Unsubscribe<LevelCompletedMessage>(OnLevelCompleted);
            _pendingLoot.Clear();
        }

        public UniTask GameEnd()
        {
            // Loot that was not banked by completing the level is lost.
            _pendingLoot.Clear();
            return UniTask.CompletedTask;
        }

        public void CollectItem(InventoryItemEntry item)
        {
            if (item == null)
            {
                return;
            }

            _pendingLoot.Add(item);

            ItemConfigSO config = _repositoryManager.GetItemConfig(item.ItemId);
            if (config == null)
            {
                return;
            }

            _messageBus.Publish(new ItemCollectedMessage(item.InstanceId));
        }

        private void OnEnemyDestroyed(EnemyDestroyedMessage message)
        {
            if (!TryRollItemDrop(out InventoryItemEntry item, out ItemRarityConfigSO rarityConfig))
            {
                return;
            }

            _spawnService.SpawnItemPickup(rarityConfig, item, message.Position);
            _messageBus.Publish(new ItemDroppedMessage(item.InstanceId, message.Position));
        }

        private void OnLevelCompleted(LevelCompletedMessage message)
        {
            if (_pendingLoot.Count == 0)
            {
                return;
            }

            var bankedLoot = new List<InventoryItemEntry>(_pendingLoot);
            _pendingLoot.Clear();
            _inventoryManager.AddItems(bankedLoot);
        }

#region  Drop Rolls

        private bool TryRollItemDrop(out InventoryItemEntry item, out ItemRarityConfigSO rarityConfig)
        {
            item = null;
            rarityConfig = null;

            if (Random.value > _repositoryManager.GetItemDropChance())
            {
                return false;
            }

            rarityConfig = RollRarity();
            if (rarityConfig == null)
            {
                return false;
            }

            ItemConfigSO itemConfig = RollItem(rarityConfig.Rarity);
            if (itemConfig == null)
            {
                return false;
            }

            item = itemConfig.RollEntry(rarityConfig.AffixCount);
            return true;
        }

        private ItemRarityConfigSO RollRarity()
        {
            IReadOnlyList<ItemRarityConfigSO> rarities = _repositoryManager.GetAllItemRarityConfigs();

            int totalWeight = 0;
            foreach (ItemRarityConfigSO candidate in rarities)
            {
                totalWeight += candidate.DropWeight;
            }

            if (totalWeight <= 0)
            {
                return null;
            }

            int roll = Random.Range(0, totalWeight);

            foreach (ItemRarityConfigSO candidate in rarities)
            {
                roll -= candidate.DropWeight;
                if (roll < 0)
                {
                    return candidate;
                }
            }

            return null;
        }

        private ItemConfigSO RollItem(ItemRarityTypes rarity)
        {
            var candidates = new List<ItemConfigSO>();

            foreach (ItemConfigSO config in _repositoryManager.GetAllItemConfigs())
            {
                if (config.Rarity == rarity)
                {
                    candidates.Add(config);
                }
            }

            if (candidates.Count == 0)
            {
                this.LogWarning($"No item configs authored for rarity '{rarity}'. Skipping drop.");
                return null;
            }

            int totalWeight = 0;
            foreach (ItemConfigSO candidate in candidates)
            {
                totalWeight += candidate.DropWeight;
            }

            if (totalWeight <= 0)
            {
                return null;
            }

            int roll = Random.Range(0, totalWeight);

            foreach (ItemConfigSO candidate in candidates)
            {
                roll -= candidate.DropWeight;
                if (roll < 0)
                {
                    return candidate;
                }
            }

            return null;
        }
        
        #endregion
    }
}
