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
        private IGameModeScopedManager _mockModeScopedManager;
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

            _mockModeScopedManager = Substitute.For<IGameModeScopedManager>();

            Container.Bind<ISaveProfileManager>().FromInstance(mockSaveProfileManager);
            Container.Bind<IExpeditionMapService>().FromInstance(_mockMapService);
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
            Object.DestroyImmediate(_mockConfig);
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
