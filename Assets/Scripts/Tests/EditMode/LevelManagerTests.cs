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
    public class LevelManagerTests : ZenjectUnitTestFixture
    {
        private LevelManager _levelManager;
        private IRepositoryManager _mockRepositoryManager;
        private IEnemiesManager _mockEnemiesManager;
        private IPlayerManager _mockPlayerManager;
        private IProgressManager _mockProgressManager;
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
            _mockRepositoryManager.GetLevelConfig(level).Returns(mockLevelConfig);
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _mockRepositoryManager = Substitute.For<IRepositoryManager>();
            _mockEnemiesManager = Substitute.For<IEnemiesManager>();
            var mockUIManager = Substitute.For<IUIManager>();
            _mockPlayerManager = Substitute.For<IPlayerManager>();
            _mockProgressManager = Substitute.For<IProgressManager>();
            _messageBus = new MessageBus();

            _mockPlayerManager.PlayerStats.Returns(new ShipStats(new ShipBaseStats()));

            Container.Bind<IRepositoryManager>().FromInstance(_mockRepositoryManager);
            Container.Bind<IEnemiesManager>().FromInstance(_mockEnemiesManager);
            Container.Bind<IUIManager>().FromInstance(mockUIManager);
            Container.Bind<IPlayerManager>().FromInstance(_mockPlayerManager);
            Container.Bind<IProgressManager>().FromInstance(_mockProgressManager);
            Container.Bind<IMessageBus>().FromInstance(_messageBus);

            _levelManager = Container.Instantiate<LevelManager>();
        }

        [TearDown]
        public override void Teardown()
        {
            _levelManager.Dispose();
            _messageBus.Dispose();
            base.Teardown();
        }

        [Test]
        public void Initialize_SetsCorrectLevelNumbers()
        {
            _mockRepositoryManager.GetLevelsCount().Returns(5);

            _levelManager.Initialize();

            Assert.AreEqual(0, _levelManager.CurrentLevelNumber);
            Assert.AreEqual(5, _levelManager.MaxLevelNumber);
        }

        [Test]
        public void Dispose_StopsReactingToAllEnemiesDestroyedMessage()
        {
            CreateMockLevelConfig(1, 3);
            _mockRepositoryManager.GetLevelsCount().Returns(3);

            _levelManager.Initialize();
            _levelManager.GameStart(1).Forget();
            _levelManager.Dispose();

            _messageBus.Publish(new AllEnemiesDestroyedMessage());

            Assert.AreEqual(1, _levelManager.CurrentWaveNumber);
            _mockEnemiesManager.Received(1).SpawnEnemies(Arg.Any<WaveConfigDTO>());
        }

        [Test]
        public void OnGameStarted_IncrementsCurrentLevelNumber()
        {
            CreateMockLevelConfig(1, 3);
            _mockRepositoryManager.GetLevelsCount().Returns(3);

            _levelManager.Initialize();
            _levelManager.GameStart(1).Forget();

            Assert.AreEqual(1, _levelManager.CurrentLevelNumber);
        }

        [Test]
        public void OnGameStarted_SetsWaveNumbers()
        {
            CreateMockLevelConfig(1, 3);
            _mockRepositoryManager.GetLevelsCount().Returns(3);

            _levelManager.Initialize();
            _levelManager.GameStart(1).Forget();

            Assert.AreEqual(1, _levelManager.CurrentWaveNumber);
            Assert.AreEqual(3, _levelManager.MaxWaveNumber);
        }

        [Test]
        public void OnGameStarted_SpawnsFirstWave()
        {
            CreateMockLevelConfig(1, 3);
            _mockRepositoryManager.GetLevelsCount().Returns(3);

            _levelManager.Initialize();
            _levelManager.GameStart(1).Forget();

            _mockEnemiesManager.Received(1).SpawnEnemies(Arg.Any<WaveConfigDTO>());
        }

        [Test]
        public void OnAllEnemiesDestroyed_StartsNextWave()
        {
            CreateMockLevelConfig(1, 3);
            _mockRepositoryManager.GetLevelsCount().Returns(3);

            _levelManager.Initialize();
            _levelManager.GameStart(1).Forget();

            _messageBus.Publish(new AllEnemiesDestroyedMessage());

            Assert.AreEqual(2, _levelManager.CurrentWaveNumber);
            _mockEnemiesManager.Received(2).SpawnEnemies(Arg.Any<WaveConfigDTO>());
        }

        [Test]
        public void OnAllEnemiesDestroyed_LastWave_PublishesLevelCompletedMessage()
        {
            CreateMockLevelConfig(1, 1);
            _mockRepositoryManager.GetLevelsCount().Returns(3);

            var levelCompletedInvoked = false;
            _messageBus.Subscribe<LevelCompletedMessage>((message) => levelCompletedInvoked = true);

            _levelManager.Initialize();
            _levelManager.GameStart(1).Forget();

            _messageBus.Publish(new AllEnemiesDestroyedMessage());

            Assert.IsTrue(levelCompletedInvoked);
        }

        [Test]
        public void OnAllEnemiesDestroyed_LastWave_PassesCorrectLevelNumber()
        {
            CreateMockLevelConfig(1, 1);
            _mockRepositoryManager.GetLevelsCount().Returns(3);

            var completedLevelNumber = -1;
            _messageBus.Subscribe<LevelCompletedMessage>((message) => completedLevelNumber = message.LevelNumber);

            _levelManager.Initialize();
            _levelManager.GameStart(1).Forget();

            _messageBus.Publish(new AllEnemiesDestroyedMessage());

            Assert.AreEqual(1, completedLevelNumber);
        }

        [Test]
        public void OnAllEnemiesDestroyed_LastWave_RecordsLevelResult()
        {
            CreateMockLevelConfig(1, 1);
            _mockRepositoryManager.GetLevelsCount().Returns(3);

            _levelManager.Initialize();
            _levelManager.GameStart(1).Forget();

            _messageBus.Publish(new AllEnemiesDestroyedMessage());

            _mockProgressManager.Received(1).RecordLevelResult(1, Arg.Any<int>());
        }
    }
}
