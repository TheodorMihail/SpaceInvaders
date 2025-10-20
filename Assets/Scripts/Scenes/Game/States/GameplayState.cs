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

    public class GameplayState : BaseState<GameStateIds>
    {
        public override GameStateIds Id => GameStateIds.Playing;

        [Inject] private readonly IUIManager _uiManager;
        [Inject] private ILevelManager _levelManager;
        [Inject] private IPlayerManager _playerManager; 
        [Inject] private readonly IList<IGameStartedListener> _gameStartedListeners;

        public override void OnEnter()
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
            TriggerEndGame();
        }

        private void OnLevelCompletedCallback(int levelNumber)
        {
            TriggerEndGame();
        }

        private void TriggerEndGame()
        {
            _levelManager.OnLevelCompleted -= OnLevelCompletedCallback;
            _playerManager.OnPlayerDestroyed -= OnPlayerDestroyedCallback;
            FinishState();
        }
        
        #endregion
    }
}