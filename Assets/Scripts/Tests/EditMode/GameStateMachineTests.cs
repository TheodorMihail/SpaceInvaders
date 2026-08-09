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
        private IState<GameStateTypes> _mockPlayingState;
        private IState<GameStateTypes> _mockGameOverState;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _mockScenesManager = Substitute.For<IScenesManager>();

            _mockPlayingState = Substitute.For<IState<GameStateTypes>>();
            _mockPlayingState.Id.Returns(GameStateTypes.Playing);

            _mockGameOverState = Substitute.For<IState<GameStateTypes>>();
            _mockGameOverState.Id.Returns(GameStateTypes.GameOver);

            Container.Bind<IScenesManager>().FromInstance(_mockScenesManager);

            var mockStates = new List<IState<GameStateTypes>> { _mockPlayingState, _mockGameOverState };
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

            _mockPlayingState.Received(1).OnStateFinished += Arg.Any<Action<(GameStateTypes, object[])>>();
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

            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.GameOver }));

            _mockGameOverState.Received(1).OnEnter(Arg.Is<object[]>(args => args.Length > 0 && (GameplayStateResultTypes)args[0] == GameplayStateResultTypes.GameOver));
        }

        [Test]
        public void OnPlayingStateFinished_WithQuit_LoadsMainMenuScene()
        {
            _gameStateMachine.Initialize();

            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.Quit }));

            _mockScenesManager.Received(1).LoadScene(SceneTypes.MainMenu.ToString());
            _mockGameOverState.DidNotReceive().OnEnter(Arg.Any<object[]>());
        }

        [Test]
        public void OnPlayingStateFinished_WithRestart_LoadsGameSceneWithCurrentLevel()
        {
            _gameStateMachine.Initialize();

            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.Restart }));

            _mockScenesManager.Received(1).LoadScene(SceneTypes.Game.ToString(), 1);
            _mockGameOverState.DidNotReceive().OnEnter(Arg.Any<object[]>());
        }

        [Test]
        public void OnPlayingStateFinished_UnsubscribesFromPreviousState()
        {
            _gameStateMachine.Initialize();

            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.GameOver }));

            _mockPlayingState.Received(1).OnStateFinished -= Arg.Any<Action<(GameStateTypes, object[])>>();
        }

        [Test]
        public void OnPlayingStateFinished_CallsOnExitOnPreviousState()
        {
            _gameStateMachine.Initialize();

            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.GameOver }));

            _mockPlayingState.Received(1).OnExit();
        }

        [Test]
        public void OnGameOverStateFinished_WithRestart_LoadsGameSceneWithCurrentLevel()
        {
            _gameStateMachine.Initialize();
            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.GameOver }));

            _mockGameOverState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.GameOver, new object[] { GameOverStateResultTypes.Restart }));

            _mockScenesManager.Received(1).LoadScene(SceneTypes.Game.ToString(), 1);
        }

        [Test]
        public void OnGameOverStateFinished_RestartAfterNextLevel_LoadsGameSceneWithAdvancedLevel()
        {
            _gameStateMachine.Initialize();
            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.LevelFinished }));
            _mockGameOverState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.GameOver, new object[] { GameOverStateResultTypes.NextLevel }));

            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.GameOver }));
            _mockGameOverState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.GameOver, new object[] { GameOverStateResultTypes.Restart }));

            _mockScenesManager.Received(1).LoadScene(SceneTypes.Game.ToString(), 2);
        }

        [Test]
        public void OnGameOverStateFinished_WithMainMenu_LoadsMainMenuScene()
        {
            _gameStateMachine.Initialize();
            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.GameOver }));

            _mockGameOverState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.GameOver, new object[] { GameOverStateResultTypes.MainMenu }));

            _mockScenesManager.Received(1).LoadScene(SceneTypes.MainMenu.ToString());
        }

        [Test]
        public void OnGameOverStateFinished_WithNextLevel_TransitionsToPlayingState()
        {
            _gameStateMachine.Initialize();
            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.GameOver }));

            _mockGameOverState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.GameOver, new object[] { GameOverStateResultTypes.NextLevel }));

            _mockPlayingState.Received(2).OnEnter(Arg.Any<object[]>());
        }

        [Test]
        public void OnGameOverStateFinished_WithChainedNextLevels_IncrementsLevelEachTime()
        {
            _gameStateMachine.Initialize();

            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.LevelFinished }));
            _mockGameOverState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.GameOver, new object[] { GameOverStateResultTypes.NextLevel }));

            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.LevelFinished }));
            _mockGameOverState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.GameOver, new object[] { GameOverStateResultTypes.NextLevel }));

            _mockPlayingState.Received(1).OnEnter(Arg.Is<object[]>(args => args.Length > 0 && (int)args[0] == 2));
            _mockPlayingState.Received(1).OnEnter(Arg.Is<object[]>(args => args.Length > 0 && (int)args[0] == 3));
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
