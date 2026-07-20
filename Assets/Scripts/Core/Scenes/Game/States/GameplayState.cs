using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using Zenject;
using static SpaceInvaders.Scenes.Game.GameStateMachine;

namespace SpaceInvaders.Scenes.Game
{
    public class GameplayState : BaseState<GameStateIds>
    {
        public override GameStateIds Id => GameStateIds.Playing;

        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly IUIManager _uiManager;
        [Inject] private readonly IList<IGameStartListener> _gameStartListeners;
        [Inject] private readonly IList<IGameEndListener> _gameEndListeners;
        [Inject] private readonly IList<IGameInitializeListener> _gameInitializeListeners;
        [Inject] private readonly IList<IGameEndCondition> _gameEndConditions;

        public override void OnEnter(params object[] paramsList)
        {
            base.OnEnter();

            paramsList.TryGetParam(out int levelNumber, 1);
            StartGameplay(levelNumber);
        }

        #region StartGameplay

        private async void StartGameplay(int levelNumber)
        {
            this.Log($"Start level: {levelNumber}");

            await TriggerGameInitialize();
            await SetupUI(levelNumber);
            await TriggerGameStart(levelNumber);
        }

        private async UniTask SetupUI(int levelNumber)
        {
            _uiManager.ShowHUD<GameplayHUD, GameplayHUD.GameplayHUDParams>(new GameplayHUD.GameplayHUDParams { LevelNumber = levelNumber });
            await _uiManager.ShowScreen<GameStartScreen>();
        }

        private UniTask TriggerGameInitialize()
        {
            return UniTask.WhenAll(_gameInitializeListeners.Select(handler => handler.GameInitialize()));
        }

        private UniTask TriggerGameStart(int levelNumber)
        {
            foreach (var condition in _gameEndConditions)
            {
                condition.ConditionMet += OnGameEndConditionMet;
            }

            return UniTask.WhenAll(_gameStartListeners.Select(handler => handler.GameStart(levelNumber)));
        }

        #endregion

        #region EndGameplay

        private void OnGameEndConditionMet(GameplayStateResult result)
        {
            TriggerEndGame(result).Forget();
        }

        private async UniTask TriggerEndGame(GameplayStateResult result)
        {
            _messageBus.Publish(new GameEndedMessage());

            foreach (var condition in _gameEndConditions)
            {
                condition.ConditionMet -= OnGameEndConditionMet;
            }

            await UniTask.WhenAll(_gameEndListeners.Select(handler => handler.GameEnd()));

            FinishState(result);
        }

        #endregion
    }
}