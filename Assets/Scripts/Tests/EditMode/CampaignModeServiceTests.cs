using NSubstitute;
using NUnit.Framework;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class CampaignModeServiceTests : ZenjectUnitTestFixture
    {
        private const int ThreeStarMaxDamage = 10;
        private const float TwoStarDamageMultiplier = 3f;

        private static readonly GameSessionDTO _session = new(GameModeTypes.Campaign, 1);

        private CampaignModeService _campaignModeService;
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

            _campaignModeService = Container.Instantiate<CampaignModeService>();
        }

        [Test]
        public void HubScene_IsTheMainMenu()
        {
            Assert.AreEqual(SceneTypes.MainMenu, _campaignModeService.HubScene);
        }

        [Test]
        public void ApplyProgressionBonuses_AppliesTalentsAndEquipment()
        {
            var stats = new ShipStats(new ShipBaseStats());

            _campaignModeService.ApplyProgressionBonuses(stats);

            _mockTalentManager.Received(1).ApplyTalentBonuses(stats);
            _mockEquipmentManager.Received(1).ApplyEquipmentBonuses(stats);
        }

        [Test]
        public void SaveRunScore_BanksTheScoreAsCurrency()
        {
            var result = new GameSessionResultDTO(_session, GameplayStateResultTypes.LevelFinished);

            _campaignModeService.SaveRunScore(result, 250);

            _mockCurrencyManager.Received(1).AddCurrency(250);
        }

        [Test]
        public void SaveLevelResult_WithinThreeStarThreshold_RecordsThreeStars()
        {
            CreateMockLevelConfig();

            _campaignModeService.SaveLevelResult(_session, CreateStatsWithDamage(ThreeStarMaxDamage));

            _mockLevelProgressManager.Received(1).RecordLevelResult(1, 3);
        }

        [Test]
        public void SaveLevelResult_WithinTwoStarThreshold_RecordsTwoStars()
        {
            CreateMockLevelConfig();

            _campaignModeService.SaveLevelResult(_session, CreateStatsWithDamage(ThreeStarMaxDamage + 1));

            _mockLevelProgressManager.Received(1).RecordLevelResult(1, 2);
        }

        [Test]
        public void SaveLevelResult_AboveEveryThreshold_RecordsOneStar()
        {
            CreateMockLevelConfig();

            int damage = (int)(ThreeStarMaxDamage * TwoStarDamageMultiplier) + 1;
            _campaignModeService.SaveLevelResult(_session, CreateStatsWithDamage(damage));

            _mockLevelProgressManager.Received(1).RecordLevelResult(1, 1);
        }

        [Test]
        public void SaveLevelResult_WithoutStats_RecordsNothing()
        {
            CreateMockLevelConfig();

            _campaignModeService.SaveLevelResult(_session, null);

            _mockLevelProgressManager.DidNotReceive().RecordLevelResult(Arg.Any<int>(), Arg.Any<int>());
        }

        [Test]
        public void SaveLevelResult_WithoutALevelConfig_RecordsNothing()
        {
            _campaignModeService.SaveLevelResult(_session, CreateStatsWithDamage(0));

            _mockLevelProgressManager.DidNotReceive().RecordLevelResult(Arg.Any<int>(), Arg.Any<int>());
        }

        [Test]
        public void GetGameOverOptions_AfterADefeat_OffersRestartAndMainMenu()
        {
            var result = new GameSessionResultDTO(_session, GameplayStateResultTypes.GameOver);

            GameOverOptionTypes actions = _campaignModeService.GetGameOverOptions(result);

            Assert.AreEqual(GameOverOptionTypes.Restart | GameOverOptionTypes.MainMenu, actions);
        }

        [Test]
        public void GetGameOverOptions_AfterAVictory_OffersNextLevelRetryAndMainMenu()
        {
            var result = new GameSessionResultDTO(_session, GameplayStateResultTypes.LevelFinished);

            GameOverOptionTypes actions = _campaignModeService.GetGameOverOptions(result);

            Assert.AreEqual(
                GameOverOptionTypes.NextLevel | GameOverOptionTypes.Retry | GameOverOptionTypes.MainMenu,
                actions);
        }

        [Test]
        public void GetGameOverOptions_AfterClearingTheFinalLevel_OmitsNextLevel()
        {
            var finalSession = new GameSessionDTO(GameModeTypes.Campaign, 3);
            var result = new GameSessionResultDTO(finalSession, GameplayStateResultTypes.LevelFinished);

            GameOverOptionTypes actions = _campaignModeService.GetGameOverOptions(result);

            Assert.AreEqual(GameOverOptionTypes.Retry | GameOverOptionTypes.MainMenu, actions);
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
