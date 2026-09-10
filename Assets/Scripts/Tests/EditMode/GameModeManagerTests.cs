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
        private const string MissingRulesError =
            "[GameModeManager] [Error] No game mode rules are bound for Campaign.";

        private static readonly GameSessionDTO _session = new(GameModeTypes.Campaign, 1, "Level 1");

        private IGameModeRules _mockCampaignRules;
        private IGameModeRules _mockExpeditionRules;
        private IGameModeScopedManager _mockModeScopedManager;
        private List<IGameModeScopedManager> _modeScopedManagers;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _mockModeScopedManager = Substitute.For<IGameModeScopedManager>();
            _modeScopedManagers = new List<IGameModeScopedManager> { _mockModeScopedManager };

            _mockCampaignRules = Substitute.For<IGameModeRules>();
            _mockCampaignRules.Mode.Returns(GameModeTypes.Campaign);

            _mockExpeditionRules = Substitute.For<IGameModeRules>();
            _mockExpeditionRules.Mode.Returns(GameModeTypes.Expedition);
        }

        /// <summary>The whole point of the manager: the same call reaches a different mode's rules.</summary>
        [Test]
        public void InitializeGameMode_RoutesToTheRulesForThatModeAndNoOther()
        {
            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignRules, _mockExpeditionRules);
            var stats = new ShipStats(new ShipBaseStats());

            gameModeManager.InitializeGameMode(GameModeTypes.Expedition);
            gameModeManager.ApplyProgressionBonuses(stats);

            _mockExpeditionRules.Received(1).ApplyProgressionBonuses(stats);
            _mockCampaignRules.DidNotReceive().ApplyProgressionBonuses(Arg.Any<ShipStats>());
        }

        [Test]
        public void InitializeGameMode_SwitchingBack_RoutesToTheOriginalRules()
        {
            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignRules, _mockExpeditionRules);
            var stats = new ShipStats(new ShipBaseStats());

            gameModeManager.InitializeGameMode(GameModeTypes.Expedition);
            gameModeManager.InitializeGameMode(GameModeTypes.Campaign);
            gameModeManager.ApplyProgressionBonuses(stats);

            _mockCampaignRules.Received(1).ApplyProgressionBonuses(stats);
            _mockExpeditionRules.DidNotReceive().ApplyProgressionBonuses(Arg.Any<ShipStats>());
        }

        [Test]
        public void HubScene_FollowsTheModeThatWasInitialized()
        {
            _mockCampaignRules.HubScene.Returns(SceneTypes.Campaign);
            _mockExpeditionRules.HubScene.Returns(SceneTypes.Expedition);

            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignRules, _mockExpeditionRules);

            Assert.AreEqual(SceneTypes.Campaign, gameModeManager.HubScene);

            gameModeManager.InitializeGameMode(GameModeTypes.Expedition);

            Assert.AreEqual(SceneTypes.Expedition, gameModeManager.HubScene);
        }

        [Test]
        public void CurrentMode_DefaultsToCampaign()
        {
            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignRules);

            Assert.AreEqual(GameModeTypes.Campaign, gameModeManager.CurrentMode);
        }

        [Test]
        public void InitializeGameMode_StoresTheMode()
        {
            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignRules);

            gameModeManager.InitializeGameMode(GameModeTypes.Campaign);

            Assert.AreEqual(GameModeTypes.Campaign, gameModeManager.CurrentMode);
        }

        /// <summary>Progression is stored per mode, so switching mode has to reload every store.</summary>
        [Test]
        public void InitializeGameMode_ReloadsEveryModeScopedManagerForThatMode()
        {
            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignRules, _mockExpeditionRules);

            gameModeManager.InitializeGameMode(GameModeTypes.Expedition);

            _mockModeScopedManager.Received(1).LoadForMode(GameModeTypes.Expedition);
        }

        [Test]
        public void HubScene_ComesFromTheRulesForTheCurrentMode()
        {
            _mockCampaignRules.HubScene.Returns(SceneTypes.Preload);
            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignRules);

            Assert.AreEqual(SceneTypes.Preload, gameModeManager.HubScene);
        }

        [Test]
        public void ApplyProgressionBonuses_ReachesTheRulesForTheCurrentMode()
        {
            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignRules);
            var stats = new ShipStats(new ShipBaseStats());

            gameModeManager.ApplyProgressionBonuses(stats);

            _mockCampaignRules.Received(1).ApplyProgressionBonuses(stats);
        }

        [Test]
        public void ResolveGameEnd_ReachesTheRulesForTheCurrentMode()
        {
            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignRules);
            var result = new GameSessionResultDTO(_session, GameplayStateResultTypes.LevelFinished, 120);

            gameModeManager.ResolveGameEnd(result);

            _mockCampaignRules.Received(1).ResolveGameEnd(result);
        }

        [Test]
        public void ResolveGameEnd_ReturnsWhatTheRulesForTheCurrentModeDecides()
        {
            var result = new GameSessionResultDTO(_session, GameplayStateResultTypes.LevelFinished, 120);
            _mockCampaignRules.ResolveGameEnd(result)
                .Returns(new GameEndResolutionDTO(GameEndOptionTypes.NextLevel));
            GameModeManager gameModeManager = CreateInitializedManagerWith(_mockCampaignRules);

            Assert.AreEqual(GameEndOptionTypes.NextLevel, gameModeManager.ResolveGameEnd(result).Options);
        }

        [Test]
        public void WithNoRulesForTheCurrentMode_LogsAnErrorOnceAndForwardsNothing()
        {
            LogAssert.Expect(LogType.Error, MissingRulesError);

            GameModeManager gameModeManager = CreateInitializedManagerWith();

            Assert.DoesNotThrow(() => gameModeManager.ApplyProgressionBonuses(new ShipStats(new ShipBaseStats())));
            Assert.AreEqual(SceneTypes.MainMenu, gameModeManager.HubScene);
        }

        private GameModeManager CreateInitializedManagerWith(params IGameModeRules[] modeRules)
        {
            Container.Bind<IList<IGameModeRules>>().FromInstance(new List<IGameModeRules>(modeRules));
            Container.Bind<IList<IGameModeScopedManager>>().FromInstance(_modeScopedManagers);

            GameModeManager gameModeManager = Container.Instantiate<GameModeManager>();
            gameModeManager.Initialize();

            return gameModeManager;
        }
    }
}
