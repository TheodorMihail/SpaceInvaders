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
    /// Spawns drops and holds what the player picked up this run. What drops is rolled elsewhere;
    /// pending loot is only added to the inventory on level completion.
    /// </summary>
    public partial class LootManager : ILootManager
    {
        [Inject] private readonly IItemsRepository _itemsRepository;
        [Inject] private readonly IInventoryManager _inventoryManager;
        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly ISpawnManager _spawnManager;

        /// <summary>Decides what drops; this manager decides where it spawns and publishes it.</summary>
        [Inject] private readonly IDropRollService _dropRolls;

        /// <summary>Added to the inventory only on level completion.</summary>
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
                    SpawnPowerupDrop(_dropRolls.RollPowerup(), localPosition);
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

            _spawnManager.SpawnItemPickup(rarityConfig, item, localPosition);
            _messageBus.Publish(new ItemDroppedMessage(item.InstanceId, localPosition));
        }

        private void SpawnPowerupDrop(PowerupConfigSO config, Vector3 localPosition)
        {
            if (config == null)
            {
                return;
            }

            _spawnManager.SpawnPowerup(config, localPosition);
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
