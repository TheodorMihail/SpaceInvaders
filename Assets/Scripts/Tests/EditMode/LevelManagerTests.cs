using NUnit.Framework;
using SpaceInvaders.Scenes.Game;
using System;
using System.Collections.Generic;
using NSubstitute;
using Zenject;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class LevelManagerTests : ZenjectUnitTestFixture
    {
        private LevelManager _levelManager;
        private IRepositoryManager _mockRepositoryManager;
        private IEnemiesManager _mockEnemiesManager;

        private void CreateMockLevelConfig(LevelTypes levelType, int waveCount)
        {
            var mockLevelConfig = Substitute.For<LevelConfigSO>();
            var waveConfigs = new List<WaveConfigDTO>();
            for (int i = 0; i < waveCount; i++)
            {
                waveConfigs.Add(new WaveConfigDTO());
            }

            mockLevelConfig.WavesConfigs.Returns(waveConfigs);
            _mockRepositoryManager.GetLevelConfig(levelType).Returns(mockLevelConfig);
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _mockRepositoryManager = Substitute.For<IRepositoryManager>();
            _mockEnemiesManager = Substitute.For<IEnemiesManager>();
            var mockUIManager = Substitute.For<IUIManager>();

            Container.Bind<IRepositoryManager>().FromInstance(_mockRepositoryManager);
            Container.Bind<IEnemiesManager>().FromInstance(_mockEnemiesManager);
            Container.Bind<IUIManager>().FromInstance(mockUIManager);

            _levelManager = Container.Instantiate<LevelManager>();
        }

        [TearDown]
        public override void Teardown()
        {
            _levelManager.Dispose();
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
        public void Initialize_SubscribesToAllEnemiesDestroyedEvent()
        {
            _mockRepositoryManager.GetLevelsCount().Returns(2);

            _levelManager.Initialize();

            _mockEnemiesManager.Received(1).OnAllEnemiesDestroyed += Arg.Any<Action>();
        }

        [Test]
        public void Dispose_UnsubscribesFromAllEnemiesDestroyedEvent()
        {
            _mockRepositoryManager.GetLevelsCount().Returns(2);

            _levelManager.Initialize();
            _levelManager.Dispose();

            _mockEnemiesManager.Received(1).OnAllEnemiesDestroyed -= Arg.Any<Action>();
        }

        [Test]
        public void OnGameStarted_IncrementsCurrentLevelNumber()
        {
            CreateMockLevelConfig(LevelTypes.Level1, 3);
            _mockRepositoryManager.GetLevelsCount().Returns(3);

            _levelManager.Initialize();
            _levelManager.OnGameStarted().Forget();

            Assert.AreEqual(1, _levelManager.CurrentLevelNumber);
        }

        [Test]
        public void OnGameStarted_SetsWaveNumbers()
        {
            CreateMockLevelConfig(LevelTypes.Level1, 3);
            _mockRepositoryManager.GetLevelsCount().Returns(3);

            _levelManager.Initialize();
            _levelManager.OnGameStarted().Forget();

            Assert.AreEqual(1, _levelManager.CurrentWaveNumber);
            Assert.AreEqual(3, _levelManager.MaxWaveNumber);
        }

        [Test]
        public void OnGameStarted_SpawnsFirstWave()
        {
            CreateMockLevelConfig(LevelTypes.Level1, 3);
            _mockRepositoryManager.GetLevelsCount().Returns(3);

            _levelManager.Initialize();
            _levelManager.OnGameStarted().Forget();

            _mockEnemiesManager.Received(1).SpawnEnemies(Arg.Any<WaveConfigDTO>());
        }

        [Test]
        public void OnAllEnemiesDestroyed_StartsNextWave()
        {
            CreateMockLevelConfig(LevelTypes.Level1, 3);
            _mockRepositoryManager.GetLevelsCount().Returns(3);

            _levelManager.Initialize();
            _levelManager.OnGameStarted().Forget();

            _mockEnemiesManager.OnAllEnemiesDestroyed += Raise.Event<Action>();

            Assert.AreEqual(2, _levelManager.CurrentWaveNumber);
            _mockEnemiesManager.Received(2).SpawnEnemies(Arg.Any<WaveConfigDTO>());
        }

        [Test]
        public void OnAllEnemiesDestroyed_LastWave_InvokesLevelCompleted()
        {
            CreateMockLevelConfig(LevelTypes.Level1, 1);
            _mockRepositoryManager.GetLevelsCount().Returns(3);

            var levelCompletedInvoked = false;
            _levelManager.OnLevelCompleted += (levelNumber) => levelCompletedInvoked = true;

            _levelManager.Initialize();
            _levelManager.OnGameStarted().Forget();

            _mockEnemiesManager.OnAllEnemiesDestroyed += Raise.Event<Action>();

            Assert.IsTrue(levelCompletedInvoked);
        }

        [Test]
        public void OnAllEnemiesDestroyed_LastWave_PassesCorrectLevelNumber()
        {
            CreateMockLevelConfig(LevelTypes.Level1, 1);
            _mockRepositoryManager.GetLevelsCount().Returns(3);

            var completedLevelNumber = -1;
            _levelManager.OnLevelCompleted += (levelNumber) => completedLevelNumber = levelNumber;

            _levelManager.Initialize();
            _levelManager.OnGameStarted().Forget();

            _mockEnemiesManager.OnAllEnemiesDestroyed += Raise.Event<Action>();

            Assert.AreEqual(1, completedLevelNumber);
        }
    }
}
