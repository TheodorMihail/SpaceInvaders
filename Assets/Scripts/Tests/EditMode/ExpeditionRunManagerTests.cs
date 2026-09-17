using System.Collections.Generic;
using BaseArchitecture.Core;
using NSubstitute;
using NUnit.Framework;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Expedition;
using SpaceInvaders.Scenes.Game;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class ExpeditionRunManagerTests : ZenjectUnitTestFixture
    {
        private const int StartNodeId = 0;
        private const int LevelNodeId = 1;
        private const int ShopNodeId = 2;
        private const int MegaBossNodeId = 3;
        private const int BossNodeId = 4;

        private const string TalentId = "Damage";

        private const string LevelId = "Level 1";
        private const string MegaBossLevelId = "Level 2";

        private const float ScrapPerScore = 2f;
        private const float ScrapPerHealthPercent = 3f;

        private const string OfferInstanceId = "offer-1";
        private const int OfferPrice = 150;

        private ExpeditionRunManager _expeditionRunManager;
        private IPersistenceManager _mockPersistenceManager;
        private IExpeditionMapService _mockMapService;
        private IExpeditionTalentDrawService _mockTalentDrawService;
        private IExpeditionShopService _mockShopService;
        private ITalentManager _mockTalentManager;
        private IInventoryManager _mockInventoryManager;
        private IEquipmentManager _mockEquipmentManager;
        private ICurrencyManager _mockCurrencyManager;
        private ILevelsRepository _mockLevelsRepository;
        private IGameModeScopedManager _mockModeScopedManager;
        private ExpeditionDataConfigSO _mockRunConfig;
        private ExpeditionRewardsDataConfigSO _mockRewardsConfig;
        private ExpeditionShopDataConfigSO _mockShopConfig;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _mockPersistenceManager = Substitute.For<IPersistenceManager>();
            _mockPersistenceManager
                .LoadVersioned<ExpeditionRunSaveData>(ExpeditionRunSaveData.SaveKey, ExpeditionRunSaveData.CurrentVersion)
                .Returns(new ExpeditionRunSaveData { Version = ExpeditionRunSaveData.CurrentVersion });

            var mockSaveProfileManager = Substitute.For<ISaveProfileManager>();
            mockSaveProfileManager.GetProfile(Arg.Any<GameModeTypes>()).Returns(_mockPersistenceManager);

            _mockMapService = Substitute.For<IExpeditionMapService>();
            _mockMapService.GenerateMap(Arg.Any<int>()).Returns(_ => CreateMap());

            _mockRunConfig = Substitute.For<ExpeditionDataConfigSO>();
            _mockRunConfig.CurrencyPerScore.Returns(ScrapPerScore);

            var mockGameModesRepository = Substitute.For<IGameModesRepository>();
            mockGameModesRepository.GetDataConfig(GameModeTypes.Expedition).Returns(_mockRunConfig);
            Container.Bind<IGameModesRepository>().FromInstance(mockGameModesRepository);

            _mockRewardsConfig = Substitute.For<ExpeditionRewardsDataConfigSO>();

            _mockShopConfig = Substitute.For<ExpeditionShopDataConfigSO>();
            _mockShopConfig.ScrapPerHealthPercent.Returns(ScrapPerHealthPercent);

            var mockExpeditionRepository = Substitute.For<IExpeditionRepository>();
            mockExpeditionRepository.GetRewardsDataConfig().Returns(_mockRewardsConfig);
            mockExpeditionRepository.GetShopDataConfig().Returns(_mockShopConfig);

            _mockCurrencyManager = Substitute.For<ICurrencyManager>();

            // Affordable by default, so only the tests about running short have to say so.
            _mockCurrencyManager.TrySpend(Arg.Any<int>()).Returns(true);

            _mockLevelsRepository = Substitute.For<ILevelsRepository>();
            _mockLevelsRepository.ContainsLevelConfig(Arg.Any<string>()).Returns(true);

            _mockModeScopedManager = Substitute.For<IGameModeScopedManager>();

            _mockTalentDrawService = Substitute.For<IExpeditionTalentDrawService>();
            _mockShopService = Substitute.For<IExpeditionShopService>();
            _mockShopService.RollOffers().Returns(_ => new List<ExpeditionShopOfferEntry>());

            _mockTalentManager = Substitute.For<ITalentManager>();
            Container.Bind<ITalentManager>().FromInstance(_mockTalentManager);

            _mockInventoryManager = Substitute.For<IInventoryManager>();
            _mockEquipmentManager = Substitute.For<IEquipmentManager>();

            Container.Bind<ISaveProfileManager>().FromInstance(mockSaveProfileManager);
            Container.Bind<IExpeditionMapService>().FromInstance(_mockMapService);
            Container.Bind<IExpeditionTalentDrawService>().FromInstance(_mockTalentDrawService);
            Container.Bind<IExpeditionShopService>().FromInstance(_mockShopService);
            Container.Bind<IInventoryManager>().FromInstance(_mockInventoryManager);
            Container.Bind<IEquipmentManager>().FromInstance(_mockEquipmentManager);
            Container.Bind<IExpeditionRepository>().FromInstance(mockExpeditionRepository);
            Container.Bind<ICurrencyManager>().FromInstance(_mockCurrencyManager);
            Container.Bind<ILevelsRepository>().FromInstance(_mockLevelsRepository);
            Container.Bind<IList<IGameModeScopedManager>>()
                .FromInstance(new List<IGameModeScopedManager> { _mockModeScopedManager });

            _expeditionRunManager = Container.Instantiate<ExpeditionRunManager>();
            _expeditionRunManager.Initialize();
        }

        [TearDown]
        public override void Teardown()
        {
            Object.DestroyImmediate(_mockRunConfig);
            Object.DestroyImmediate(_mockRewardsConfig);
            Object.DestroyImmediate(_mockShopConfig);
            base.Teardown();
        }

        [Test]
        public void StartNewExpedition_PutsTheGeneratedMapOnTheScreenAndNoLevelInProgress()
        {
            _expeditionRunManager.StartNewExpedition();

            Assert.IsNotNull(_expeditionRunManager.CurrentExpedition);
            Assert.IsFalse(_expeditionRunManager.CurrentExpedition.IsLevelInProgress);
            Assert.AreEqual(CreateMap().Count, _expeditionRunManager.CurrentExpedition.Nodes.Count);
        }

        /// <summary>Nothing exists to read until one is started.</summary>
        [Test]
        public void CurrentExpedition_WithoutOne_IsNull()
        {
            Assert.IsNull(_expeditionRunManager.CurrentExpedition);
        }

        /// <summary>The whole profile belongs to the expedition, so leaving one store behind arms the
        /// next one with it.</summary>
        [Test]
        public void AbandonExpedition_ClearsTheExpeditionAndEveryModeScopedStore()
        {
            _expeditionRunManager.StartNewExpedition();
            _mockModeScopedManager.ClearReceivedCalls();

            _expeditionRunManager.AbandonExpedition();

            Assert.IsNull(_expeditionRunManager.CurrentExpedition);
            _mockModeScopedManager.Received(1).ClearLoadedData();
        }

        [Test]
        public void StartNewExpedition_ClearsEveryModeScopedStore()
        {
            _expeditionRunManager.StartNewExpedition();

            _mockModeScopedManager.Received(1).ClearLoadedData();
        }

        [Test]
        public void EnterNode_WithALevelNode_MovesIntoTheLevel()
        {
            _expeditionRunManager.StartNewExpedition();

            _expeditionRunManager.EnterNode(LevelNodeId);

            Assert.IsTrue(_expeditionRunManager.CurrentExpedition.IsLevelInProgress);
        }

        [Test]
        public void EnterNode_WithANodeThatRunsNoLevel_StaysOnTheMap()
        {
            _expeditionRunManager.StartNewExpedition();

            _expeditionRunManager.EnterNode(ShopNodeId);

            Assert.IsFalse(_expeditionRunManager.CurrentExpedition.IsLevelInProgress);
        }

        [Test]
        public void EnterNode_WithAnUnreachableNode_DoesNothing()
        {
            _expeditionRunManager.StartNewExpedition();

            _expeditionRunManager.EnterNode(MegaBossNodeId);

            // The mega boss carries a level, so arriving there would have started one.
            Assert.IsFalse(_expeditionRunManager.CurrentExpedition.IsLevelInProgress);
        }

        [Test]
        public void TryGetCurrentLevelSession_OnALevelNode_DescribesThatLevel()
        {
            _expeditionRunManager.StartNewExpedition();
            _expeditionRunManager.EnterNode(LevelNodeId);

            bool found = _expeditionRunManager.TryGetCurrentLevelSession(out GameSessionDTO session);

            Assert.IsTrue(found);
            Assert.AreEqual(GameModeTypes.Expedition, session.Mode);
            Assert.AreEqual(LevelId, session.LevelId);
            Assert.AreEqual(1, session.LevelNumber);
        }

        [Test]
        public void TryGetCurrentLevelSession_OnANodeThatRunsNoLevel_FindsNothing()
        {
            _expeditionRunManager.StartNewExpedition();

            Assert.IsFalse(_expeditionRunManager.TryGetCurrentLevelSession(out GameSessionDTO _));
        }

        [Test]
        public void CompleteCurrentLevel_LeavesTheLevelAndCarriesTheHealthLeft()
        {
            _expeditionRunManager.StartNewExpedition();
            _expeditionRunManager.EnterNode(LevelNodeId);

            _expeditionRunManager.CompleteCurrentLevel(CreateResult(CreateStatsWithHalfHealth()));

            Assert.IsFalse(_expeditionRunManager.CurrentExpedition.IsLevelInProgress);
            Assert.AreEqual(0.5f, _expeditionRunManager.CurrentExpedition.RemainingHealthRatio, 0.01f);
        }

        [Test]
        public void CompleteCurrentLevel_PaysTheScoreAtTheAuthoredRate()
        {
            _expeditionRunManager.StartNewExpedition();
            _expeditionRunManager.EnterNode(LevelNodeId);

            _expeditionRunManager.CompleteCurrentLevel(CreateResult(null, 100));

            _mockCurrencyManager.Received(1).AddCurrency(200);
        }

        /// <summary>Nothing to complete means nothing to pay for either.</summary>
        [Test]
        public void CompleteCurrentLevel_OutsideALevel_DoesNothing()
        {
            _expeditionRunManager.StartNewExpedition();

            _expeditionRunManager.CompleteCurrentLevel(CreateResult(CreateStatsWithHalfHealth(), 100));

            Assert.AreEqual(1f, _expeditionRunManager.RemainingHealthRatio);
            _mockCurrencyManager.DidNotReceive().AddCurrency(Arg.Any<int>());
        }

        [Test]
        public void IsOnFinalLevel_OnlyOnTheMegaBoss()
        {
            _expeditionRunManager.StartNewExpedition();
            _expeditionRunManager.EnterNode(LevelNodeId);

            Assert.IsFalse(_expeditionRunManager.IsOnFinalLevel);

            _expeditionRunManager.CompleteCurrentLevel(CreateResult(null));
            _expeditionRunManager.EnterNode(MegaBossNodeId);

            Assert.IsTrue(_expeditionRunManager.IsOnFinalLevel);
        }

        /// <summary>Recorded and dropped in one step, so no screen has to be reached for it to count.</summary>
        [Test]
        public void FinishExpedition_ReportsWhereItReachedAndDropsIt()
        {
            _expeditionRunManager.StartNewExpedition();
            _expeditionRunManager.EnterNode(LevelNodeId);
            _mockModeScopedManager.ClearReceivedCalls();

            ExpeditionRunResultDTO result = _expeditionRunManager.FinishExpedition(ExpeditionRunResultTypes.Defeated);

            Assert.AreEqual(ExpeditionRunResultTypes.Defeated, result.Result);
            Assert.AreEqual(1, result.DepthReached);
            Assert.IsNull(_expeditionRunManager.CurrentExpedition);
            _mockModeScopedManager.Received(1).ClearLoadedData();
        }

        [Test]
        public void HasMissingLevels_WhenAMappedLevelIsGone_IsTrue()
        {
            _expeditionRunManager.StartNewExpedition();
            _mockLevelsRepository.ContainsLevelConfig(LevelId).Returns(false);

            Assert.IsTrue(_expeditionRunManager.HasMissingLevels);
        }

        [Test]
        public void HasMissingLevels_WithEveryMappedLevelPresent_IsFalse()
        {
            _expeditionRunManager.StartNewExpedition();

            Assert.IsFalse(_expeditionRunManager.HasMissingLevels);
        }

        /// <summary>Start, one level node and one shop at depth 1, both leading to the mega boss.</summary>
        /// <summary>Stocked on arrival rather than when the screen opens, so what was rolled survives
        /// the app closing in front of it.</summary>
        [Test]
        public void EnterNode_WithAShopNode_StocksTheShelf()
        {
            _mockShopService.RollOffers().Returns(_ => CreateOffers());
            _expeditionRunManager.StartNewExpedition();

            _expeditionRunManager.EnterNode(ShopNodeId);

            Assert.IsTrue(_expeditionRunManager.CurrentExpedition.HasOpenShop);
            Assert.AreEqual(1, _expeditionRunManager.CurrentExpedition.ShopOffers.Count);
        }

        /// <summary>An unauthored catalogue reads as a node walked past rather than an empty screen.</summary>
        [Test]
        public void EnterNode_WithNothingToStock_LeavesNoShopOpen()
        {
            _expeditionRunManager.StartNewExpedition();

            _expeditionRunManager.EnterNode(ShopNodeId);

            Assert.IsFalse(_expeditionRunManager.CurrentExpedition.HasOpenShop);
        }

        [Test]
        public void EnterNode_WithANodeThatIsNotAShop_LeavesNoShopOpen()
        {
            _mockShopService.RollOffers().Returns(_ => CreateOffers());
            _expeditionRunManager.StartNewExpedition();

            _expeditionRunManager.EnterNode(LevelNodeId);

            Assert.IsFalse(_expeditionRunManager.CurrentExpedition.HasOpenShop);
        }

        /// <summary>Paid for, owned and worn in one step, since a run has no reason to buy gear it
        /// leaves off the ship.</summary>
        [Test]
        public void TryBuyShopOffer_SpendsThenOwnsThenEquips()
        {
            _mockShopService.RollOffers().Returns(_ => CreateOffers());
            _expeditionRunManager.StartNewExpedition();
            _expeditionRunManager.EnterNode(ShopNodeId);

            bool bought = _expeditionRunManager.TryBuyShopOffer(OfferInstanceId);

            Assert.IsTrue(bought);
            Assert.IsTrue(_expeditionRunManager.CurrentExpedition.ShopOffers[0].IsSold);
            _mockCurrencyManager.Received(1).TrySpend(OfferPrice);
            _mockInventoryManager.Received(1).AddItems(Arg.Any<IReadOnlyList<InventoryItemEntry>>());
            _mockEquipmentManager.Received(1).Equip(OfferInstanceId);
        }

        [Test]
        public void TryBuyShopOffer_WithoutTheScrap_ChangesNothing()
        {
            _mockShopService.RollOffers().Returns(_ => CreateOffers());
            _mockCurrencyManager.TrySpend(Arg.Any<int>()).Returns(false);
            _expeditionRunManager.StartNewExpedition();
            _expeditionRunManager.EnterNode(ShopNodeId);

            bool bought = _expeditionRunManager.TryBuyShopOffer(OfferInstanceId);

            Assert.IsFalse(bought);
            Assert.IsFalse(_expeditionRunManager.CurrentExpedition.ShopOffers[0].IsSold);
            _mockEquipmentManager.DidNotReceive().Equip(Arg.Any<string>());
        }

        /// <summary>The same offer cannot be bought twice, however the screen got there.</summary>
        [Test]
        public void TryBuyShopOffer_AlreadySold_ChangesNothing()
        {
            _mockShopService.RollOffers().Returns(_ => CreateOffers());
            _expeditionRunManager.StartNewExpedition();
            _expeditionRunManager.EnterNode(ShopNodeId);
            _expeditionRunManager.TryBuyShopOffer(OfferInstanceId);
            _mockCurrencyManager.ClearReceivedCalls();

            bool bought = _expeditionRunManager.TryBuyShopOffer(OfferInstanceId);

            Assert.IsFalse(bought);
            _mockCurrencyManager.DidNotReceive().TrySpend(Arg.Any<int>());
        }

        /// <summary>Dropped on the way out, so a node cannot be shopped a second time.</summary>
        [Test]
        public void CloseShop_DropsTheShelf()
        {
            _mockShopService.RollOffers().Returns(_ => CreateOffers());
            _expeditionRunManager.StartNewExpedition();
            _expeditionRunManager.EnterNode(ShopNodeId);

            _expeditionRunManager.CloseShop();

            Assert.IsFalse(_expeditionRunManager.CurrentExpedition.HasOpenShop);
        }

        [Test]
        public void RepairCost_WithNothingMissing_IsNothing()
        {
            _expeditionRunManager.StartNewExpedition();

            Assert.AreEqual(0, _expeditionRunManager.CurrentExpedition.RepairCost);
        }

        /// <summary>Priced off the share missing, so a full mend costs the same whatever the hull.</summary>
        [Test]
        public void RepairCost_PricesTheShareMissing()
        {
            _expeditionRunManager.StartNewExpedition();
            _expeditionRunManager.EnterNode(LevelNodeId);
            _expeditionRunManager.CompleteCurrentLevel(CreateResult(CreateStatsWithHalfHealth()));

            Assert.AreEqual((int)(50 * ScrapPerHealthPercent), _expeditionRunManager.CurrentExpedition.RepairCost);
        }

        [Test]
        public void TryRepair_SpendsAndRestoresToFull()
        {
            _expeditionRunManager.StartNewExpedition();
            _expeditionRunManager.EnterNode(LevelNodeId);
            _expeditionRunManager.CompleteCurrentLevel(CreateResult(CreateStatsWithHalfHealth()));

            bool repaired = _expeditionRunManager.TryRepair();

            Assert.IsTrue(repaired);
            Assert.AreEqual(1f, _expeditionRunManager.CurrentExpedition.RemainingHealthRatio);
            _mockCurrencyManager.Received(1).TrySpend((int)(50 * ScrapPerHealthPercent));
        }

        /// <summary>Nothing to mend means nothing to charge for.</summary>
        [Test]
        public void TryRepair_WithNothingMissing_ChangesNothing()
        {
            _expeditionRunManager.StartNewExpedition();

            bool repaired = _expeditionRunManager.TryRepair();

            Assert.IsFalse(repaired);
            _mockCurrencyManager.DidNotReceive().TrySpend(Arg.Any<int>());
        }

        [Test]
        public void TryRepair_WithoutTheScrap_LeavesTheHealthAlone()
        {
            _mockCurrencyManager.TrySpend(Arg.Any<int>()).Returns(false);
            _expeditionRunManager.StartNewExpedition();
            _expeditionRunManager.EnterNode(LevelNodeId);
            _expeditionRunManager.CompleteCurrentLevel(CreateResult(CreateStatsWithHalfHealth()));

            bool repaired = _expeditionRunManager.TryRepair();

            Assert.IsFalse(repaired);
            Assert.AreEqual(0.5f, _expeditionRunManager.CurrentExpedition.RemainingHealthRatio, 0.01f);
        }

        private static List<ExpeditionNodeEntry> CreateMap()
        {
            return new List<ExpeditionNodeEntry>
            {
                new()
                {
                    Id = StartNodeId, Depth = 0, Column = 0,
                    NodeType = ExpeditionNodeTypes.Start.ToString(),
                    LevelId = string.Empty,
                    NextNodeIds = new List<int> { LevelNodeId, ShopNodeId, BossNodeId }
                },
                new()
                {
                    Id = LevelNodeId, Depth = 1, Column = 0,
                    NodeType = ExpeditionNodeTypes.Normal.ToString(),
                    LevelId = LevelId,
                    NextNodeIds = new List<int> { MegaBossNodeId }
                },
                new()
                {
                    Id = ShopNodeId, Depth = 1, Column = 1,
                    NodeType = ExpeditionNodeTypes.Shop.ToString(),
                    LevelId = string.Empty,
                    NextNodeIds = new List<int> { MegaBossNodeId }
                },
                new()
                {
                    Id = BossNodeId, Depth = 1, Column = 2,
                    NodeType = ExpeditionNodeTypes.Boss.ToString(),
                    LevelId = LevelId,
                    NextNodeIds = new List<int> { MegaBossNodeId }
                },
                new()
                {
                    Id = MegaBossNodeId, Depth = 2, Column = 0,
                    NodeType = ExpeditionNodeTypes.MegaBoss.ToString(),
                    LevelId = MegaBossLevelId,
                    NextNodeIds = new List<int>()
                }
            };
        }

        /// <summary>Cards are owed by the run, not held by a screen, so closing the app between the
        /// two a boss gives cannot eat the second.</summary>
        [Test]
        public void CompleteCurrentLevel_OnABoss_LeavesTheBossCountPending()
        {
            _mockRewardsConfig.TalentRewardCount.Returns(1);
            _mockRewardsConfig.BossTalentRewardCount.Returns(2);

            _expeditionRunManager.StartNewExpedition();
            _expeditionRunManager.EnterNode(BossNodeId);
            _expeditionRunManager.CompleteCurrentLevel(CreateResult(null));

            Assert.AreEqual(2, _expeditionRunManager.CurrentExpedition.PendingTalentRewards);
        }

        [Test]
        public void CompleteCurrentLevel_OnANormalLevel_LeavesTheNormalCountPending()
        {
            _mockRewardsConfig.TalentRewardCount.Returns(1);
            _mockRewardsConfig.BossTalentRewardCount.Returns(2);

            _expeditionRunManager.StartNewExpedition();
            _expeditionRunManager.EnterNode(LevelNodeId);
            _expeditionRunManager.CompleteCurrentLevel(CreateResult(null));

            Assert.AreEqual(1, _expeditionRunManager.CurrentExpedition.PendingTalentRewards);
        }

        [Test]
        public void GrantTalent_SpendsOnePendingCardAndAddsTheLevel()
        {
            _mockRewardsConfig.BossTalentRewardCount.Returns(2);

            _expeditionRunManager.StartNewExpedition();
            _expeditionRunManager.EnterNode(BossNodeId);
            _expeditionRunManager.CompleteCurrentLevel(CreateResult(null));

            _expeditionRunManager.GrantTalent(TalentId);

            Assert.AreEqual(1, _expeditionRunManager.CurrentExpedition.PendingTalentRewards);
            _mockTalentManager.Received(1).TryGrantLevel(TalentId);
        }

        /// <summary>The card is spent either way, so an offer with nothing to draw cannot leave one
        /// pending forever.</summary>
        [Test]
        public void GrantTalent_WithNothingToGrant_StillSpendsTheCard()
        {
            _mockRewardsConfig.TalentRewardCount.Returns(1);

            _expeditionRunManager.StartNewExpedition();
            _expeditionRunManager.EnterNode(LevelNodeId);
            _expeditionRunManager.CompleteCurrentLevel(CreateResult(null));

            _expeditionRunManager.GrantTalent(null);

            Assert.AreEqual(0, _expeditionRunManager.CurrentExpedition.PendingTalentRewards);
        }

        [Test]
        public void GrantTalent_WithNothingPending_AddsNoLevel()
        {
            _expeditionRunManager.StartNewExpedition();

            _expeditionRunManager.GrantTalent(TalentId);

            _mockTalentManager.DidNotReceive().TryGrantLevel(Arg.Any<string>());
        }

        private static List<ExpeditionShopOfferEntry> CreateOffers()
        {
            return new List<ExpeditionShopOfferEntry>
            {
                new()
                {
                    Item = new InventoryItemEntry { InstanceId = OfferInstanceId, ItemId = "Core" },
                    Price = OfferPrice
                }
            };
        }

        private static GameSessionResultDTO CreateResult(ShipStats stats, int score = 0)
        {
            var session = new GameSessionDTO(GameModeTypes.Expedition, 1, LevelId);

            return new GameSessionResultDTO(session, GameplayStateResultTypes.LevelFinished, score, stats);
        }

        private static ShipStats CreateStatsWithHalfHealth()
        {
            var stats = new ShipStats(new ShipBaseStats());
            stats.ApplyDamage(stats.CurrentMaxHealth / 2);

            return stats;
        }
    }
}
