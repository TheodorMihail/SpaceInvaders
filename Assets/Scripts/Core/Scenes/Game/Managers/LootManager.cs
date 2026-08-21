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
    /// Puts drops on the board and holds what the player has picked up this run. What actually drops
    /// is rolled elsewhere; pending loot is only committed to the inventory on level completion.
    /// </summary>
    public class LootManager : ILootManager
    {
        [Inject] private readonly IItemsRepository _itemsRepository;
        [Inject] private readonly IInventoryManager _inventoryManager;
        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly ISpawnService _spawnService;

        /// <summary>Decides what drops; this manager decides where it lands and who hears about it.</summary>
        [Inject] private readonly IDropRollService _dropRolls;

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
            SpawnDrop(_dropRolls.RollKillCategory(), message.LocalPosition);
        }

        /// <summary>A hazard costs the player something to break, so it always pays out.</summary>
        private void OnHazardDestroyed(HazardDestroyedMessage message)
        {
            SpawnDrop(_dropRolls.RollGuaranteedCategory(), message.LocalPosition);
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
            if (!_dropRolls.TryRollItem(out ItemRarityConfigSO rarityConfig, out InventoryItemEntry item))
            {
                return;
            }

            _spawnService.SpawnItemPickup(rarityConfig, item, localPosition);
            _messageBus.Publish(new ItemDroppedMessage(item.InstanceId, localPosition));
        }

        private void SpawnPowerupDrop(Vector3 localPosition)
        {
            PowerupConfigSO config = _dropRolls.RollPowerup();

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
    }
}
