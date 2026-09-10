using NSubstitute;
using NUnit.Framework;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class CampaignRulesTests : ZenjectUnitTestFixture
    {
        private const int ThreeStarMaxDamage = 10;
        private const float TwoStarDamageMultiplier = 3f;

        private static readonly GameSessionDTO _session = new(GameModeTypes.Campaign, 1, "Level 1");

        private CampaignRules _campaignRules;
        private ILevelsRepository _mockLevelsRepository;
        private ILevelProgressManager _mockLevelProgressManager;
        private ITalentManager _mockTalentManager;
        private IEquipmentManager _mockEquipmentManager;
        private ICurrencyManager _mockCurrencyManager;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _mockLevelsRepository = Substitute.For<ILevelsRepository>();
            _mockLevelProgressManager = Substitute.For<ILevelProgressManager>();
            _mockTalentManager = Substitute.For<ITalentManager>();
            _mockEquipmentManager = Substitute.For<IEquipmentManager>();
            _mockCurrencyManager = Substitute.For<ICurrencyManager>();

            _mockLevelsRepository.GetTwoStarDamageMultiplier().Returns(TwoStarDamageMultiplier);
            _mockLevelProgressManager.MaxLevelNumber.Returns(3);

            Container.Bind<ILevelsRepository>().FromInstance(_mockLevelsRepository);
            Container.Bind<ILevelProgressManager>().FromInstance(_mockLevelProgressManager);
            Container.Bind<ITalentManager>().FromInstance(_mockTalentManager);
            Container.Bind<IEquipmentManager>().FromInstance(_mockEquipmentManager);
            Container.Bind<ICurrencyManager>().FromInstance(_mockCurrencyManager);

            _campaignRules = Container.Instantiate<CampaignRules>();
        }

        [Test]
        public void HubScene_IsTheCampaignScene()
        {
            Assert.AreEqual(SceneTypes.Campaign, _campaignRules.HubScene);
        }

        /// <summary>An authored level can always be thrown away and played again.</summary>
        [Test]
        public void CanReplayLevel_IsTrue()
        {
            Assert.IsTrue(_campaignRules.CanReplayLevel);
        }

        [Test]
        public void DropTableType_IsTheCampaignTable()
        {
            Assert.AreEqual(DropTableTypes.Campaign, _campaignRules.DropTableType);
        }

        [Test]
        public void ApplyProgressionBonuses_AppliesTalentsAndEquipment()
        {
            var stats = new ShipStats(new ShipBaseStats());

            _campaignRules.ApplyProgressionBonuses(stats);

            _mockTalentManager.Received(1).ApplyTalentBonuses(stats);
            _mockEquipmentManager.Received(1).ApplyEquipmentBonuses(stats);
        }

        [Test]
        public void ResolveGameEnd_BanksTheScoreAsCurrency()
        {
            var result = new GameSessionResultDTO(_session, GameplayStateResultTypes.LevelFinished, 250);

            _campaignRules.ResolveGameEnd(result);

            _mockCurrencyManager.Received(1).AddCurrency(250);
        }

        /// <summary>Campaign keeps its own hub, so it hands the next scene nothing.</summary>
        [Test]
        public void ResolveGameEnd_HandsTheHubNothing()
        {
            var result = new GameSessionResultDTO(_session, GameplayStateResultTypes.LevelFinished, 250);

            GameEndResolutionDTO resolution = _campaignRules.ResolveGameEnd(result);

            Assert.AreEqual(0, resolution.HubSceneParams.Length);
        }

        [Test]
        public void ResolveGameEnd_WithinThreeStarThreshold_RecordsThreeStars()
        {
            CreateMockLevelConfig();

            _campaignRules.ResolveGameEnd(CreateClearedResult(CreateStatsWithDamage(ThreeStarMaxDamage)));

            _mockLevelProgressManager.Received(1).RecordLevelResult(1, 3);
        }

        [Test]
        public void ResolveGameEnd_WithinTwoStarThreshold_RecordsTwoStars()
        {
            CreateMockLevelConfig();

            _campaignRules.ResolveGameEnd(CreateClearedResult(CreateStatsWithDamage(ThreeStarMaxDamage + 1)));

            _mockLevelProgressManager.Received(1).RecordLevelResult(1, 2);
        }

        [Test]
        public void ResolveGameEnd_AboveEveryThreshold_RecordsOneStar()
        {
            CreateMockLevelConfig();

            int damage = (int)(ThreeStarMaxDamage * TwoStarDamageMultiplier) + 1;
            _campaignRules.ResolveGameEnd(CreateClearedResult(CreateStatsWithDamage(damage)));

            _mockLevelProgressManager.Received(1).RecordLevelResult(1, 1);
        }

        [Test]
        public void ResolveGameEnd_WithoutStats_RecordsNothing()
        {
            CreateMockLevelConfig();

            _campaignRules.ResolveGameEnd(CreateClearedResult(null));

            _mockLevelProgressManager.DidNotReceive().RecordLevelResult(Arg.Any<int>(), Arg.Any<int>());
        }

        [Test]
        public void ResolveGameEnd_WithoutALevelConfig_RecordsNothing()
        {
            _campaignRules.ResolveGameEnd(CreateClearedResult(CreateStatsWithDamage(0)));

            _mockLevelProgressManager.DidNotReceive().RecordLevelResult(Arg.Any<int>(), Arg.Any<int>());
        }

        /// <summary>A level that was not cleared is never rated, however it ended.</summary>
        [Test]
        public void ResolveGameEnd_AfterADefeat_RecordsNoStars()
        {
            CreateMockLevelConfig();
            var result = new GameSessionResultDTO(_session, GameplayStateResultTypes.GameOver, 0,
                CreateStatsWithDamage(0));

            _campaignRules.ResolveGameEnd(result);

            _mockLevelProgressManager.DidNotReceive().RecordLevelResult(Arg.Any<int>(), Arg.Any<int>());
        }

        [Test]
        public void ResolveGameEnd_AfterADefeat_OffersReplayAndMainMenu()
        {
            var result = new GameSessionResultDTO(_session, GameplayStateResultTypes.GameOver);

            GameEndOptionTypes actions = _campaignRules.ResolveGameEnd(result).Options;

            Assert.AreEqual(GameEndOptionTypes.ReplayLevel | GameEndOptionTypes.MainMenu, actions);
        }

        [Test]
        public void ResolveGameEnd_AfterAVictory_OffersNextLevelReplayAndMainMenu()
        {
            var result = new GameSessionResultDTO(_session, GameplayStateResultTypes.LevelFinished);

            GameEndOptionTypes actions = _campaignRules.ResolveGameEnd(result).Options;

            Assert.AreEqual(
                GameEndOptionTypes.NextLevel | GameEndOptionTypes.ReplayLevel | GameEndOptionTypes.MainMenu,
                actions);
        }

        [Test]
        public void ResolveGameEnd_AfterClearingTheFinalLevel_OmitsNextLevel()
        {
            var finalSession = new GameSessionDTO(GameModeTypes.Campaign, 3, "Level 3");
            var result = new GameSessionResultDTO(finalSession, GameplayStateResultTypes.LevelFinished);

            GameEndOptionTypes actions = _campaignRules.ResolveGameEnd(result).Options;

            Assert.AreEqual(GameEndOptionTypes.ReplayLevel | GameEndOptionTypes.MainMenu, actions);
        }

        private static GameSessionResultDTO CreateClearedResult(ShipStats stats)
        {
            return new GameSessionResultDTO(_session, GameplayStateResultTypes.LevelFinished, 0, stats);
        }

        private void CreateMockLevelConfig()
        {
            var mockLevelConfig = Substitute.For<LevelConfigSO>();
            mockLevelConfig.ThreeStarMaxDamage.Returns(ThreeStarMaxDamage);

            _mockLevelsRepository.TryGetLevelConfig(_session.LevelNumber, out LevelConfigSO _)
                .Returns(call =>
                {
                    call[1] = mockLevelConfig;
                    return true;
                });
        }

        private static ShipStats CreateStatsWithDamage(int damage)
        {
            var stats = new ShipStats(new ShipBaseStats());
            stats.ApplyDamage(damage);
            return stats;
        }
    }
}
