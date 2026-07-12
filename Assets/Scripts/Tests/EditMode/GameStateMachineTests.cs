using NUnit.Framework;
using SpaceInvaders.Scenes.Game;
using SpaceInvaders.Project;
using System;
using System.Collections.Generic;
using NSubstitute;
using Zenject;
using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Game.GameplayState;
using static SpaceInvaders.Scenes.Game.GameOverState;
using static SpaceInvaders.Scenes.Game.GameStateMachine;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class GameStateMachineTests : ZenjectUnitTestFixture
    {
        private GameStateMachine _gameStateMachine;
        private IScenesManager _mockScenesManager;
        private IState<GameStateIds> _mockPlayingState;
        private IState<GameStateIds> _mockGameOverState;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _mockScenesManager = Substitute.For<IScenesManager>();

            _mockPlayingState = Substitute.For<IState<GameStateIds>>();
            _mockPlayingState.Id.Returns(GameStateIds.Playing);

            _mockGameOverState = Substitute.For<IState<GameStateIds>>();
            _mockGameOverState.Id.Returns(GameStateIds.GameOver);

            Container.Bind<IScenesManager>().FromInstance(_mockScenesManager);

            var mockStates = new List<IState<GameStateIds>> { _mockPlayingState, _mockGameOverState };
            _gameStateMachine = new GameStateMachine(mockStates);
            Container.Inject(_gameStateMachine);
        }

        [TearDown]
        public override void Teardown()
        {
            _gameStateMachine.Dispose();
            base.Teardown();
        }

        [Test]
        public void Initialize_StartsWithPlayingState()
        {
            _gameStateMachine.Initialize();

            _mockPlayingState.Received(1).OnEnter(Arg.Any<object[]>());
        }

        [Test]
        public void Initialize_SubscribesToPlayingStateFinishedEvent()
        {
            _gameStateMachine.Initialize();

            _mockPlayingState.Received(1).OnStateFinished += Arg.Any<Action<(GameStateIds, object[])>>();
        }

        [Test]
        public void Tick_CallsActiveStateOnUpdate()
        {
            _gameStateMachine.Initialize();

            _gameStateMachine.Tick();

            _mockPlayingState.Received(1).OnUpdate();
        }

        [Test]
        public void OnPlayingStateFinished_WithGameOver_TransitionsToGameOverState()
        {
            _gameStateMachine.Initialize();

            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateIds, object[])>>((GameStateIds.Playing, new object[] { GameplayStateResult.GameOver }));

            _mockGameOverState.Received(1).OnEnter(Arg.Is<object[]>(args => args.Length > 0 && (GameplayStateResult)args[0] == GameplayStateResult.GameOver));
        }

        [Test]
        public void OnPlayingStateFinished_UnsubscribesFromPreviousState()
        {
            _gameStateMachine.Initialize();

            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateIds, object[])>>((GameStateIds.Playing, new object[] { GameplayStateResult.GameOver }));

            _mockPlayingState.Received(1).OnStateFinished -= Arg.Any<Action<(GameStateIds, object[])>>();
        }

        [Test]
        public void OnPlayingStateFinished_CallsOnExitOnPreviousState()
        {
            _gameStateMachine.Initialize();

            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateIds, object[])>>((GameStateIds.Playing, new object[] { GameplayStateResult.GameOver }));

            _mockPlayingState.Received(1).OnExit();
        }

        [Test]
        public void OnGameOverStateFinished_WithRestart_LoadsGameScene()
        {
            _gameStateMachine.Initialize();
            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateIds, object[])>>((GameStateIds.Playing, new object[] { GameplayStateResult.GameOver }));

            _mockGameOverState.OnStateFinished += Raise.Event<Action<(GameStateIds, object[])>>((GameStateIds.GameOver, new object[] { GameOverStateResult.Restart }));

            _mockScenesManager.Received(1).ReloadCurrentScene();
        }

        [Test]
        public void OnGameOverStateFinished_WithMainMenu_LoadsMainMenuScene()
        {
            _gameStateMachine.Initialize();
            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateIds, object[])>>((GameStateIds.Playing, new object[] { GameplayStateResult.GameOver }));

            _mockGameOverState.OnStateFinished += Raise.Event<Action<(GameStateIds, object[])>>((GameStateIds.GameOver, new object[] { GameOverStateResult.MainMenu }));

            _mockScenesManager.Received(1).LoadScene(SceneType.MainMenu.ToString());
        }

        [Test]
        public void OnGameOverStateFinished_WithNextLevel_TransitionsToPlayingState()
        {
            _gameStateMachine.Initialize();
            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateIds, object[])>>((GameStateIds.Playing, new object[] { GameplayStateResult.GameOver }));

            _mockGameOverState.OnStateFinished += Raise.Event<Action<(GameStateIds, object[])>>((GameStateIds.GameOver, new object[] { GameOverStateResult.NextLevel }));

            _mockPlayingState.Received(2).OnEnter(Arg.Any<object[]>());
        }

        [Test]
        public void Dispose_CallsOnExitOnActiveState()
        {
            _gameStateMachine.Initialize();

            _gameStateMachine.Dispose();

            _mockPlayingState.Received(1).OnExit();
        }
    }
}
