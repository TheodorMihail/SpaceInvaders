using System.Collections.Generic;
using BaseArchitecture.Core;
using NSubstitute;
using NUnit.Framework;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class InventoryManagerTests : ZenjectUnitTestFixture
    {
        private InventoryManager _inventoryManager;
        private IPersistenceManager _mockPersistenceManager;
        private IRepositoryManager _mockRepositoryManager;
        private InventorySaveData _saveData;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _saveData = new InventorySaveData();
            _mockPersistenceManager = Substitute.For<IPersistenceManager>();
            _mockPersistenceManager.Load<InventorySaveData>(InventorySaveData.SaveKey).Returns(_saveData);
            _mockRepositoryManager = Substitute.For<IRepositoryManager>();

            Container.Bind<IPersistenceManager>().FromInstance(_mockPersistenceManager);
            Container.Bind<IRepositoryManager>().FromInstance(_mockRepositoryManager);

            _inventoryManager = Container.Instantiate<InventoryManager>();
            _inventoryManager.Initialize();
        }

        private static InventoryItemEntry CreateEntry(string instanceId, string itemId = "PlasmaWing")
        {
            return new InventoryItemEntry { InstanceId = instanceId, ItemId = itemId };
        }

        [Test]
        public void AddItems_AppendsEntriesAndPersists()
        {
            _inventoryManager.AddItems(new List<InventoryItemEntry> { CreateEntry("a"), CreateEntry("b") });

            Assert.AreEqual(2, _inventoryManager.Items.Count);
            _mockPersistenceManager.Received(1).Save(InventorySaveData.SaveKey, _saveData);
        }

        [Test]
        public void AddItems_WithEmptyList_DoesNotPersist()
        {
            _inventoryManager.AddItems(new List<InventoryItemEntry>());

            Assert.AreEqual(0, _inventoryManager.Items.Count);
            _mockPersistenceManager.DidNotReceive().Save(InventorySaveData.SaveKey, Arg.Any<InventorySaveData>());
        }

        [Test]
        public void ContainsItem_WithKnownInstanceId_ReturnsTrue()
        {
            _inventoryManager.AddItems(new List<InventoryItemEntry> { CreateEntry("a") });

            Assert.IsTrue(_inventoryManager.ContainsItem("a"));
        }

        [Test]
        public void ContainsItem_WithUnknownInstanceId_ReturnsFalse()
        {
            Assert.IsFalse(_inventoryManager.ContainsItem("missing"));
        }

        [Test]
        public void ContainsItem_WithNullInstanceId_ReturnsFalse()
        {
            Assert.IsFalse(_inventoryManager.ContainsItem(null));
        }

        [Test]
        public void RemoveItem_DropsEntryAndPersists()
        {
            _inventoryManager.AddItems(new List<InventoryItemEntry> { CreateEntry("a"), CreateEntry("b") });
            _mockPersistenceManager.ClearReceivedCalls();

            _inventoryManager.RemoveItem("a");

            Assert.IsFalse(_inventoryManager.ContainsItem("a"));
            Assert.IsTrue(_inventoryManager.ContainsItem("b"));
            _mockPersistenceManager.Received(1).Save(InventorySaveData.SaveKey, _saveData);
        }

        [Test]
        public void RemoveItem_WithUnknownInstanceId_DoesNotPersist()
        {
            _inventoryManager.RemoveItem("missing");

            _mockPersistenceManager.DidNotReceive().Save(InventorySaveData.SaveKey, Arg.Any<InventorySaveData>());
        }

        [Test]
        public void GetItemConfig_ResolvesTemplateFromItemId()
        {
            var config = Substitute.For<ItemConfigSO>();
            _mockRepositoryManager.GetItemConfig("PlasmaWing").Returns(config);
            Assert.AreSame(config, _inventoryManager.GetItemConfig("PlasmaWing"));
        }

        [Test]
        public void GetItemConfig_WithNullEntry_ReturnsNull()
        {
            Assert.IsNull(_inventoryManager.GetItemConfig(null));
        }
    }
}
