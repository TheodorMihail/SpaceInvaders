using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Project;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface ILootManager : IDisposable, IInitializable, IGameEndListener
    {
        IReadOnlyList<InventoryItemEntry> LastBankedLoot { get; }
        void CollectItem(InventoryItemEntry item);
    }

    /// <summary>
    /// Decides what, if anything, drops on an enemy kill - exactly one category (Powerup, Item,
    /// or none) wins a weighted roll, so a single kill can never drop both. Also holds what the
    /// player picked up this run; pending loot is only committed to the inventory when the level
    /// is completed.
    /// </summary>
    public class LootManager : ILootManager
    {
        [Inject] private readonly IItemsRepository _itemsRepository;
        [Inject] private readonly IPowerupsRepository _powerupsRepository;
        [Inject] private readonly IDropsRepository _dropsRepository;
        [Inject] private readonly IInventoryManager _inventoryManager;
        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly ISpawnService _spawnService;

        /// <summary>Loot collected during the current run. Added to the inventory only on level completion.</summary>
        private readonly List<InventoryItemEntry> _pendingLoot = new();
        private IReadOnlyList<InventoryItemEntry> _lastBankedLoot = Array.Empty<InventoryItemEntry>();

        public IReadOnlyList<InventoryItemEntry> LastBankedLoot => _lastBankedLoot;

        public void Initialize()
        {
            _messageBus.Subscribe<EnemyDestroyedMessage>(OnEnemyDestroyed);
            _messageBus.Subscribe<HazardDestroyedMessage>(OnHazardDestroyed);
            _messageBus.Subscribe<LevelCompletedMessage>(OnLevelCompleted);
        }

        public void Dispose()
        {
            _messageBus.Unsubscribe<EnemyDestroyedMessage>(OnEnemyDestroyed);
            _messageBus.Unsubscribe<HazardDestroyedMessage>(OnHazardDestroyed);
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

            if (!_itemsRepository.TryGetItemConfig(item.ItemId, out ItemConfigSO config))
            {
                return;
            }

            _messageBus.Publish(new ItemCollectedMessage(item.InstanceId, config.Rarity));
        }

        private void OnEnemyDestroyed(EnemyDestroyedMessage message)
        {
            SpawnDrop(RollDropCategory(), message.LocalPosition);
        }

        /// <summary>A hazard costs the player something to break, so it always pays out.</summary>
        private void OnHazardDestroyed(HazardDestroyedMessage message)
        {
            SpawnDrop(RollGuaranteedDropCategory(), message.LocalPosition);
        }

        private void SpawnDrop(DropCategoryTypes category, Vector3 localPosition)
        {
            switch (category)
            {
                case DropCategoryTypes.Powerup:
                {
                    SpawnPowerupDrop(localPosition);
                    break;
                }
                case DropCategoryTypes.Item:
                {
                    SpawnItemDrop(localPosition);
                    break;
                }
            }
        }

        private void SpawnItemDrop(Vector3 localPosition)
        {
            ItemRarityConfigSO rarityConfig = RollRarity();
            if (rarityConfig == null)
            {
                return;
            }

            ItemConfigSO itemConfig = RollItem(rarityConfig.Rarity);
            if (itemConfig == null)
            {
                return;
            }

            InventoryItemEntry item = itemConfig.RollEntry(rarityConfig.AffixCount);

            _spawnService.SpawnItemPickup(rarityConfig, item, localPosition);
            _messageBus.Publish(new ItemDroppedMessage(item.InstanceId, localPosition));
        }

        private void SpawnPowerupDrop(Vector3 localPosition)
        {
            PowerupConfigSO config = GameUtils.RollWeighted(_powerupsRepository.GetAllPowerupConfigs(), candidate => candidate.DropWeight);
            if (config == null)
            {
                return;
            }

            _spawnService.SpawnPowerup(config, localPosition);
            _messageBus.Publish(new PowerupDroppedMessage(config.PowerupType, localPosition));
        }

        private void OnLevelCompleted(LevelCompletedMessage message)
        {
            if (_pendingLoot.Count == 0)
            {
                return;
            }

            var bankedLoot = new List<InventoryItemEntry>(_pendingLoot);
            _pendingLoot.Clear();
            _lastBankedLoot = bankedLoot;
            _inventoryManager.AddItems(bankedLoot);
        }

        #region Drop Rolls

        private DropCategoryTypes RollDropCategory()
        {
            IReadOnlyList<DropCategoryWeightDTO> weights = _dropsRepository.GetAllDropCategoryWeights();
            DropCategoryWeightDTO winner = GameUtils.RollWeighted(weights, weight => weight.Weight);

            return winner?.Category ?? DropCategoryTypes.None;
        }

        /// <summary>The same table, with the "nothing" entry left out, so a category always wins.</summary>
        private DropCategoryTypes RollGuaranteedDropCategory()
        {
            var candidates = new List<DropCategoryWeightDTO>();

            foreach (DropCategoryWeightDTO weight in _dropsRepository.GetAllDropCategoryWeights())
            {
                if (weight.Category != DropCategoryTypes.None)
                {
                    candidates.Add(weight);
                }
            }

            DropCategoryWeightDTO winner = GameUtils.RollWeighted(candidates, weight => weight.Weight);

            return winner?.Category ?? DropCategoryTypes.None;
        }

        private ItemRarityConfigSO RollRarity()
        {
            return GameUtils.RollWeighted(_itemsRepository.GetAllItemRarityConfigs(), candidate => candidate.DropWeight);
        }

        private ItemConfigSO RollItem(ItemRarityTypes rarity)
        {
            var candidates = new List<ItemConfigSO>();

            foreach (ItemConfigSO config in _itemsRepository.GetAllItemConfigs())
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

            return GameUtils.RollWeighted(candidates, candidate => candidate.DropWeight);
        }

        #endregion
    }
}
