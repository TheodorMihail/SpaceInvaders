using NUnit.Framework;
using SpaceInvaders.Scenes.Game;
using SpaceInvaders.Project;
using System;
using System.Collections.Generic;
using NSubstitute;
using Zenject;
using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Game.GameplayState;
using static SpaceInvaders.Scenes.Game.GameEndState;
using static SpaceInvaders.Scenes.Game.GameStateMachine;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class GameStateMachineTests : ZenjectUnitTestFixture
    {
        private GameStateMachine _gameStateMachine;
        private IScenesManager _mockScenesManager;
        private IGameModeManager _mockGameModeManager;
        private ILevelsRepository _mockLevelsRepository;
        private IState<GameStateTypes> _mockPlayingState;
        private IState<GameStateTypes> _mockGameEndState;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _mockScenesManager = Substitute.For<IScenesManager>();

            _mockGameModeManager = Substitute.For<IGameModeManager>();
            _mockGameModeManager.HubScene.Returns(SceneTypes.MainMenu);

            _mockLevelsRepository = Substitute.For<ILevelsRepository>();
            _mockLevelsRepository.GetLevelId(Arg.Any<int>()).Returns(call => $"Level {call.Arg<int>()}");

            _mockPlayingState = Substitute.For<IState<GameStateTypes>>();
            _mockPlayingState.Id.Returns(GameStateTypes.Playing);

            _mockGameEndState = Substitute.For<IState<GameStateTypes>>();
            _mockGameEndState.Id.Returns(GameStateTypes.GameEnd);

            Container.Bind<IScenesManager>().FromInstance(_mockScenesManager);
            Container.Bind<IGameModeManager>().FromInstance(_mockGameModeManager);
            Container.Bind<ILevelsRepository>().FromInstance(_mockLevelsRepository);

            var mockStates = new List<IState<GameStateTypes>> { _mockPlayingState, _mockGameEndState };
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
        public void OnPlayingStateFinished_WithGameOver_TransitionsToGameEndState()
        {
            _gameStateMachine.Initialize();

            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.GameOver }));

            _mockGameEndState.Received(1).OnEnter(Arg.Is<object[]>(args => args.Length > 0 && ((GameSessionResultDTO)args[0]).Result == GameplayStateResultTypes.GameOver));
        }

        [Test]
        public void OnPlayingStateFinished_WithQuit_LoadsMainMenuScene()
        {
            _gameStateMachine.Initialize();

            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.Quit }));

            _mockScenesManager.Received(1).LoadScene(SceneTypes.MainMenu.ToString());
            _mockGameEndState.DidNotReceive().OnEnter(Arg.Any<object[]>());
        }

        [Test]
        public void OnPlayingStateFinished_WithRestart_LoadsGameSceneWithCurrentLevel()
        {
            _gameStateMachine.Initialize();

            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.Restart }));

            _mockScenesManager.Received(1).LoadScene(SceneTypes.Game.ToString(), new GameSessionDTO(GameModeTypes.Campaign, 1, "Level 1"));
            _mockGameEndState.DidNotReceive().OnEnter(Arg.Any<object[]>());
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
        public void OnGameEndStateFinished_WithRestart_LoadsGameSceneWithCurrentLevel()
        {
            _gameStateMachine.Initialize();
            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.GameOver }));

            _mockGameEndState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.GameEnd, new object[] { GameEndStateResultTypes.Restart }));

            _mockScenesManager.Received(1).LoadScene(SceneTypes.Game.ToString(), new GameSessionDTO(GameModeTypes.Campaign, 1, "Level 1"));
        }

        [Test]
        public void OnGameEndStateFinished_RestartAfterNextLevel_LoadsGameSceneWithAdvancedLevel()
        {
            _gameStateMachine.Initialize();
            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.LevelFinished }));
            _mockGameEndState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.GameEnd, new object[] { GameEndStateResultTypes.NextLevel }));

            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.GameOver }));
            _mockGameEndState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.GameEnd, new object[] { GameEndStateResultTypes.Restart }));

            _mockScenesManager.Received(1).LoadScene(SceneTypes.Game.ToString(), new GameSessionDTO(GameModeTypes.Campaign, 2, "Level 2"));
        }

        [Test]
        public void OnGameEndStateFinished_WithReturnToHub_LoadsTheModeHubScene()
        {
            _gameStateMachine.Initialize();
            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.GameOver }));

            _mockGameEndState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.GameEnd, new object[] { GameEndStateResultTypes.ReturnToHub }));

            _mockScenesManager.Received(1).LoadScene(SceneTypes.MainMenu.ToString());
        }

        [Test]
        public void OnGameEndStateFinished_WithNextLevel_TransitionsToPlayingState()
        {
            _gameStateMachine.Initialize();
            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.GameOver }));

            _mockGameEndState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.GameEnd, new object[] { GameEndStateResultTypes.NextLevel }));

            _mockPlayingState.Received(2).OnEnter(Arg.Any<object[]>());
        }

        [Test]
        public void OnGameEndStateFinished_WithChainedNextLevels_IncrementsLevelEachTime()
        {
            _gameStateMachine.Initialize();

            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.LevelFinished }));
            _mockGameEndState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.GameEnd, new object[] { GameEndStateResultTypes.NextLevel }));

            _mockPlayingState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.Playing, new object[] { GameplayStateResultTypes.LevelFinished }));
            _mockGameEndState.OnStateFinished += Raise.Event<Action<(GameStateTypes, object[])>>((GameStateTypes.GameEnd, new object[] { GameEndStateResultTypes.NextLevel }));

            _mockPlayingState.Received(1).OnEnter(Arg.Is<object[]>(args => args.Length > 0 && ((GameSessionDTO)args[0]).LevelNumber == 2));
            _mockPlayingState.Received(1).OnEnter(Arg.Is<object[]>(args => args.Length > 0 && ((GameSessionDTO)args[0]).LevelNumber == 3));
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
