using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using UnityEngine;
using UnityEngine.TestTools;
using Zenject;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class GameModeManagerTests : ZenjectUnitTestFixture
    {
        private const string MissingServiceError =
            "[GameModeManager] [Error] No game mode service is bound for Campaign.";

        private static readonly GameSessionDTO _session = new(GameModeTypes.Campaign, 1);

        private IGameModeService _mockCampaignService;
        private IGameModeService _mockExpeditionService;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _mockCampaignService = Substitute.For<IGameModeService>();
            _mockCampaignService.Mode.Returns(GameModeTypes.Campaign);

            _mockExpeditionService = Substitute.For<IGameModeService>();
            _mockExpeditionService.Mode.Returns(GameModeTypes.Expedition);
        }

        /// <summary>The whole point of the manager: the same call reaches a different service.</summary>
        [Test]
        public void InitializeGameMode_RoutesToTheServiceForThatModeAndNoOther()
        {
            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignService, _mockExpeditionService);
            var stats = new ShipStats(new ShipBaseStats());

            gameModeManager.InitializeGameMode(GameModeTypes.Expedition);
            gameModeManager.ApplyProgressionBonuses(stats);

            _mockExpeditionService.Received(1).ApplyProgressionBonuses(stats);
            _mockCampaignService.DidNotReceive().ApplyProgressionBonuses(Arg.Any<ShipStats>());
        }

        [Test]
        public void InitializeGameMode_SwitchingBack_RoutesToTheOriginalService()
        {
            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignService, _mockExpeditionService);
            var stats = new ShipStats(new ShipBaseStats());

            gameModeManager.InitializeGameMode(GameModeTypes.Expedition);
            gameModeManager.InitializeGameMode(GameModeTypes.Campaign);
            gameModeManager.ApplyProgressionBonuses(stats);

            _mockCampaignService.Received(1).ApplyProgressionBonuses(stats);
            _mockExpeditionService.DidNotReceive().ApplyProgressionBonuses(Arg.Any<ShipStats>());
        }

        [Test]
        public void HubScene_FollowsTheModeThatWasInitialized()
        {
            _mockCampaignService.HubScene.Returns(SceneTypes.Campaign);
            _mockExpeditionService.HubScene.Returns(SceneTypes.Expedition);

            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignService, _mockExpeditionService);

            Assert.AreEqual(SceneTypes.Campaign, gameModeManager.HubScene);

            gameModeManager.InitializeGameMode(GameModeTypes.Expedition);

            Assert.AreEqual(SceneTypes.Expedition, gameModeManager.HubScene);
        }

        [Test]
        public void CurrentMode_DefaultsToCampaign()
        {
            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignService);

            Assert.AreEqual(GameModeTypes.Campaign, gameModeManager.CurrentMode);
        }

        [Test]
        public void InitializeGameMode_StoresTheMode()
        {
            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignService);

            gameModeManager.InitializeGameMode(GameModeTypes.Campaign);

            Assert.AreEqual(GameModeTypes.Campaign, gameModeManager.CurrentMode);
        }

        [Test]
        public void HubScene_ComesFromTheServiceForTheCurrentMode()
        {
            _mockCampaignService.HubScene.Returns(SceneTypes.Preload);
            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignService);

            Assert.AreEqual(SceneTypes.Preload, gameModeManager.HubScene);
        }

        [Test]
        public void ApplyProgressionBonuses_ReachesTheServiceForTheCurrentMode()
        {
            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignService);
            var stats = new ShipStats(new ShipBaseStats());

            gameModeManager.ApplyProgressionBonuses(stats);

            _mockCampaignService.Received(1).ApplyProgressionBonuses(stats);
        }

        [Test]
        public void SaveLevelResult_ReachesTheServiceForTheCurrentMode()
        {
            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignService);
            var stats = new ShipStats(new ShipBaseStats());

            gameModeManager.SaveLevelResult(_session, stats);

            _mockCampaignService.Received(1).SaveLevelResult(_session, stats);
        }

        [Test]
        public void SaveRunScore_ReachesTheServiceForTheCurrentMode()
        {
            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignService);
            var result = new GameSessionResultDTO(_session, GameplayStateResultTypes.LevelFinished);

            gameModeManager.SaveRunScore(result, 120);

            _mockCampaignService.Received(1).SaveRunScore(result, 120);
        }

        [Test]
        public void GetGameOverOptions_ReturnsWhatTheServiceForTheCurrentModeDecides()
        {
            var result = new GameSessionResultDTO(_session, GameplayStateResultTypes.LevelFinished);
            _mockCampaignService.GetGameOverOptions(result).Returns(GameOverOptionTypes.NextLevel);
            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignService);

            Assert.AreEqual(GameOverOptionTypes.NextLevel, gameModeManager.GetGameOverOptions(result));
        }

        [Test]
        public void WithNoServiceForTheCurrentMode_LogsAnErrorOnceAndForwardsNothing()
        {
            LogAssert.Expect(LogType.Error, MissingServiceError);

            GameModeManager gameModeManager = CreateInitializedManagerWith();

            Assert.DoesNotThrow(() => gameModeManager.ApplyProgressionBonuses(new ShipStats(new ShipBaseStats())));
            Assert.AreEqual(SceneTypes.MainMenu, gameModeManager.HubScene);
        }

        private GameModeManager CreateInitializedManagerWith(params IGameModeService[] services)
        {
            Container.Bind<IList<IGameModeService>>().FromInstance(new List<IGameModeService>(services));

            GameModeManager gameModeManager = Container.Instantiate<GameModeManager>();
            gameModeManager.Initialize();

            return gameModeManager;
        }
    }
}
