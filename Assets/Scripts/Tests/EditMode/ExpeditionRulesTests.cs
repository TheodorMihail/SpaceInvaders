using NSubstitute;
using NUnit.Framework;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class ExpeditionRulesTests : ZenjectUnitTestFixture
    {
        private const int Score = 120;

        private static readonly GameSessionDTO _session = new(GameModeTypes.Expedition, 1, "Level 1");

        private ExpeditionRules _expeditionRules;
        private IExpeditionRunManager _mockExpeditionRunManager;
        private IExpeditionState _mockExpedition;
        private ITalentManager _mockTalentManager;
        private IEquipmentManager _mockEquipmentManager;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _mockExpedition = Substitute.For<IExpeditionState>();
            _mockExpeditionRunManager = Substitute.For<IExpeditionRunManager>();
            _mockExpeditionRunManager.CurrentExpedition.Returns(_mockExpedition);
            _mockTalentManager = Substitute.For<ITalentManager>();
            _mockEquipmentManager = Substitute.For<IEquipmentManager>();

            Container.Bind<IExpeditionRunManager>().FromInstance(_mockExpeditionRunManager);
            Container.Bind<ITalentManager>().FromInstance(_mockTalentManager);
            Container.Bind<IEquipmentManager>().FromInstance(_mockEquipmentManager);

            _expeditionRules = Container.Instantiate<ExpeditionRules>();
        }

        [Test]
        public void HubScene_IsTheExpeditionScene()
        {
            Assert.AreEqual(SceneTypes.Expedition, _expeditionRules.HubScene);
        }

        /// <summary>A node is spent the moment it is entered, so nothing can be replayed.</summary>
        [Test]
        public void CanReplayLevel_IsFalse()
        {
            Assert.IsFalse(_expeditionRules.CanReplayLevel);
        }

        /// <summary>Gear comes from the shop, so kills roll against a table of its own.</summary>
        [Test]
        public void DropTableType_IsTheExpeditionTable()
        {
            Assert.AreEqual(DropTableTypes.Expedition, _expeditionRules.DropTableType);
        }

        /// <summary>The same machinery as Campaign, only against the Expedition profile.</summary>
        [Test]
        public void ApplyProgressionBonuses_AppliesTalentsEquipmentAndTheHealthCarried()
        {
            _mockExpedition.RemainingHealthRatio.Returns(0.5f);
            var stats = new ShipStats(new ShipBaseStats());

            _expeditionRules.ApplyProgressionBonuses(stats);

            _mockTalentManager.Received(1).ApplyTalentBonuses(stats);
            _mockEquipmentManager.Received(1).ApplyEquipmentBonuses(stats);
            Assert.AreEqual(stats.CurrentMaxHealth / 2, stats.CurrentHealth);
        }

        [Test]
        public void ResolveGameEnd_AfterClearingALevel_CompletesItAndKeepsTheExpedition()
        {
            GameSessionResultDTO result = CreateResult(GameplayStateResultTypes.LevelFinished,
                new ShipStats(new ShipBaseStats()));

            _expeditionRules.ResolveGameEnd(result);

            _mockExpeditionRunManager.Received(1).CompleteCurrentLevel(result);
            _mockExpeditionRunManager.DidNotReceive().FinishExpedition(Arg.Any<ExpeditionRunResultTypes>());
        }

        [Test]
        public void ResolveGameEnd_AfterClearingTheFinalLevel_FinishesTheExpedition()
        {
            _mockExpedition.IsOnFinalLevel.Returns(true);

            _expeditionRules.ResolveGameEnd(CreateResult(GameplayStateResultTypes.LevelFinished));

            _mockExpeditionRunManager.Received(1).FinishExpedition(ExpeditionRunResultTypes.Completed);
        }

        [Test]
        public void ResolveGameEnd_AfterADefeat_EndsTheExpeditionAndPaysNothing()
        {
            _expeditionRules.ResolveGameEnd(CreateResult(GameplayStateResultTypes.GameOver));

            _mockExpeditionRunManager.Received(1).FinishExpedition(ExpeditionRunResultTypes.Defeated);
            _mockExpeditionRunManager.DidNotReceive().CompleteCurrentLevel(Arg.Any<GameSessionResultDTO>());
        }

        /// <summary>The result rides the scene change rather than waiting on disk to be read.</summary>
        [Test]
        public void ResolveGameEnd_WhenTheExpeditionEnds_HandsTheResultToTheHubScene()
        {
            var expeditionResult = new ExpeditionRunResultDTO(ExpeditionRunResultTypes.Defeated, 4);
            _mockExpeditionRunManager.FinishExpedition(Arg.Any<ExpeditionRunResultTypes>()).Returns(expeditionResult);

            GameEndResolutionDTO resolution =
                _expeditionRules.ResolveGameEnd(CreateResult(GameplayStateResultTypes.GameOver));

            Assert.AreEqual(1, resolution.HubSceneParams.Length);
            Assert.AreEqual(expeditionResult, resolution.HubSceneParams[0]);
        }

        [Test]
        public void ResolveGameEnd_AfterClearingALevel_HandsTheHubNothing()
        {
            GameEndResolutionDTO resolution =
                _expeditionRules.ResolveGameEnd(CreateResult(GameplayStateResultTypes.LevelFinished));

            Assert.AreEqual(0, resolution.HubSceneParams.Length);
        }

        /// <summary>Quitting is not a defeat, but the node is spent, so the expedition ends and is
        /// still reported rather than vanishing.</summary>
        [Test]
        public void ResolveGameEnd_AfterQuitting_EndsTheExpeditionAsAbandoned()
        {
            _expeditionRules.ResolveGameEnd(CreateResult(GameplayStateResultTypes.Quit));

            _mockExpeditionRunManager.Received(1).FinishExpedition(ExpeditionRunResultTypes.Abandoned);
            _mockExpeditionRunManager.DidNotReceive().CompleteCurrentLevel(Arg.Any<GameSessionResultDTO>());
        }

        [Test]
        public void ResolveGameEnd_AfterQuitting_HandsTheResultToTheHubScene()
        {
            var expeditionResult = new ExpeditionRunResultDTO(ExpeditionRunResultTypes.Abandoned, 2);
            _mockExpeditionRunManager.FinishExpedition(Arg.Any<ExpeditionRunResultTypes>()).Returns(expeditionResult);

            GameEndResolutionDTO resolution =
                _expeditionRules.ResolveGameEnd(CreateResult(GameplayStateResultTypes.Quit));

            Assert.AreEqual(1, resolution.HubSceneParams.Length);
            Assert.AreEqual(expeditionResult, resolution.HubSceneParams[0]);
        }

        /// <summary>No options means no result screen at all, win or lose.</summary>
        [Test]
        public void ResolveGameEnd_OffersNoResultScreen()
        {
            Assert.AreEqual(GameEndOptionTypes.None,
                _expeditionRules.ResolveGameEnd(CreateResult(GameplayStateResultTypes.LevelFinished)).Options);
            Assert.AreEqual(GameEndOptionTypes.None,
                _expeditionRules.ResolveGameEnd(CreateResult(GameplayStateResultTypes.GameOver)).Options);
        }

        private static GameSessionResultDTO CreateResult(GameplayStateResultTypes result, ShipStats stats = null)
        {
            return new GameSessionResultDTO(_session, result, Score, stats);
        }
    }
}
