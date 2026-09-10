using NSubstitute;
using NUnit.Framework;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class ExpeditionModeServiceTests : ZenjectUnitTestFixture
    {
        private const int Score = 120;

        private static readonly GameSessionDTO _session = new(GameModeTypes.Expedition, 1, "Level 1");

        private ExpeditionModeService _expeditionModeService;
        private IExpeditionRunManager _mockExpeditionRunManager;
        private ITalentManager _mockTalentManager;
        private IEquipmentManager _mockEquipmentManager;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _mockExpeditionRunManager = Substitute.For<IExpeditionRunManager>();
            _mockTalentManager = Substitute.For<ITalentManager>();
            _mockEquipmentManager = Substitute.For<IEquipmentManager>();

            Container.Bind<IExpeditionRunManager>().FromInstance(_mockExpeditionRunManager);
            Container.Bind<ITalentManager>().FromInstance(_mockTalentManager);
            Container.Bind<IEquipmentManager>().FromInstance(_mockEquipmentManager);

            _expeditionModeService = Container.Instantiate<ExpeditionModeService>();
        }

        [Test]
        public void HubScene_IsTheExpeditionScene()
        {
            Assert.AreEqual(SceneTypes.Expedition, _expeditionModeService.HubScene);
        }

        /// <summary>The same machinery as Campaign, only against the Expedition profile.</summary>
        [Test]
        public void ApplyProgressionBonuses_AppliesTalentsEquipmentAndTheHealthCarried()
        {
            _mockExpeditionRunManager.RemainingHealthRatio.Returns(0.5f);
            var stats = new ShipStats(new ShipBaseStats());

            _expeditionModeService.ApplyProgressionBonuses(stats);

            _mockTalentManager.Received(1).ApplyTalentBonuses(stats);
            _mockEquipmentManager.Received(1).ApplyEquipmentBonuses(stats);
            Assert.AreEqual(stats.CurrentMaxHealth / 2, stats.CurrentHealth);
        }

        [Test]
        public void SaveLevelResult_CompletesTheCurrentNode()
        {
            var stats = new ShipStats(new ShipBaseStats());

            _expeditionModeService.SaveLevelResult(_session, stats);

            _mockExpeditionRunManager.Received(1).CompleteCurrentNode(stats);
        }

        [Test]
        public void SaveRunScore_AfterClearingTheLevel_BanksTheScoreAsScrap()
        {
            _expeditionModeService.SaveRunScore(CreateResult(GameplayStateResultTypes.LevelFinished), Score);

            _mockExpeditionRunManager.Received(1).BankScrap(Score);
            _mockExpeditionRunManager.DidNotReceive().EndRunInDefeat();
        }

        [Test]
        public void SaveRunScore_AfterADefeat_EndsTheRunAndPaysNothing()
        {
            _expeditionModeService.SaveRunScore(CreateResult(GameplayStateResultTypes.GameOver), Score);

            _mockExpeditionRunManager.Received(1).EndRunInDefeat();
            _mockExpeditionRunManager.DidNotReceive().BankScrap(Arg.Any<int>());
        }

        /// <summary>Leaving a level any other way is not a defeat, but it is still the end of the run.</summary>
        [Test]
        public void SaveRunScore_AfterQuitting_AbandonsTheRun()
        {
            _expeditionModeService.SaveRunScore(CreateResult(GameplayStateResultTypes.Quit), Score);

            _mockExpeditionRunManager.Received(1).AbandonRun();
            _mockExpeditionRunManager.DidNotReceive().BankScrap(Arg.Any<int>());
        }

        /// <summary>No options means no result screen at all, win or lose.</summary>
        [Test]
        public void GetGameOverOptions_OffersNothing()
        {
            Assert.AreEqual(GameOverOptionTypes.None,
                _expeditionModeService.GetGameOverOptions(CreateResult(GameplayStateResultTypes.LevelFinished)));
            Assert.AreEqual(GameOverOptionTypes.None,
                _expeditionModeService.GetGameOverOptions(CreateResult(GameplayStateResultTypes.GameOver)));
        }

        private static GameSessionResultDTO CreateResult(GameplayStateResultTypes result)
        {
            return new GameSessionResultDTO(_session, result);
        }
    }
}
