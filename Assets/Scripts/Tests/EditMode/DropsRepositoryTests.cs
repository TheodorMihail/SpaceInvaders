using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using SpaceInvaders.Project;
using UnityEngine;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class DropsRepositoryTests
    {
        private DropsRepository _dropsRepository;
        private DropsDataConfigSO _mockDataConfig;
        private DropTableConfigSO _mockCampaignTable;
        private DropTableConfigSO _mockExpeditionTable;

        [SetUp]
        public void Setup()
        {
            _mockCampaignTable = CreateTable(DropTableTypes.Campaign, DropCategoryTypes.Item, 5);
            _mockExpeditionTable = CreateTable(DropTableTypes.Expedition, DropCategoryTypes.Powerup, 7);

            _mockDataConfig = Substitute.For<DropsDataConfigSO>();
            _mockDataConfig.DropTableConfigs.Returns(new List<DropTableConfigSO>
            {
                _mockCampaignTable,
                _mockExpeditionTable
            });

            _dropsRepository = new DropsRepository(_mockDataConfig);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_mockCampaignTable);
            Object.DestroyImmediate(_mockExpeditionTable);
            Object.DestroyImmediate(_mockDataConfig);
        }

        /// <summary>The whole point: the same call reaches a different table per mode.</summary>
        [Test]
        public void GetDropCategoryWeights_ReturnsTheTableForThatType()
        {
            IReadOnlyList<DropCategoryWeightDTO> campaign =
                _dropsRepository.GetDropCategoryWeights(DropTableTypes.Campaign);
            IReadOnlyList<DropCategoryWeightDTO> expedition =
                _dropsRepository.GetDropCategoryWeights(DropTableTypes.Expedition);

            Assert.AreEqual(DropCategoryTypes.Item, campaign[0].Category);
            Assert.AreEqual(DropCategoryTypes.Powerup, expedition[0].Category);
        }

        /// <summary>An unauthored table drops nothing rather than throwing at the roll.</summary>
        [Test]
        public void GetDropCategoryWeights_WithNoTableForThatType_IsEmpty()
        {
            _mockDataConfig.DropTableConfigs.Returns(new List<DropTableConfigSO> { _mockCampaignTable });
            var repository = new DropsRepository(_mockDataConfig);

            Assert.IsEmpty(repository.GetDropCategoryWeights(DropTableTypes.Expedition));
        }

        private static DropTableConfigSO CreateTable(DropTableTypes tableType, DropCategoryTypes category, int weight)
        {
            var config = Substitute.For<DropTableConfigSO>();

            config.ObjectID.Returns(tableType.ToString());
            config.CategoryWeights.Returns(new List<DropCategoryWeightDTO> { new(category, weight) });

            return config;
        }
    }
}
