using System.Collections.Generic;
using BaseArchitecture.Core;
using NSubstitute;
using NUnit.Framework;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class EquipmentManagerTests : ZenjectUnitTestFixture
    {
        private EquipmentManager _equipmentManager;
        private IPersistenceManager _mockPersistenceManager;
        private IRepositoryManager _mockRepositoryManager;
        private IInventoryManager _mockInventoryManager;
        private IMessageBus _messageBus;

        private readonly Dictionary<string, InventoryItemEntry> _ownedItems = new();
        private readonly Dictionary<string, ItemConfigSO> _configsByItemId = new();

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _ownedItems.Clear();
            _configsByItemId.Clear();

            _mockPersistenceManager = Substitute.For<IPersistenceManager>();
            _mockPersistenceManager.Load<EquipmentSaveData>(EquipmentSaveData.SaveKey).Returns(new EquipmentSaveData());

            _mockRepositoryManager = Substitute.For<IRepositoryManager>();
            _mockRepositoryManager.GetAllEquipmentSlotConfigs().Returns(new List<EquipmentSlotConfigDTO>
            {
                new(EquipmentSlotTypes.Wings, ItemSlotTypes.Wings, "Wings"),
                new(EquipmentSlotTypes.Engine, ItemSlotTypes.Engine, "Tail"),
                new(EquipmentSlotTypes.Weapon, ItemSlotTypes.Weapon, "Head"),
                new(EquipmentSlotTypes.Core, ItemSlotTypes.Core, "Center")
            });

            _mockInventoryManager = Substitute.For<IInventoryManager>();
            _mockInventoryManager.GetItem(Arg.Any<string>())
                .Returns(call => GetOwnedItem((string)call[0]));
            _mockInventoryManager.GetItemConfig(Arg.Any<string>())
                .Returns(call => GetConfigForItemId((string)call[0]));

            _messageBus = new MessageBus();

            Container.Bind<IPersistenceManager>().FromInstance(_mockPersistenceManager);
            Container.Bind<IRepositoryManager>().FromInstance(_mockRepositoryManager);
            Container.Bind<IInventoryManager>().FromInstance(_mockInventoryManager);
            Container.Bind<IMessageBus>().FromInstance(_messageBus);

            _equipmentManager = Container.Instantiate<EquipmentManager>();
            _equipmentManager.Initialize();
        }

        [TearDown]
        public override void Teardown()
        {
            _messageBus.Dispose();
            base.Teardown();
        }

        private InventoryItemEntry GetOwnedItem(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
            {
                return null;
            }

            _ownedItems.TryGetValue(instanceId, out InventoryItemEntry entry);
            return entry;
        }

        private ItemConfigSO GetConfigForItemId(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return null;
            }

            _configsByItemId.TryGetValue(itemId, out ItemConfigSO config);
            return config;
        }

        /// <summary>Registers an owned item of the given slot type, with optional rolled affixes.</summary>
        private InventoryItemEntry GiveItem(string instanceId, ItemSlotTypes slotType, params RolledAffixEntry[] affixes)
        {
            string itemId = $"{slotType}Item";

            if (!_configsByItemId.ContainsKey(itemId))
            {
                var config = Substitute.For<ItemConfigSO>();
                config.ItemId.Returns(itemId);
                config.SlotType.Returns(slotType);
                _configsByItemId[itemId] = config;
            }

            var entry = new InventoryItemEntry
            {
                InstanceId = instanceId,
                ItemId = itemId,
                Affixes = new List<RolledAffixEntry>(affixes)
            };

            _ownedItems[instanceId] = entry;
            return entry;
        }

        private static RolledAffixEntry Affix(ShipUpgradableStatTypes statType, float bonus)
        {
            return new RolledAffixEntry { StatType = statType.ToString(), Bonus = bonus };
        }

        [Test]
        public void GetSlotForItem_MapsEachItemSlotTypeToMatchingEquipmentSlot()
        {
            GiveItem("w1", ItemSlotTypes.Wings);
            GiveItem("e1", ItemSlotTypes.Engine);
            GiveItem("h1", ItemSlotTypes.Weapon);
            GiveItem("c1", ItemSlotTypes.Core);

            Assert.AreEqual(EquipmentSlotTypes.Wings, _equipmentManager.GetEquipmentSlotTypeForItem("w1"));
            Assert.AreEqual(EquipmentSlotTypes.Engine, _equipmentManager.GetEquipmentSlotTypeForItem("e1"));
            Assert.AreEqual(EquipmentSlotTypes.Weapon, _equipmentManager.GetEquipmentSlotTypeForItem("h1"));
            Assert.AreEqual(EquipmentSlotTypes.Core, _equipmentManager.GetEquipmentSlotTypeForItem("c1"));
        }

        [Test]
        public void GetSlotForItem_WithNoMatchingSlotConfig_ReturnsNull()
        {
            _mockRepositoryManager.GetAllEquipmentSlotConfigs().Returns(new List<EquipmentSlotConfigDTO>
            {
                new(EquipmentSlotTypes.Wings, ItemSlotTypes.Wings, "Wings")
            });

            GiveItem("h1", ItemSlotTypes.Weapon);

            Assert.IsNull(_equipmentManager.GetEquipmentSlotTypeForItem("h1"));
        }

        [Test]
        public void Equip_StoresItemInSlot()
        {
            GiveItem("w1", ItemSlotTypes.Wings);

            _equipmentManager.Equip("w1");

            Assert.AreEqual("w1", _equipmentManager.GetEquippedItemForEquipmentSlotType(EquipmentSlotTypes.Wings).InstanceId);
            Assert.IsTrue(_equipmentManager.IsEquipped("w1"));
        }

        [Test]
        public void Equip_WithUnknownInstanceId_DoesNothing()
        {
            _equipmentManager.Equip("missing");

            Assert.IsFalse(_equipmentManager.IsEquipped("missing"));
        }

        [Test]
        public void Equip_WithNoMatchingSlotConfig_IsIgnored()
        {
            _mockRepositoryManager.GetAllEquipmentSlotConfigs().Returns(new List<EquipmentSlotConfigDTO>
            {
                new(EquipmentSlotTypes.Wings, ItemSlotTypes.Wings, "Wings")
            });

            GiveItem("h1", ItemSlotTypes.Weapon);

            _equipmentManager.Equip("h1");

            Assert.IsFalse(_equipmentManager.IsEquipped("h1"));
        }

        [Test]
        public void Equip_IntoOccupiedSlot_ReplacesPreviousItem()
        {
            GiveItem("w1", ItemSlotTypes.Wings);
            GiveItem("w2", ItemSlotTypes.Wings);

            _equipmentManager.Equip("w1");
            _equipmentManager.Equip("w2");

            Assert.AreEqual("w2", _equipmentManager.GetEquippedItemForEquipmentSlotType(EquipmentSlotTypes.Wings).InstanceId);
            Assert.IsFalse(_equipmentManager.IsEquipped("w1"));
        }

        [Test]
        public void Equip_IntoEmptySlot_PublishesMessageWithNoUnequippedInstance()
        {
            GiveItem("w1", ItemSlotTypes.Wings);
            ItemEquipChangedMessage? received = null;
            _messageBus.Subscribe<ItemEquipChangedMessage>(message => received = message);

            _equipmentManager.Equip("w1");

            Assert.AreEqual("w1", received?.EquippedInstanceId);
            Assert.IsNull(received?.UnequippedInstanceId);
        }

        [Test]
        public void Equip_IntoOccupiedSlot_PublishesMessageWithDisplacedInstance()
        {
            GiveItem("w1", ItemSlotTypes.Wings);
            GiveItem("w2", ItemSlotTypes.Wings);
            _equipmentManager.Equip("w1");

            ItemEquipChangedMessage? received = null;
            _messageBus.Subscribe<ItemEquipChangedMessage>(message => received = message);

            _equipmentManager.Equip("w2");

            Assert.AreEqual("w2", received?.EquippedInstanceId);
            Assert.AreEqual("w1", received?.UnequippedInstanceId);
        }

        [Test]
        public void Unequip_ClearsSlot()
        {
            GiveItem("w1", ItemSlotTypes.Wings);
            _equipmentManager.Equip("w1");

            _equipmentManager.Unequip("w1");

            Assert.IsNull(_equipmentManager.GetEquippedItemForEquipmentSlotType(EquipmentSlotTypes.Wings));
            Assert.IsFalse(_equipmentManager.IsEquipped("w1"));
        }

        [Test]
        public void Unequip_PublishesMessageWithNoEquippedInstance()
        {
            GiveItem("w1", ItemSlotTypes.Wings);
            _equipmentManager.Equip("w1");

            ItemEquipChangedMessage? received = null;
            _messageBus.Subscribe<ItemEquipChangedMessage>(message => received = message);

            _equipmentManager.Unequip("w1");

            Assert.IsNull(received?.EquippedInstanceId);
            Assert.AreEqual("w1", received?.UnequippedInstanceId);
        }

        [Test]
        public void Unequip_WithUnknownInstanceId_DoesNothing()
        {
            GiveItem("w1", ItemSlotTypes.Wings);
            _equipmentManager.Equip("w1");

            _equipmentManager.Unequip("missing");

            Assert.IsTrue(_equipmentManager.IsEquipped("w1"));
        }

        [Test]
        public void GetEquippedItem_WhenItemNoLongerOwned_PrunesSlot()
        {
            GiveItem("w1", ItemSlotTypes.Wings);
            _equipmentManager.Equip("w1");

            _ownedItems.Remove("w1");

            Assert.IsNull(_equipmentManager.GetEquippedItemForEquipmentSlotType(EquipmentSlotTypes.Wings));
            Assert.IsFalse(_equipmentManager.IsEquipped("w1"));
        }

        [Test]
        public void ApplyEquipmentBonuses_AppliesRolledDamageBonus()
        {
            GiveItem("w1", ItemSlotTypes.Wings, Affix(ShipUpgradableStatTypes.Damage, 0.5f));
            _equipmentManager.Equip("w1");

            var stats = new ShipStats(new ShipBaseStats());
            _equipmentManager.ApplyEquipmentBonuses(stats);

            Assert.AreEqual(15, stats.CurrentProjectileDamage);
        }

        [Test]
        public void ApplyEquipmentBonuses_InvertsFireRateSoPositiveBonusShootsFaster()
        {
            GiveItem("t1", ItemSlotTypes.Engine, Affix(ShipUpgradableStatTypes.FireRate, 0.5f));
            _equipmentManager.Equip("t1");

            var stats = new ShipStats(new ShipBaseStats());
            float baseFireRate = stats.BaseFireRate;

            _equipmentManager.ApplyEquipmentBonuses(stats);

            Assert.Less(stats.CurrentFireRate, baseFireRate);
        }

        [Test]
        public void ApplyEquipmentBonuses_StacksAcrossSlots()
        {
            GiveItem("w1", ItemSlotTypes.Wings, Affix(ShipUpgradableStatTypes.Damage, 0.5f));
            GiveItem("h1", ItemSlotTypes.Weapon, Affix(ShipUpgradableStatTypes.Damage, 0.5f));
            _equipmentManager.Equip("w1");
            _equipmentManager.Equip("h1");

            var stats = new ShipStats(new ShipBaseStats());
            _equipmentManager.ApplyEquipmentBonuses(stats);

            Assert.AreEqual(20, stats.CurrentProjectileDamage);
        }

        [Test]
        public void ApplyEquipmentBonuses_WithNothingEquipped_LeavesBaseStats()
        {
            var stats = new ShipStats(new ShipBaseStats());
            _equipmentManager.ApplyEquipmentBonuses(stats);

            Assert.AreEqual(stats.BaseProjectileDamage, stats.CurrentProjectileDamage);
        }

        [Test]
        public void ApplyEquipmentBonuses_RefillsHealthAfterHealthBonus()
        {
            GiveItem("c1", ItemSlotTypes.Core, Affix(ShipUpgradableStatTypes.Health, 1f));
            _equipmentManager.Equip("c1");

            var stats = new ShipStats(new ShipBaseStats());
            _equipmentManager.ApplyEquipmentBonuses(stats);

            Assert.AreEqual(stats.CurrentMaxHealth, stats.CurrentHealth);
            Assert.AreEqual(200, stats.CurrentMaxHealth);
        }
    }
}
