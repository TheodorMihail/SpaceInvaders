using NUnit.Framework;
using SpaceInvaders.Scenes.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using NSubstitute;
using Zenject;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class EnemiesServicePlayModeTests : ZenjectUnitTestFixture
    {
        private EnemiesService _enemiesService;
        private ISpawnManager _mockSpawnManager;
        private IMessageBus _messageBus;

        private List<IEnemySpaceship> CreateMockEnemies(List<EnemyTypes> enemyTypes)
        {
            var enemyList = new List<IEnemySpaceship>();
            foreach (var type in enemyTypes)
            {
                enemyList.Add(CreateMockEnemy(type));
            }

            return enemyList;
        }

        private IEnemySpaceship CreateMockEnemy(EnemyTypes enemyType)
        {
            var mockEnemy = Substitute.For<IEnemySpaceship>();
            mockEnemy.SpaceshipID.Returns(enemyType.ToString());
            mockEnemy.EnemyType.Returns(enemyType);
            return mockEnemy;
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _mockSpawnManager = Substitute.For<ISpawnManager>();
            _messageBus = new MessageBus();

            Container.Bind<ISpawnManager>().FromInstance(_mockSpawnManager);
            Container.Bind<IMessageBus>().FromInstance(_messageBus);

            _enemiesService = Container.Instantiate<EnemiesService>();
        }

        [TearDown]
        public override void Teardown()
        {
            _enemiesService.GameEnd();
            _messageBus.Dispose();
            base.Teardown();
        }

        [UnityTest]
        public IEnumerator OnGameInitialized_InitializesEnemiesList()
        {
            _enemiesService.GameInitialize();

            Assert.AreEqual(0, _enemiesService.EnemiesAlive);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SpawnEnemies_SetsCorrectEnemiesAliveCount()
        {
            var enemyList = CreateMockEnemies(new List<EnemyTypes> { EnemyTypes.Enemy1, EnemyTypes.Enemy1 });
            _mockSpawnManager.SpawnEnemies(Arg.Any<WaveConfigDTO>()).Returns(UniTask.FromResult(enemyList));

            _enemiesService.GameInitialize();
            yield return _enemiesService.SpawnEnemies(new WaveConfigDTO()).ToCoroutine();

            Assert.AreEqual(2, _enemiesService.EnemiesAlive);
        }

        [UnityTest]
        public IEnumerator SpawnEnemies_CallsStartEntryAnimationOnEachEnemy()
        {
            var enemyList = CreateMockEnemies(new List<EnemyTypes> { EnemyTypes.Enemy1, EnemyTypes.Enemy1 });
            _mockSpawnManager.SpawnEnemies(Arg.Any<WaveConfigDTO>()).Returns(UniTask.FromResult(enemyList));

            var waveConfig = new WaveConfigDTO();
            _enemiesService.GameInitialize();
            yield return _enemiesService.SpawnEnemies(waveConfig).ToCoroutine();

            enemyList[0].Received(1).PrepareEntry(Arg.Any<float>());
            enemyList[1].Received(1).PrepareEntry(Arg.Any<float>());
            enemyList[0].Received(1).StartEntryAnimation(Arg.Any<float>());
            enemyList[1].Received(1).StartEntryAnimation(Arg.Any<float>());
        }

        [UnityTest]
        public IEnumerator SpawnEnemies_SubscribesToEachEnemyDestroyedEvent()
        {
            var enemyList = CreateMockEnemies(new List<EnemyTypes> { EnemyTypes.Enemy1, EnemyTypes.Enemy1 });
            _mockSpawnManager.SpawnEnemies(Arg.Any<WaveConfigDTO>()).Returns(UniTask.FromResult(enemyList));

            _enemiesService.GameInitialize();
            yield return _enemiesService.SpawnEnemies(new WaveConfigDTO()).ToCoroutine();

            enemyList[0].Received(1).OnDestroyed += Arg.Any<Action<IEnemySpaceship>>();
            enemyList[1].Received(1).OnDestroyed += Arg.Any<Action<IEnemySpaceship>>();
        }

        [UnityTest]
        public IEnumerator OnEnemyDestroyed_PublishesEnemyDestroyedMessage()
        {
            var enemyList = CreateMockEnemies(new List<EnemyTypes> { EnemyTypes.Enemy1 });
            _mockSpawnManager.SpawnEnemies(Arg.Any<WaveConfigDTO>()).Returns(UniTask.FromResult(enemyList));

            EnemyTypes? destroyedEnemyType = null;
            _messageBus.Subscribe<EnemyDestroyedMessage>((message) => destroyedEnemyType = message.Type);

            _enemiesService.GameInitialize();
            yield return _enemiesService.SpawnEnemies(new WaveConfigDTO()).ToCoroutine();

            enemyList[0].OnDestroyed += Raise.Event<Action<IEnemySpaceship>>(enemyList[0]);
            Assert.AreEqual(EnemyTypes.Enemy1, destroyedEnemyType);
        }

        [UnityTest]
        public IEnumerator OnEnemyDestroyed_UnsubscribesFromEnemyEvent()
        {
            var enemy1 = CreateMockEnemy(EnemyTypes.Enemy1);
            var enemyList = new List<IEnemySpaceship> { enemy1 };
            _mockSpawnManager.SpawnEnemies(Arg.Any<WaveConfigDTO>()).Returns(UniTask.FromResult(enemyList));

            _enemiesService.GameInitialize();
            yield return _enemiesService.SpawnEnemies(new WaveConfigDTO()).ToCoroutine();

            enemy1.OnDestroyed += Raise.Event<Action<IEnemySpaceship>>(enemy1);
            enemy1.Received(1).OnDestroyed -= Arg.Any<Action<IEnemySpaceship>>();
        }

        [UnityTest]
        public IEnumerator OnEnemyDestroyed_LastEnemyDestroyed_PublishesAllEnemiesDestroyedMessage()
        {
            var enemyList = CreateMockEnemies(new List<EnemyTypes> { EnemyTypes.Enemy1 });
            _mockSpawnManager.SpawnEnemies(Arg.Any<WaveConfigDTO>()).Returns(UniTask.FromResult(enemyList));

            var allEnemiesDestroyedPublished = false;
            _messageBus.Subscribe<AllEnemiesDestroyedMessage>((message) => allEnemiesDestroyedPublished = true);

            _enemiesService.GameInitialize();
            yield return _enemiesService.SpawnEnemies(new WaveConfigDTO()).ToCoroutine();

            enemyList[0].OnDestroyed += Raise.Event<Action<IEnemySpaceship>>(enemyList[0]);
            Assert.IsTrue(allEnemiesDestroyedPublished);
        }

        [UnityTest]
        public IEnumerator OnEnemyDestroyed_WithMultipleEnemies_DoesNotPublishAllEnemiesDestroyedMessage()
        {
            var enemyList = CreateMockEnemies(new List<EnemyTypes> { EnemyTypes.Enemy1, EnemyTypes.Enemy1 });
            _mockSpawnManager.SpawnEnemies(Arg.Any<WaveConfigDTO>()).Returns(UniTask.FromResult(enemyList));

            var allEnemiesDestroyedPublished = false;
            _messageBus.Subscribe<AllEnemiesDestroyedMessage>((message) => allEnemiesDestroyedPublished = true);

            _enemiesService.GameInitialize();
            yield return _enemiesService.SpawnEnemies(new WaveConfigDTO()).ToCoroutine();

            enemyList[0].OnDestroyed += Raise.Event<Action<IEnemySpaceship>>(enemyList[0]);
            Assert.IsFalse(allEnemiesDestroyedPublished);
        }

        [UnityTest]
        public IEnumerator OnGameEnded_ClearsAllEnemies()
        {
            var enemy1 = CreateMockEnemy(EnemyTypes.Enemy1);
            var enemy2 = CreateMockEnemy(EnemyTypes.Enemy1);
            var enemyList = new List<IEnemySpaceship> { enemy1, enemy2 };
            _mockSpawnManager.SpawnEnemies(Arg.Any<WaveConfigDTO>()).Returns(UniTask.FromResult(enemyList));

            _enemiesService.GameInitialize();
            yield return _enemiesService.SpawnEnemies(new WaveConfigDTO()).ToCoroutine();

            _enemiesService.GameEnd();

            enemy1.Received(1).OnDestroyed -= Arg.Any<Action<IEnemySpaceship>>();
            enemy2.Received(1).OnDestroyed -= Arg.Any<Action<IEnemySpaceship>>();
            Assert.AreEqual(0, _enemiesService.EnemiesAlive);
        }

        [UnityTest]
        public IEnumerator OnGameEnded_WithWaveStillPending_NeverSpawnsIt()
        {
            var enemyList = CreateMockEnemies(new List<EnemyTypes> { EnemyTypes.Enemy1 });
            _mockSpawnManager.SpawnEnemies(Arg.Any<WaveConfigDTO>()).Returns(UniTask.FromResult(enemyList));

            _enemiesService.GameInitialize();
            _enemiesService.SpawnEnemies(new WaveConfigDTO()).Forget();

            _enemiesService.GameEnd();
            yield return null;
            yield return null;

            _mockSpawnManager.DidNotReceive().SpawnEnemies(Arg.Any<WaveConfigDTO>());
            Assert.AreEqual(0, _enemiesService.EnemiesAlive);
        }

        [UnityTest]
        public IEnumerator Dispose_ClearsAllEnemies()
        {
            var enemy1 = CreateMockEnemy(EnemyTypes.Enemy1);
            var enemy2 = CreateMockEnemy(EnemyTypes.Enemy1);
            var enemyList = new List<IEnemySpaceship> { enemy1, enemy2 };
            _mockSpawnManager.SpawnEnemies(Arg.Any<WaveConfigDTO>()).Returns(UniTask.FromResult(enemyList));

            _enemiesService.GameInitialize();
            yield return _enemiesService.SpawnEnemies(new WaveConfigDTO()).ToCoroutine();

            _enemiesService.GameEnd();

            enemy1.Received(1).OnDestroyed -= Arg.Any<Action<IEnemySpaceship>>();
            enemy2.Received(1).OnDestroyed -= Arg.Any<Action<IEnemySpaceship>>();
            Assert.AreEqual(0, _enemiesService.EnemiesAlive);
        }
    }
}
