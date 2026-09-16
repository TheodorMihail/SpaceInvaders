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
    public class TalentManagerTests : ZenjectUnitTestFixture
    {
        private const string TalentId = "Damage";
        private const int FirstLevelCost = 50;
        private const int SecondLevelCost = 100;

        private TalentManager _talentManager;
        private ICurrencyManager _mockCurrencyManager;
        private TalentConfigSO _mockTalentConfig;

        private readonly List<TalentConfigSO> _talents = new();

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _talents.Clear();
            _mockTalentConfig = CreateTalent(TalentId, FirstLevelCost, SecondLevelCost);
            _talents.Add(_mockTalentConfig);

            var mockPersistenceManager = Substitute.For<IPersistenceManager>();
            mockPersistenceManager
                .LoadVersioned<TalentsSaveData>(TalentsSaveData.SaveKey, TalentsSaveData.CurrentVersion)
                .Returns(new TalentsSaveData { Version = TalentsSaveData.CurrentVersion });

            var mockSaveProfileManager = Substitute.For<ISaveProfileManager>();
            mockSaveProfileManager.GetProfile(Arg.Any<GameModeTypes>()).Returns(mockPersistenceManager);

            var mockTalentsRepository = Substitute.For<ITalentsRepository>();
            mockTalentsRepository.GetAllTalentConfigs().Returns(_ => _talents);
            mockTalentsRepository.TryGetTalentConfig(TalentId, out TalentConfigSO _)
                .Returns(call =>
                {
                    call[1] = _mockTalentConfig;
                    return true;
                });

            _mockCurrencyManager = Substitute.For<ICurrencyManager>();

            Container.Bind<ISaveProfileManager>().FromInstance(mockSaveProfileManager);
            Container.Bind<ITalentsRepository>().FromInstance(mockTalentsRepository);
            Container.Bind<ICurrencyManager>().FromInstance(_mockCurrencyManager);

            _talentManager = Container.Instantiate<TalentManager>();
            _talentManager.LoadForMode(GameModeTypes.Campaign);
        }

        [TearDown]
        public override void Teardown()
        {
            foreach (TalentConfigSO talent in _talents)
            {
                Object.DestroyImmediate(talent);
            }

            base.Teardown();
        }

        [Test]
        public void GetTalentLevel_BeforeAnythingIsTaken_IsZero()
        {
            Assert.AreEqual(0, _talentManager.GetTalentLevel(TalentId));
        }

        [Test]
        public void GetNextLevelCost_FollowsTheLevelAlreadyOwned()
        {
            Assert.AreEqual(FirstLevelCost, _talentManager.GetNextLevelCost(TalentId));

            _talentManager.TryGrantLevel(TalentId);

            Assert.AreEqual(SecondLevelCost, _talentManager.GetNextLevelCost(TalentId));
        }

        [Test]
        public void TryPurchaseLevel_WhenAffordable_SpendsTheCostAndAddsALevel()
        {
            _mockCurrencyManager.TrySpend(FirstLevelCost).Returns(true);

            Assert.IsTrue(_talentManager.TryPurchaseLevel(TalentId));
            Assert.AreEqual(1, _talentManager.GetTalentLevel(TalentId));
            _mockCurrencyManager.Received(1).TrySpend(FirstLevelCost);
        }

        [Test]
        public void TryPurchaseLevel_WhenTheCurrencyIsRefused_AddsNothing()
        {
            _mockCurrencyManager.TrySpend(Arg.Any<int>()).Returns(false);

            Assert.IsFalse(_talentManager.TryPurchaseLevel(TalentId));
            Assert.AreEqual(0, _talentManager.GetTalentLevel(TalentId));
        }

        /// <summary>A granted level is the same level, only nobody pays for it.</summary>
        [Test]
        public void TryGrantLevel_AddsALevelWithoutSpending()
        {
            Assert.IsTrue(_talentManager.TryGrantLevel(TalentId));

            Assert.AreEqual(1, _talentManager.GetTalentLevel(TalentId));
            _mockCurrencyManager.DidNotReceive().TrySpend(Arg.Any<int>());
        }

        /// <summary>The level count is the cap, so a maxed talent refuses a grant rather than
        /// silently swallowing the reward.</summary>
        [Test]
        public void TryGrantLevel_AtMaxLevel_IsRefused()
        {
            _talentManager.TryGrantLevel(TalentId);
            _talentManager.TryGrantLevel(TalentId);

            Assert.IsTrue(_talentManager.IsMaxLevel(TalentId));
            Assert.IsFalse(_talentManager.TryGrantLevel(TalentId));
            Assert.AreEqual(2, _talentManager.GetTalentLevel(TalentId));
        }

        [Test]
        public void TryPurchaseLevel_AtMaxLevel_IsRefused()
        {
            _mockCurrencyManager.TrySpend(Arg.Any<int>()).Returns(true);
            _talentManager.TryGrantLevel(TalentId);
            _talentManager.TryGrantLevel(TalentId);

            Assert.IsFalse(_talentManager.TryPurchaseLevel(TalentId));
        }

        /// <summary>An unknown talent has no ladder, so it is treated as having nothing left.</summary>
        [Test]
        public void IsMaxLevel_ForAnUnknownTalent_IsTrue()
        {
            Assert.IsTrue(_talentManager.IsMaxLevel("NotATalent"));
        }

        /// <summary>Each level carries its own lines, so every level owned has to be applied rather
        /// than the last one standing in for the rest.</summary>
        [Test]
        public void ApplyTalentBonuses_AppliesEveryLevelOwnedExactlyOnce()
        {
            var stats = new ShipStats(new ShipBaseStats());

            _talentManager.TryGrantLevel(TalentId);
            _talentManager.TryGrantLevel(TalentId);
            _talentManager.ApplyTalentBonuses(stats);

            _mockTalentConfig.Received(1).ApplyLevel(stats, 0);
            _mockTalentConfig.Received(1).ApplyLevel(stats, 1);
        }

        [Test]
        public void ApplyTalentBonuses_NeverAppliesALevelNotOwned()
        {
            var stats = new ShipStats(new ShipBaseStats());

            _talentManager.TryGrantLevel(TalentId);
            _talentManager.ApplyTalentBonuses(stats);

            _mockTalentConfig.Received(1).ApplyLevel(stats, 0);
            _mockTalentConfig.DidNotReceive().ApplyLevel(stats, 1);
        }

        [Test]
        public void ApplyTalentBonuses_WithNothingOwned_AppliesNoLevel()
        {
            var stats = new ShipStats(new ShipBaseStats());

            _talentManager.ApplyTalentBonuses(stats);

            _mockTalentConfig.DidNotReceive().ApplyLevel(Arg.Any<ShipStats>(), Arg.Any<int>());
        }

        /// <summary>A mode whose progression lasts one run wipes its store, and the wipe has to reach
        /// the levels, not just the file.</summary>
        [Test]
        public void ClearLoadedData_DropsEveryLevel()
        {
            _talentManager.TryGrantLevel(TalentId);

            _talentManager.ClearLoadedData();

            Assert.AreEqual(0, _talentManager.GetTalentLevel(TalentId));
        }

        private static TalentConfigSO CreateTalent(string talentId, params int[] levelCosts)
        {
            var config = Substitute.For<TalentConfigSO>();
            var levels = new List<TalentLevelDTO>();

            foreach (int cost in levelCosts)
            {
                levels.Add(new TalentLevelDTO(cost, new List<TalentModifierDTO>()));
            }

            config.ObjectID.Returns(talentId);
            config.Levels.Returns(levels);
            config.MaxLevel.Returns(levels.Count);

            return config;
        }
    }
}
