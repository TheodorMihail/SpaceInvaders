using System.Collections.Generic;
using BaseArchitecture.Core;
using NSubstitute;
using NUnit.Framework;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class LootManagerTests : ZenjectUnitTestFixture
    {
        private static readonly GameSessionResultDTO _sessionResult =
            new(new GameSessionDTO(GameModeTypes.Campaign, 1), GameplayStateResultTypes.GameOver);

        private LootManager _lootManager;
        private IItemsRepository _mockItemsRepository;
        private IPowerupsRepository _mockPowerupsRepository;
        private IDropsRepository _mockDropsRepository;
        private IInventoryManager _mockInventoryManager;
        private ISpawnManager _mockSpawnManager;
        private IMessageBus _messageBus;

        private readonly List<ItemConfigSO> _itemConfigs = new();
        private readonly List<ItemRarityConfigSO> _rarityConfigs = new();
        private readonly List<PowerupConfigSO> _powerupConfigs = new();

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _itemConfigs.Clear();
            _rarityConfigs.Clear();
            _powerupConfigs.Clear();

            _mockItemsRepository = Substitute.For<IItemsRepository>();
            _mockItemsRepository.GetAllItemConfigs().Returns(_ => _itemConfigs);
            _mockItemsRepository.GetAllItemRarityConfigs().Returns(_ => _rarityConfigs);
            _mockItemsRepository.TryGetItemConfig(Arg.Any<string>(), out ItemConfigSO _)
                .Returns(call =>
                {
                    var found = _itemConfigs.Find(config => config.ItemId == (string)call[0]);
                    call[1] = found;
                    return found != null;
                });

            _mockPowerupsRepository = Substitute.For<IPowerupsRepository>();
            _mockPowerupsRepository.GetAllPowerupConfigs().Returns(_ => _powerupConfigs);

            _mockDropsRepository = Substitute.For<IDropsRepository>();

            _mockInventoryManager = Substitute.For<IInventoryManager>();
            _mockSpawnManager = Substitute.For<ISpawnManager>();
            _messageBus = new MessageBus();

            Container.Bind<IItemsRepository>().FromInstance(_mockItemsRepository);
            Container.Bind<IPowerupsRepository>().FromInstance(_mockPowerupsRepository);
            Container.Bind<IDropsRepository>().FromInstance(_mockDropsRepository);
            Container.Bind<IInventoryManager>().FromInstance(_mockInventoryManager);
            Container.Bind<ISpawnManager>().FromInstance(_mockSpawnManager);
            Container.Bind<IMessageBus>().FromInstance(_messageBus);

            // The real roller over the mocked repositories, so these tests keep exercising the drop
            // tables end to end rather than asserting against a stubbed result.
            Container.Bind<IDropRollService>().To<DropRollService>().AsSingle();

            _lootManager = Container.Instantiate<LootManager>();
            _lootManager.Initialize();
        }

        [TearDown]
        public override void Teardown()
        {
            _lootManager.Dispose();
            _messageBus.Dispose();
            base.Teardown();
        }

        private void AddRarity(ItemRarityTypes rarity, int dropWeight, int affixCount)
        {
            var config = Substitute.For<ItemRarityConfigSO>();
            config.Rarity.Returns(rarity);
            config.DropWeight.Returns(dropWeight);
            config.AffixCount.Returns(affixCount);
            _rarityConfigs.Add(config);
        }

        private void AddItem(string itemId, ItemRarityTypes rarity, params ItemAffixDTO[] affixes)
        {
            var config = Substitute.For<ItemConfigSO>();
            config.ItemId.Returns(itemId);
            config.Rarity.Returns(rarity);
            config.DropWeight.Returns(1);
            config.SlotType.Returns(ItemSlotTypes.Wings);
            config.PossibleAffixes.Returns(new List<ItemAffixDTO>(affixes));
            _itemConfigs.Add(config);
        }

        private static ItemAffixDTO Affix(ShipUpgradableStatTypes statType, float min = 0.1f, float max = 0.2f, ShipStatValueTypes valueType = ShipStatValueTypes.Percentage)
        {
            return new ItemAffixDTO(statType, valueType, min, max);
        }

        private void AddPowerup(PowerupTypes type, int dropWeight = 1)
        {
            var config = Substitute.For<PowerupConfigSO>();
            config.PowerupType.Returns(type);
            config.DropWeight.Returns(dropWeight);
            _powerupConfigs.Add(config);
        }

        private void SetupSingleNormalItem(int affixCount = 1, params ItemAffixDTO[] affixes)
        {
            AddRarity(ItemRarityTypes.Normal, 1, affixCount);
            AddItem("PlasmaWing", ItemRarityTypes.Normal, affixes);
        }

        /// <summary>Stubs the category roll so only the given category can ever win.</summary>
        private void GuaranteeDropCategory(DropCategoryTypes category)
        {
            _mockDropsRepository.GetAllDropCategoryWeights().Returns(new List<DropCategoryWeightDTO>
            {
                new(category, 1)
            });
        }

        private static void PublishEnemyDestroyed(IMessageBus bus)
        {
            bus.Publish(new EnemyDestroyedMessage(EnemyTypes.Enemy1, EnemyCategoryTypes.Normal, Vector3.zero));
        }

        private InventoryItemEntry CaptureSpawnedItem()
        {
            InventoryItemEntry captured = null;
            _mockSpawnManager.SpawnItemPickup(
                Arg.Any<ItemRarityConfigSO>(),
                Arg.Do<InventoryItemEntry>(entry => captured = entry),
                Arg.Any<Vector3>());

            PublishEnemyDestroyed(_messageBus);
            return captured;
        }

        [Test]
        public void OnEnemyDestroyed_WhenNoneCategoryWins_DoesNotSpawnPickup()
        {
            SetupSingleNormalItem();
            GuaranteeDropCategory(DropCategoryTypes.None);

            PublishEnemyDestroyed(_messageBus);

            _mockSpawnManager.DidNotReceive().SpawnItemPickup(
                Arg.Any<ItemRarityConfigSO>(), Arg.Any<InventoryItemEntry>(), Arg.Any<Vector3>());
        }

        [Test]
        public void OnEnemyDestroyed_WhenItemCategoryWins_SpawnsPickup()
        {
            SetupSingleNormalItem(1, Affix(ShipUpgradableStatTypes.Damage));
            GuaranteeDropCategory(DropCategoryTypes.Item);

            PublishEnemyDestroyed(_messageBus);

            _mockSpawnManager.Received(1).SpawnItemPickup(
                Arg.Any<ItemRarityConfigSO>(), Arg.Any<InventoryItemEntry>(), Arg.Any<Vector3>());
        }

        [Test]
        public void OnEnemyDestroyed_WhenPowerupCategoryWins_SpawnsPowerupDropAndNoItem()
        {
            SetupSingleNormalItem(1, Affix(ShipUpgradableStatTypes.Damage));
            AddPowerup(PowerupTypes.Heal);
            GuaranteeDropCategory(DropCategoryTypes.Powerup);

            PublishEnemyDestroyed(_messageBus);

            _mockSpawnManager.Received(1).SpawnPowerup(Arg.Any<PowerupConfigSO>(), Vector3.zero);
            _mockSpawnManager.DidNotReceive().SpawnItemPickup(
                Arg.Any<ItemRarityConfigSO>(), Arg.Any<InventoryItemEntry>(), Arg.Any<Vector3>());
        }

        [Test]
        public void OnEnemyDestroyed_WhenPowerupCategoryWins_PublishesPowerupDroppedMessage()
        {
            SetupSingleNormalItem(1, Affix(ShipUpgradableStatTypes.Damage));
            AddPowerup(PowerupTypes.Heal);
            GuaranteeDropCategory(DropCategoryTypes.Powerup);

            var droppedType = default(PowerupTypes?);
            _messageBus.Subscribe<PowerupDroppedMessage>(message => droppedType = message.Type);

            PublishEnemyDestroyed(_messageBus);

            Assert.AreEqual(PowerupTypes.Heal, droppedType);
        }

        [Test]
        public void OnEnemyDestroyed_WhenItemCategoryWins_DoesNotSpawnPowerupDrop()
        {
            SetupSingleNormalItem(1, Affix(ShipUpgradableStatTypes.Damage));
            AddPowerup(PowerupTypes.Heal);
            GuaranteeDropCategory(DropCategoryTypes.Item);

            PublishEnemyDestroyed(_messageBus);

            _mockSpawnManager.DidNotReceive().SpawnPowerup(Arg.Any<PowerupConfigSO>(), Arg.Any<Vector3>());
        }

        [Test]
        public void OnEnemyDestroyed_RollsAffixCountFromRarity()
        {
            AddRarity(ItemRarityTypes.Normal, 1, 2);
            AddItem("PlasmaWing", ItemRarityTypes.Normal,
                Affix(ShipUpgradableStatTypes.Damage),
                Affix(ShipUpgradableStatTypes.MoveSpeed),
                Affix(ShipUpgradableStatTypes.Health));
            GuaranteeDropCategory(DropCategoryTypes.Item);

            InventoryItemEntry item = CaptureSpawnedItem();

            Assert.IsNotNull(item);
            Assert.AreEqual(2, item.Affixes.Count);
        }

        [Test]
        public void OnEnemyDestroyed_RollsDistinctAffixes()
        {
            AddRarity(ItemRarityTypes.Normal, 1, 3);
            AddItem("PlasmaWing", ItemRarityTypes.Normal,
                Affix(ShipUpgradableStatTypes.Damage),
                Affix(ShipUpgradableStatTypes.MoveSpeed),
                Affix(ShipUpgradableStatTypes.Health));
            GuaranteeDropCategory(DropCategoryTypes.Item);

            InventoryItemEntry item = CaptureSpawnedItem();

            var seen = new HashSet<string>();
            foreach (AffixEntry affix in item.Affixes)
            {
                Assert.IsTrue(seen.Add(affix.StatType), $"Affix {affix.StatType} was rolled twice.");
            }
        }

        [Test]
        public void OnEnemyDestroyed_RollsBonusWithinAuthoredRange()
        {
            SetupSingleNormalItem(1, Affix(ShipUpgradableStatTypes.Damage, 0.25f, 0.75f));
            GuaranteeDropCategory(DropCategoryTypes.Item);

            InventoryItemEntry item = CaptureSpawnedItem();

            Assert.AreEqual(1, item.Affixes.Count);
            Assert.GreaterOrEqual(item.Affixes[0].Bonus, 0.25f);
            Assert.LessOrEqual(item.Affixes[0].Bonus, 0.75f);
        }

        [Test]
        public void OnEnemyDestroyed_AssignsUniqueInstanceIds()
        {
            SetupSingleNormalItem(1, Affix(ShipUpgradableStatTypes.Damage));
            GuaranteeDropCategory(DropCategoryTypes.Item);

            var ids = new List<string>();
            _mockSpawnManager.SpawnItemPickup(
                Arg.Any<ItemRarityConfigSO>(),
                Arg.Do<InventoryItemEntry>(entry => ids.Add(entry.InstanceId)),
                Arg.Any<Vector3>());

            PublishEnemyDestroyed(_messageBus);
            PublishEnemyDestroyed(_messageBus);

            Assert.AreEqual(2, ids.Count);
            Assert.AreNotEqual(ids[0], ids[1]);
        }

        [Test]
        public void OnEnemyDestroyed_WithUndersizedAffixPool_RollsWhatIsAvailable()
        {
            AddRarity(ItemRarityTypes.Normal, 1, 3);
            AddItem("PlasmaWing", ItemRarityTypes.Normal, Affix(ShipUpgradableStatTypes.Damage));
            GuaranteeDropCategory(DropCategoryTypes.Item);

            InventoryItemEntry item = CaptureSpawnedItem();

            Assert.AreEqual(1, item.Affixes.Count);
        }

        [Test]
        public void OnEnemyDestroyed_WithNoItemsForRolledRarity_DoesNotSpawnPickup()
        {
            AddRarity(ItemRarityTypes.Legendary, 1, 1);
            GuaranteeDropCategory(DropCategoryTypes.Item);

            PublishEnemyDestroyed(_messageBus);

            _mockSpawnManager.DidNotReceive().SpawnItemPickup(
                Arg.Any<ItemRarityConfigSO>(), Arg.Any<InventoryItemEntry>(), Arg.Any<Vector3>());
        }

        [Test]
        public void CollectItem_PublishesItemCollectedMessage()
        {
            SetupSingleNormalItem();

            string collectedInstanceId = null;
            _messageBus.Subscribe<ItemCollectedMessage>(message => collectedInstanceId = message.InstanceId);

            _lootManager.CollectItem(new InventoryItemEntry { InstanceId = "a", ItemId = "PlasmaWing" });

            Assert.AreEqual("a", collectedInstanceId);
        }

        [Test]
        public void OnLevelCompleted_BanksPendingLootIntoInventory()
        {
            SetupSingleNormalItem();

            _lootManager.CollectItem(new InventoryItemEntry { InstanceId = "a", ItemId = "PlasmaWing" });
            _lootManager.CollectItem(new InventoryItemEntry { InstanceId = "b", ItemId = "PlasmaWing" });

            _messageBus.Publish(new LevelCompletedMessage(1));

            _mockInventoryManager.Received(1).AddItems(Arg.Is<IReadOnlyList<InventoryItemEntry>>(list => list.Count == 2));
        }

        [Test]
        public void OnLevelCompleted_SetsLastBankedLootToWhatWasBanked()
        {
            SetupSingleNormalItem();

            _lootManager.CollectItem(new InventoryItemEntry { InstanceId = "a", ItemId = "PlasmaWing" });
            _lootManager.CollectItem(new InventoryItemEntry { InstanceId = "b", ItemId = "PlasmaWing" });

            _messageBus.Publish(new LevelCompletedMessage(1));

            Assert.AreEqual(2, _lootManager.LastBankedLoot.Count);
            CollectionAssert.AreEquivalent(
                new[] { "a", "b" },
                new List<string> { _lootManager.LastBankedLoot[0].InstanceId, _lootManager.LastBankedLoot[1].InstanceId });
        }

        [Test]
        public void OnLevelCompleted_WithNoPendingLoot_DoesNotChangeLastBankedLoot()
        {
            SetupSingleNormalItem();
            _lootManager.CollectItem(new InventoryItemEntry { InstanceId = "a", ItemId = "PlasmaWing" });
            _messageBus.Publish(new LevelCompletedMessage(1));

            _messageBus.Publish(new LevelCompletedMessage(2));

            Assert.AreEqual(1, _lootManager.LastBankedLoot.Count);
            Assert.AreEqual("a", _lootManager.LastBankedLoot[0].InstanceId);
        }

        [Test]
        public void OnLevelCompleted_WithNoPendingLoot_DoesNotTouchInventory()
        {
            _messageBus.Publish(new LevelCompletedMessage(1));

            _mockInventoryManager.DidNotReceive().AddItems(Arg.Any<IReadOnlyList<InventoryItemEntry>>());
        }

        [Test]
        public void GameEnd_DiscardsPendingLoot()
        {
            SetupSingleNormalItem();
            _lootManager.CollectItem(new InventoryItemEntry { InstanceId = "a", ItemId = "PlasmaWing" });

            _lootManager.GameEnd(_sessionResult);
            _messageBus.Publish(new LevelCompletedMessage(1));

            _mockInventoryManager.DidNotReceive().AddItems(Arg.Any<IReadOnlyList<InventoryItemEntry>>());
        }

        [Test]
        public void OnLevelCompleted_ClearsPendingLootSoNextRunStartsEmpty()
        {
            SetupSingleNormalItem();
            _lootManager.CollectItem(new InventoryItemEntry { InstanceId = "a", ItemId = "PlasmaWing" });

            _messageBus.Publish(new LevelCompletedMessage(1));
            _mockInventoryManager.ClearReceivedCalls();
            _messageBus.Publish(new LevelCompletedMessage(2));

            _mockInventoryManager.DidNotReceive().AddItems(Arg.Any<IReadOnlyList<InventoryItemEntry>>());
        }

        [Test]
        public void Dispose_StopsReactingToEnemyDestroyedMessage()
        {
            SetupSingleNormalItem(1, Affix(ShipUpgradableStatTypes.Damage));
            GuaranteeDropCategory(DropCategoryTypes.Item);

            _lootManager.Dispose();
            PublishEnemyDestroyed(_messageBus);

            _mockSpawnManager.DidNotReceive().SpawnItemPickup(
                Arg.Any<ItemRarityConfigSO>(), Arg.Any<InventoryItemEntry>(), Arg.Any<Vector3>());
        }
    }
}
