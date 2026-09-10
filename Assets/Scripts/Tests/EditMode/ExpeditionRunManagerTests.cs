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

        private const string LevelId = "Level 1";
        private const string MegaBossLevelId = "Level 2";

        private const float ScrapPerScore = 2f;

        private ExpeditionRunManager _expeditionRunManager;
        private IPersistenceManager _mockPersistenceManager;
        private IExpeditionMapService _mockMapService;
        private ICurrencyManager _mockCurrencyManager;
        private ILevelsRepository _mockLevelsRepository;
        private IModeScopedManager _mockModeScopedManager;
        private ExpeditionDataConfigSO _mockConfig;

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

            _mockConfig = Substitute.For<ExpeditionDataConfigSO>();
            _mockConfig.ScrapPerScore.Returns(ScrapPerScore);

            var mockExpeditionRepository = Substitute.For<IExpeditionRepository>();
            mockExpeditionRepository.GetExpeditionDataConfig().Returns(_mockConfig);

            _mockCurrencyManager = Substitute.For<ICurrencyManager>();

            _mockLevelsRepository = Substitute.For<ILevelsRepository>();
            _mockLevelsRepository.ContainsLevelConfig(Arg.Any<string>()).Returns(true);

            _mockModeScopedManager = Substitute.For<IModeScopedManager>();

            Container.Bind<ISaveProfileManager>().FromInstance(mockSaveProfileManager);
            Container.Bind<IExpeditionMapService>().FromInstance(_mockMapService);
            Container.Bind<IExpeditionRepository>().FromInstance(mockExpeditionRepository);
            Container.Bind<ICurrencyManager>().FromInstance(_mockCurrencyManager);
            Container.Bind<ILevelsRepository>().FromInstance(_mockLevelsRepository);
            Container.Bind<IList<IModeScopedManager>>()
                .FromInstance(new List<IModeScopedManager> { _mockModeScopedManager });

            _expeditionRunManager = Container.Instantiate<ExpeditionRunManager>();
            _expeditionRunManager.Initialize();
        }

        [TearDown]
        public override void Teardown()
        {
            Object.DestroyImmediate(_mockConfig);
            base.Teardown();
        }

        [Test]
        public void StartNewRun_PutsThePlayerOnTheStartNodeAndOnTheMap()
        {
            _expeditionRunManager.StartNewRun();

            Assert.AreEqual(ExpeditionRunPhaseTypes.OnMap, _expeditionRunManager.RunPhase);
            Assert.AreEqual(StartNodeId, _expeditionRunManager.CurrentNodeId);
            Assert.IsTrue(_expeditionRunManager.HasActiveRun);
        }

        /// <summary>The whole profile is the run's, so leaving one store behind arms the next run with it.</summary>
        [Test]
        public void AbandonRun_ClearsTheRunAndEveryModeScopedStore()
        {
            _expeditionRunManager.StartNewRun();
            _mockModeScopedManager.ClearReceivedCalls();

            _expeditionRunManager.AbandonRun();

            Assert.AreEqual(ExpeditionRunPhaseTypes.None, _expeditionRunManager.RunPhase);
            Assert.IsFalse(_expeditionRunManager.HasActiveRun);
            Assert.AreEqual(0, _expeditionRunManager.Nodes.Count);
            _mockModeScopedManager.Received(1).ClearLoadedData();
        }

        [Test]
        public void StartNewRun_ClearsEveryModeScopedStore()
        {
            _expeditionRunManager.StartNewRun();

            _mockModeScopedManager.Received(1).ClearLoadedData();
        }

        [Test]
        public void EnterNode_WithALevelNode_MovesIntoTheLevel()
        {
            _expeditionRunManager.StartNewRun();

            _expeditionRunManager.EnterNode(LevelNodeId);

            Assert.AreEqual(ExpeditionRunPhaseTypes.InLevel, _expeditionRunManager.RunPhase);
        }

        [Test]
        public void EnterNode_WithANodeThatRunsNoLevel_StaysOnTheMap()
        {
            _expeditionRunManager.StartNewRun();

            _expeditionRunManager.EnterNode(ShopNodeId);

            Assert.AreEqual(ExpeditionRunPhaseTypes.OnMap, _expeditionRunManager.RunPhase);
        }

        [Test]
        public void EnterNode_WithAnUnreachableNode_DoesNothing()
        {
            _expeditionRunManager.StartNewRun();

            _expeditionRunManager.EnterNode(MegaBossNodeId);

            Assert.AreEqual(StartNodeId, _expeditionRunManager.CurrentNodeId);
            Assert.AreEqual(ExpeditionRunPhaseTypes.OnMap, _expeditionRunManager.RunPhase);
        }

        [Test]
        public void TryGetCurrentNodeSession_OnALevelNode_DescribesThatLevel()
        {
            _expeditionRunManager.StartNewRun();
            _expeditionRunManager.EnterNode(LevelNodeId);

            bool found = _expeditionRunManager.TryGetCurrentNodeSession(out GameSessionDTO session);

            Assert.IsTrue(found);
            Assert.AreEqual(GameModeTypes.Expedition, session.Mode);
            Assert.AreEqual(LevelId, session.LevelId);
            Assert.AreEqual(1, session.LevelNumber);
        }

        [Test]
        public void TryGetCurrentNodeSession_OnANodeThatRunsNoLevel_FindsNothing()
        {
            _expeditionRunManager.StartNewRun();

            Assert.IsFalse(_expeditionRunManager.TryGetCurrentNodeSession(out GameSessionDTO _));
        }

        [Test]
        public void CompleteCurrentNode_MarksTheNodeClearedAndCarriesTheHealthLeft()
        {
            _expeditionRunManager.StartNewRun();
            _expeditionRunManager.EnterNode(LevelNodeId);

            _expeditionRunManager.CompleteCurrentNode(CreateStatsWithHalfHealth());

            Assert.AreEqual(ExpeditionRunPhaseTypes.NodeCleared, _expeditionRunManager.RunPhase);
            Assert.AreEqual(0.5f, _expeditionRunManager.RemainingHealthRatio, 0.01f);
        }

        [Test]
        public void CompleteCurrentNode_OnTheMegaBoss_FinishesTheRun()
        {
            _expeditionRunManager.StartNewRun();
            _expeditionRunManager.EnterNode(LevelNodeId);
            _expeditionRunManager.CompleteCurrentNode(null);
            _expeditionRunManager.ReturnToMap();
            _expeditionRunManager.EnterNode(MegaBossNodeId);

            _expeditionRunManager.CompleteCurrentNode(null);

            Assert.AreEqual(ExpeditionRunPhaseTypes.Finished, _expeditionRunManager.RunPhase);
            Assert.AreEqual(ExpeditionRunResultTypes.Completed, _expeditionRunManager.ConsumeRunResult().Result);
        }

        [Test]
        public void CompleteCurrentNode_OutsideALevel_DoesNothing()
        {
            _expeditionRunManager.StartNewRun();

            _expeditionRunManager.CompleteCurrentNode(CreateStatsWithHalfHealth());

            Assert.AreEqual(ExpeditionRunPhaseTypes.OnMap, _expeditionRunManager.RunPhase);
        }

        [Test]
        public void BankScrap_PaysTheScoreAtTheAuthoredRate()
        {
            _expeditionRunManager.StartNewRun();

            _expeditionRunManager.BankScrap(100);

            _mockCurrencyManager.Received(1).AddCurrency(200);
        }

        [Test]
        public void EndRunInDefeat_FromInsideALevel_FinishesTheRunAsADefeat()
        {
            _expeditionRunManager.StartNewRun();
            _expeditionRunManager.EnterNode(LevelNodeId);

            _expeditionRunManager.EndRunInDefeat();

            Assert.AreEqual(ExpeditionRunPhaseTypes.Finished, _expeditionRunManager.RunPhase);
            Assert.AreEqual(ExpeditionRunResultTypes.Defeated, _expeditionRunManager.ConsumeRunResult().Result);
        }

        /// <summary>A run is only ever reported once, so reading its result drops it.</summary>
        [Test]
        public void ConsumeRunResult_EndsTheRun()
        {
            _expeditionRunManager.StartNewRun();
            _expeditionRunManager.EnterNode(LevelNodeId);
            _expeditionRunManager.EndRunInDefeat();

            ExpeditionRunResultDTO result = _expeditionRunManager.ConsumeRunResult();

            Assert.AreEqual(1, result.DepthReached);
            Assert.IsFalse(_expeditionRunManager.HasActiveRun);
        }

        [Test]
        public void HasMissingLevels_WhenAMappedLevelIsGone_IsTrue()
        {
            _expeditionRunManager.StartNewRun();
            _mockLevelsRepository.ContainsLevelConfig(LevelId).Returns(false);

            Assert.IsTrue(_expeditionRunManager.HasMissingLevels);
        }

        [Test]
        public void HasMissingLevels_WithEveryMappedLevelPresent_IsFalse()
        {
            _expeditionRunManager.StartNewRun();

            Assert.IsFalse(_expeditionRunManager.HasMissingLevels);
        }

        /// <summary>Start, one level node and one shop at depth 1, both leading to the mega boss.</summary>
        private static List<ExpeditionNodeEntry> CreateMap()
        {
            return new List<ExpeditionNodeEntry>
            {
                new()
                {
                    Id = StartNodeId, Depth = 0, Column = 0,
                    NodeType = ExpeditionNodeTypes.Start.ToString(),
                    LevelId = string.Empty,
                    NextNodeIds = new List<int> { LevelNodeId, ShopNodeId }
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
                    Id = MegaBossNodeId, Depth = 2, Column = 0,
                    NodeType = ExpeditionNodeTypes.MegaBoss.ToString(),
                    LevelId = MegaBossLevelId,
                    NextNodeIds = new List<int>()
                }
            };
        }

        private static ShipStats CreateStatsWithHalfHealth()
        {
            var stats = new ShipStats(new ShipBaseStats());
            stats.ApplyDamage(stats.CurrentMaxHealth / 2);

            return stats;
        }
    }
}
