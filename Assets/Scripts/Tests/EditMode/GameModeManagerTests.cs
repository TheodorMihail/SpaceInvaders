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
    /// <summary>
    /// Selection between modes is only exercised once a second mode exists; until then these cover
    /// that every call reaches the service for the running mode and that a missing one is survivable.
    /// </summary>
    [TestFixture]
    public class GameModeManagerTests : ZenjectUnitTestFixture
    {
        private const string MissingServiceError =
            "[GameModeManager] [Error] No game mode service is bound for Campaign.";

        private static readonly GameSessionDTO _session = new(GameModeTypes.Campaign, 1);

        private IGameModeService _mockCampaignService;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _mockCampaignService = Substitute.For<IGameModeService>();
            _mockCampaignService.Mode.Returns(GameModeTypes.Campaign);
        }

        [Test]
        public void CurrentMode_DefaultsToCampaign()
        {
            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignService);

            Assert.AreEqual(GameModeTypes.Campaign, gameModeManager.CurrentMode);
        }

        [Test]
        public void InitializeMode_StoresTheMode()
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
