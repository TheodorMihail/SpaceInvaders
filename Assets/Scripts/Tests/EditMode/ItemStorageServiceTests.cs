using System.Collections.Generic;
using BaseArchitecture.Core;
using NSubstitute;
using NUnit.Framework;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class ItemStorageServiceTests : ZenjectUnitTestFixture
    {
        private ItemStorageService _itemStorage;
        private IPersistenceManager _mockPersistenceManager;
        private InventorySaveData _saveData;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _saveData = new InventorySaveData();
            _mockPersistenceManager = Substitute.For<IPersistenceManager>();
            _mockPersistenceManager.Load<InventorySaveData>(InventorySaveData.SaveKey).Returns(_saveData);

            Container.Bind<IPersistenceManager>().FromInstance(_mockPersistenceManager);

            _itemStorage = Container.Instantiate<ItemStorageService>();
            _itemStorage.Initialize();
        }

        private static InventoryItemEntry CreateEntry(string instanceId, string itemId = "PlasmaWing")
        {
            return new InventoryItemEntry { InstanceId = instanceId, ItemId = itemId };
        }

        [Test]
        public void AddItems_AppendsEntriesAndPersists()
        {
            _itemStorage.AddItems(new List<InventoryItemEntry> { CreateEntry("a"), CreateEntry("b") });

            Assert.AreEqual(2, _itemStorage.Items.Count);
            _mockPersistenceManager.Received(1).Save(InventorySaveData.SaveKey, _saveData);
        }

        [Test]
        public void AddItems_WithEmptyList_DoesNotPersist()
        {
            _itemStorage.AddItems(new List<InventoryItemEntry>());

            Assert.AreEqual(0, _itemStorage.Items.Count);
            _mockPersistenceManager.DidNotReceive().Save(InventorySaveData.SaveKey, Arg.Any<InventorySaveData>());
        }

        [Test]
        public void ContainsItem_WithKnownInstanceId_ReturnsTrue()
        {
            _itemStorage.AddItems(new List<InventoryItemEntry> { CreateEntry("a") });

            Assert.IsTrue(_itemStorage.ContainsItem("a"));
        }

        [Test]
        public void ContainsItem_WithUnknownInstanceId_ReturnsFalse()
        {
            Assert.IsFalse(_itemStorage.ContainsItem("missing"));
        }

        [Test]
        public void ContainsItem_WithNullInstanceId_ReturnsFalse()
        {
            Assert.IsFalse(_itemStorage.ContainsItem(null));
        }

        [Test]
        public void RemoveItem_DropsEntryAndPersists()
        {
            _itemStorage.AddItems(new List<InventoryItemEntry> { CreateEntry("a"), CreateEntry("b") });
            _mockPersistenceManager.ClearReceivedCalls();

            _itemStorage.RemoveItem("a");

            Assert.IsFalse(_itemStorage.ContainsItem("a"));
            Assert.IsTrue(_itemStorage.ContainsItem("b"));
            _mockPersistenceManager.Received(1).Save(InventorySaveData.SaveKey, _saveData);
        }

        [Test]
        public void RemoveItem_WithUnknownInstanceId_DoesNotPersist()
        {
            _itemStorage.RemoveItem("missing");

            _mockPersistenceManager.DidNotReceive().Save(InventorySaveData.SaveKey, Arg.Any<InventorySaveData>());
        }

        [Test]
        public void ClearAll_EmptiesStorageAndPersists()
        {
            _itemStorage.AddItems(new List<InventoryItemEntry> { CreateEntry("a"), CreateEntry("b") });
            _mockPersistenceManager.ClearReceivedCalls();

            _itemStorage.ClearAll();

            Assert.AreEqual(0, _itemStorage.Items.Count);
            _mockPersistenceManager.Received(1).Save(InventorySaveData.SaveKey, _saveData);
        }
    }
}
