using System;
using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using Zenject;
using static SpaceInvaders.Scenes.Game.GameStateMachine;

namespace SpaceInvaders.Scenes.Game
{
    public interface IGameInitializeListener
    {
        UniTask OnGameInitialized();
    }

    public interface IGameStartedListener
    {
        UniTask OnGameStarted();
    }

    public interface IGameEndedListener
    {
        UniTask OnGameEnded();
    }

    public class GameplayState : BaseState<GameStateIds>
    {
        public enum GameplayStateResult
        {
            LevelFinished,
            GameOver
        }

        public override GameStateIds Id => GameStateIds.Playing;

        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly IUIManager _uiManager;
        [Inject] private ILevelManager _levelManager;
        [Inject] private IPlayerManager _playerManager; 
        [Inject] private readonly IList<IGameStartedListener> _gameStartedListeners;
        [Inject] private readonly IList<IGameEndedListener> _gameEndedListeners;
        [Inject] private readonly IList<IGameInitializeListener> _gameInitializeListeners;

        public override void OnEnter(params object[] paramsList)
        {
            base.OnEnter();

            if (!paramsList.TryGetParam(out int levelNumber))
                levelNumber = 1;

            StartGameplay(levelNumber);
        }

        #region StartGameplay

        private async void StartGameplay(int levelNumber)
        {
            this.Log($"Start level: {levelNumber}");

            await TriggerInitializeGame();
            await SetupUI(levelNumber);
            await TriggerStartGame();
        }

        private UniTask TriggerInitializeGame()
        {
            return UniTask.WhenAll(_gameInitializeListeners.Select(handler => handler.OnGameInitialized()));
        }

        private async UniTask SetupUI(int levelNumber)
        {
            _uiManager.ShowHUD<GameplayHUD, GameplayHUD.GameplayHUDParams>(new GameplayHUD.GameplayHUDParams { LevelNumber = levelNumber });
            await _uiManager.ShowScreen<GameStartScreen>();
        }

        private UniTask TriggerStartGame()
        {
            _levelManager.OnLevelCompleted += OnLevelCompletedCallback;
            _playerManager.OnPlayerDestroyed += OnPlayerDestroyedCallback;

            return UniTask.WhenAll(_gameStartedListeners.Select(handler => handler.OnGameStarted()));
        }

        #endregion

        #region EndGameplay

        private void OnPlayerDestroyedCallback()
        {
            TriggerEndGame(GameplayStateResult.GameOver).Forget();
        }

        private void OnLevelCompletedCallback(int levelNumber)
        {
            TriggerEndGame(GameplayStateResult.LevelFinished).Forget();
        }

        private async UniTask TriggerEndGame(GameplayStateResult result)
        {
            _messageBus.Publish(new GameEndedMessage());
            _levelManager.OnLevelCompleted -= OnLevelCompletedCallback;
            _playerManager.OnPlayerDestroyed -= OnPlayerDestroyedCallback;

            await UniTask.WhenAll(_gameEndedListeners.Select(handler => handler.OnGameEnded()));

            FinishState(result);
        }
        
        #endregion
    }
}