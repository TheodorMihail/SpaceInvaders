using System.Collections.Generic;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using Zenject;
using static SpaceInvaders.Scenes.Game.GameStateMachine;

namespace SpaceInvaders.Scenes.Game
{
    public interface IGameStartedListener
    {
        void OnGameStarted();
    }
    
    public interface IGameEndedListener
    {
        void OnGameEnded();
    }

    public class GameplayState : BaseState<GameStateIds>
    {
        public enum GameplayStateResult
        {
            LevelFinished,
            GameOver
        }

        public override GameStateIds Id => GameStateIds.Playing;

        [Inject] private readonly IUIManager _uiManager;
        [Inject] private ILevelManager _levelManager;
        [Inject] private IPlayerManager _playerManager; 
        [Inject] private readonly IList<IGameStartedListener> _gameStartedListeners;
        [Inject] private readonly IList<IGameEndedListener> _gameEndedListeners;

        public override void OnEnter(params object[] paramsList)
        {
            base.OnEnter();
            StartGameplay();
        }

        #region StartGameplay

        private async void StartGameplay()
        {
            await SetupUI();
            TriggerStartGame();
        }

        private async UniTask SetupUI()
        {
            _uiManager.ShowHUD<GameplayHUD>();
            await _uiManager.ShowScreen<GameStartScreen>();
        }

        private void TriggerStartGame()
        {
            _levelManager.OnLevelCompleted += OnLevelCompletedCallback;
            _playerManager.OnPlayerDestroyed += OnPlayerDestroyedCallback;

            foreach (var handler in _gameStartedListeners)
            {
                handler.OnGameStarted();
            }
        }

        #endregion

        #region EndGameplay

        private void OnPlayerDestroyedCallback()
        {
            TriggerEndGame(GameplayStateResult.GameOver);
        }

        private void OnLevelCompletedCallback(int levelNumber)
        {
            TriggerEndGame(GameplayStateResult.LevelFinished);
        }

        private void TriggerEndGame(GameplayStateResult result)
        {
            _levelManager.OnLevelCompleted -= OnLevelCompletedCallback;
            _playerManager.OnPlayerDestroyed -= OnPlayerDestroyedCallback;

            foreach (var handler in _gameEndedListeners)
            {
                handler.OnGameEnded();
            }
            
            FinishState(result);
        }
        
        #endregion
    }
}