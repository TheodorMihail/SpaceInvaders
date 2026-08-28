using NUnit.Framework;
using SpaceInvaders.Scenes.Game;
using System.Collections.Generic;
using NSubstitute;
using Zenject;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Project;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class LevelSessionManagerTests : ZenjectUnitTestFixture
    {
        private static readonly GameSessionDTO _session = new(GameModeTypes.Campaign, 1);

        private LevelSessionManager _levelSessionManager;
        private ILevelsRepository _mockLevelsRepository;
        private IGameModeManager _mockGameModeManager;
        private IShipsRepository _mockShipsRepository;
        private IEnemiesService _mockEnemiesService;
        private IPlayerManager _mockPlayerManager;
        private IHazardsService _mockHazardsService;
        private IScoreService _mockScoreService;
        private IImpactFeedbackService _mockImpactFeedbackService;
        private IMessageBus _messageBus;

        private void CreateMockLevelConfig(int level, int waveCount)
        {
            var mockLevelConfig = Substitute.For<LevelConfigSO>();
            var waveConfigs = new List<WaveConfigDTO>();
            for (int i = 0; i < waveCount; i++)
            {
                waveConfigs.Add(new WaveConfigDTO());
            }

            mockLevelConfig.WavesConfigs.Returns(waveConfigs);
            mockLevelConfig.LevelName.Returns($"Level {level}");
            _mockLevelsRepository.TryGetLevelConfig(level, out LevelConfigSO _)
                .Returns(call =>
                {
                    call[1] = mockLevelConfig;
                    return true;
                });
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _mockLevelsRepository = Substitute.For<ILevelsRepository>();
            _mockGameModeManager = Substitute.For<IGameModeManager>();
            _mockShipsRepository = Substitute.For<IShipsRepository>();
            _mockEnemiesService = Substitute.For<IEnemiesService>();
            _mockPlayerManager = Substitute.For<IPlayerManager>();
            _mockHazardsService = Substitute.For<IHazardsService>();
            _mockScoreService = Substitute.For<IScoreService>();
            _mockImpactFeedbackService = Substitute.For<IImpactFeedbackService>();
            _messageBus = new MessageBus();

            _mockPlayerManager.PlayerStats.Returns(new ShipStats(new ShipBaseStats()));

            Container.Bind<ILevelsRepository>().FromInstance(_mockLevelsRepository);
            Container.Bind<IGameModeManager>().FromInstance(_mockGameModeManager);
            Container.Bind<IShipsRepository>().FromInstance(_mockShipsRepository);
            Container.Bind<IEnemiesService>().FromInstance(_mockEnemiesService);
            Container.Bind<IPlayerManager>().FromInstance(_mockPlayerManager);
            Container.Bind<IHazardsService>().FromInstance(_mockHazardsService);
            Container.Bind<IScoreService>().FromInstance(_mockScoreService);
            Container.Bind<IImpactFeedbackService>().FromInstance(_mockImpactFeedbackService);
            Container.Bind<IMessageBus>().FromInstance(_messageBus);

            _levelSessionManager = Container.Instantiate<LevelSessionManager>();
        }

        [TearDown]
        public override void Teardown()
        {
            _levelSessionManager.Dispose();
            _messageBus.Dispose();
            base.Teardown();
        }

        [Test]
        public void Dispose_StopsReactingToAllEnemiesDestroyedMessage()
        {
            CreateMockLevelConfig(1, 3);

            _levelSessionManager.Initialize();
            _levelSessionManager.GameStart(_session).Forget();
            _levelSessionManager.Dispose();

            _messageBus.Publish(new AllEnemiesDestroyedMessage());

            _mockEnemiesService.Received(1).SpawnEnemies(Arg.Any<WaveConfigDTO>());
        }

        [Test]
        public void OnGameStarted_TakesCurrentLevelNumberFromTheSession()
        {
            CreateMockLevelConfig(1, 3);

            _levelSessionManager.Initialize();
            _levelSessionManager.GameStart(_session).Forget();

            Assert.AreEqual(1, _levelSessionManager.CurrentLevelNumber);
        }

        [Test]
        public void OnGameStarted_SpawnsFirstWave()
        {
            CreateMockLevelConfig(1, 3);

            _levelSessionManager.Initialize();
            _levelSessionManager.GameStart(_session).Forget();

            _mockEnemiesService.Received(1).SpawnEnemies(Arg.Any<WaveConfigDTO>());
        }

        [Test]
        public void OnGameStarted_PublishesLevelStartedMessage()
        {
            CreateMockLevelConfig(1, 3);

            var startedLevelNumber = -1;
            string startedLevelName = null;
            _messageBus.Subscribe<LevelStartedMessage>((message) =>
            {
                startedLevelNumber = message.LevelNumber;
                startedLevelName = message.LevelName;
            });

            _levelSessionManager.Initialize();
            _levelSessionManager.GameStart(_session).Forget();

            Assert.AreEqual(1, startedLevelNumber);
            Assert.AreEqual("Level 1", startedLevelName);
        }

        [Test]
        public void OnGameStarted_PublishesWaveStartedMessage()
        {
            CreateMockLevelConfig(1, 3);

            var startedWaveNumber = -1;
            _messageBus.Subscribe<WaveStartedMessage>((message) => startedWaveNumber = message.WaveNumber);

            _levelSessionManager.Initialize();
            _levelSessionManager.GameStart(_session).Forget();

            Assert.AreEqual(1, startedWaveNumber);
        }

        [Test]
        public void OnAllEnemiesDestroyed_StartsNextWave()
        {
            CreateMockLevelConfig(1, 3);

            _levelSessionManager.Initialize();
            _levelSessionManager.GameStart(_session).Forget();

            _messageBus.Publish(new AllEnemiesDestroyedMessage());

            _mockEnemiesService.Received(2).SpawnEnemies(Arg.Any<WaveConfigDTO>());
        }

        [Test]
        public void OnAllEnemiesDestroyed_LastWave_PublishesLevelCompletedMessage()
        {
            CreateMockLevelConfig(1, 1);

            var levelCompletedInvoked = false;
            _messageBus.Subscribe<LevelCompletedMessage>((message) => levelCompletedInvoked = true);

            _levelSessionManager.Initialize();
            _levelSessionManager.GameStart(_session).Forget();

            _messageBus.Publish(new AllEnemiesDestroyedMessage());

            Assert.IsTrue(levelCompletedInvoked);
        }

        [Test]
        public void OnAllEnemiesDestroyed_LastWave_PassesCorrectLevelNumber()
        {
            CreateMockLevelConfig(1, 1);

            var completedLevelNumber = -1;
            _messageBus.Subscribe<LevelCompletedMessage>((message) => completedLevelNumber = message.LevelNumber);

            _levelSessionManager.Initialize();
            _levelSessionManager.GameStart(_session).Forget();

            _messageBus.Publish(new AllEnemiesDestroyedMessage());

            Assert.AreEqual(1, completedLevelNumber);
        }

        [Test]
        public void OnAllEnemiesDestroyed_LastWave_SavesLevelResultThroughTheMode()
        {
            CreateMockLevelConfig(1, 1);

            _levelSessionManager.Initialize();
            _levelSessionManager.GameStart(_session).Forget();

            _messageBus.Publish(new AllEnemiesDestroyedMessage());

            _mockGameModeManager.Received(1).SaveLevelResult(_session, Arg.Any<ShipStats>());
        }
    }
}
