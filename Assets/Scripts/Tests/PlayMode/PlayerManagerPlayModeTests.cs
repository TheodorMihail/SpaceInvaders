using NUnit.Framework;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using System;
using System.Collections;
using NSubstitute;
using Zenject;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class PlayerManagerPlayModeTests : ZenjectUnitTestFixture
    {
        private PlayerManager _playerManager;
        private ISpawnService _mockSpawnService;
        private IPlayerSpaceship _mockPlayer;
        private IMessageBus _messageBus;
        private ITalentManager _mockTalentManager;
        private IEquipmentManager _mockEquipmentManager;

        private IEnumerator InitializeAndSpawnPlayer()
        {
            yield return _playerManager.GameInitialize().ToCoroutine();
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _mockSpawnService = Substitute.For<ISpawnService>();
            _mockPlayer = Substitute.For<IPlayerSpaceship>();
            _messageBus = new MessageBus();
            _mockTalentManager = Substitute.For<ITalentManager>();
            _mockEquipmentManager = Substitute.For<IEquipmentManager>();

            _mockSpawnService.SpawnPlayer().Returns(UniTask.FromResult(_mockPlayer));

            Container.Bind<ISpawnService>().FromInstance(_mockSpawnService);
            Container.Bind<IMessageBus>().FromInstance(_messageBus);
            Container.Bind<ITalentManager>().FromInstance(_mockTalentManager);
            Container.Bind<IEquipmentManager>().FromInstance(_mockEquipmentManager);

            _playerManager = Container.Instantiate<PlayerManager>();
        }

        [TearDown]
        public override void Teardown()
        {
            _playerManager.Dispose();
            _messageBus.Dispose();
            base.Teardown();
        }

        [UnityTest]
        public IEnumerator OnGameInitialized_SpawnsPlayer()
        {
            yield return InitializeAndSpawnPlayer();
            _mockSpawnService.Received(1).SpawnPlayer();
        }

        [UnityTest]
        public IEnumerator OnGameInitialized_SubscribesToPlayerDestroyedEvent()
        {
            yield return InitializeAndSpawnPlayer();

            _mockPlayer.Received(1).OnDestroyed += Arg.Any<Action<IPlayerSpaceship>>();
        }

        [UnityTest]
        public IEnumerator OnGameStarted_EnablesPlayerControls()
        {
            yield return InitializeAndSpawnPlayer();
            _playerManager.GameStart(1).Forget();

            _mockPlayer.Received(1).EnableControls();
        }

        [UnityTest]
        public IEnumerator OnPlayerDestroyed_PublishesPlayerDestroyedMessage()
        {
            var playerDestroyedPublished = false;
            _messageBus.Subscribe<PlayerDestroyedMessage>((message) => playerDestroyedPublished = true);

            yield return InitializeAndSpawnPlayer();

            _mockPlayer.OnDestroyed += Raise.Event<Action<IPlayerSpaceship>>(_mockPlayer);
            Assert.IsTrue(playerDestroyedPublished);
        }

        [UnityTest]
        public IEnumerator OnPlayerDestroyed_UnsubscribesFromPlayerEvent()
        {
            yield return InitializeAndSpawnPlayer();

            _mockPlayer.OnDestroyed += Raise.Event<Action<IPlayerSpaceship>>(_mockPlayer);
            _mockPlayer.Received(1).OnDestroyed -= Arg.Any<Action<IPlayerSpaceship>>();
        }

        [UnityTest]
        public IEnumerator OnGameEnded_DesawnsPlayer()
        {
            yield return InitializeAndSpawnPlayer();

            _playerManager.GameEnd().Forget();

            _mockPlayer.Received(1).OnDestroyed -= Arg.Any<Action<IPlayerSpaceship>>();
        }

        [UnityTest]
        public IEnumerator Dispose_DespawnsPlayer()
        {
            yield return InitializeAndSpawnPlayer();

            _playerManager.Dispose();

            _mockPlayer.Received(1).OnDestroyed -= Arg.Any<Action<IPlayerSpaceship>>();
        }
    }
}
