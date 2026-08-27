using System;
using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Project;
using Zenject;
using static SpaceInvaders.Scenes.Game.GameStateMachine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Runs a single gameplay session: initialization, UI setup, start, and end handling once an end
    /// condition is met.
    /// </summary>
    public class GameplayState : BaseState<GameStateTypes>
    {
        public override GameStateTypes Id => GameStateTypes.Playing;

        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly IUIManager _uiManager;
        [Inject] private readonly IList<IGameStartListener> _gameStartListeners;
        [Inject] private readonly IList<IGameEndListener> _gameEndListeners;
        [Inject] private readonly IList<IGameInitializeListener> _gameInitializeListeners;
        [Inject] private readonly IList<IGameEndCondition> _gameEndConditions;
        [Inject] private readonly IPlatformManager _platformManager;
        [Inject] private readonly IProjectRepository _projectRepository;
        [Inject] private readonly ITimeManager _timeManager;

        /// <summary>Kept so the end result can be reported against the session that produced it.</summary>
        private GameSessionDTO _session;

        public override void OnEnter(params object[] paramsList)
        {
            base.OnEnter();

            _messageBus.Subscribe<GamePausedMessage>(OnGamePaused);

            paramsList.TryGetParam(out _session, new GameSessionDTO(GameModeTypes.Campaign, 1));
            StartGameplay(_session).Forget();
        }

        public override void OnExit()
        {
            _messageBus.Unsubscribe<GamePausedMessage>(OnGamePaused);
            base.OnExit();
        }

        #region StartGameplay

        private async UniTask StartGameplay(GameSessionDTO session)
        {
            this.Log($"Start level: {session.LevelNumber}");

            await TriggerGameInitialize(session);
            await SetupUI(session);
            await TriggerGameStart(session);
        }

        private async UniTask SetupUI(GameSessionDTO session)
        {
            _uiManager.ShowHUD<GameplayHUD, GameplayHUD.GameplayHUDParams>(new GameplayHUD.GameplayHUDParams { LevelNumber = session.LevelNumber });
            _uiManager.ShowHUD<GameAnnouncerHUD>();

            if (_platformManager.IsTouchPlatform)
            {
                _uiManager.ShowHUD<MobileControlsHUD>();
            }

            await _uiManager.ShowScreen<GameStartScreen>();
        }

        private UniTask TriggerGameInitialize(GameSessionDTO session)
        {
            // Listeners run concurrently, with no ordering guarantee.
            return UniTask.WhenAll(_gameInitializeListeners.Select(handler => handler.GameInitialize(session)));
        }

        private UniTask TriggerGameStart(GameSessionDTO session)
        {
            foreach (var condition in _gameEndConditions)
            {
                condition.GameStart(session);
                condition.ConditionMet += OnGameEndConditionMet;
            }

            return UniTask.WhenAll(_gameStartListeners.Select(handler => handler.GameStart(session)));
        }

        #endregion

        #region Pause

        private void OnGamePaused(GamePausedMessage message)
        {
            ShowPauseScreen().Forget();
        }

        private async UniTask ShowPauseScreen()
        {
            var result = await _uiManager.ShowScreen<GamePausedScreen, GamePausedScreen.GamePausedScreenResult>();

            // Resumed first on every path, so the time scale is restored before anything else runs.
            _timeManager.Resume();

            switch (result.Result)
            {
                case GamePausedScreen.ResultTypes.Restart:
                    TriggerEndGame(GameplayStateResultTypes.Restart).Forget();
                    break;
                case GamePausedScreen.ResultTypes.Quit:
                    TriggerEndGame(GameplayStateResultTypes.Quit).Forget();
                    break;
            }
        }

        #endregion

        #region EndGameplay

        private void OnGameEndConditionMet(GameplayStateResultTypes result)
        {
            TriggerEndGame(result).Forget();
        }

        private async UniTask TriggerEndGame(GameplayStateResultTypes result)
        {
            _messageBus.Publish(new GameEndedMessage());

            foreach (var condition in _gameEndConditions)
            {
                condition.ConditionMet -= OnGameEndConditionMet;
                condition.GameEnd();
            }

            // The transition delay only exists to let the death or victory beat land.
            if (result == GameplayStateResultTypes.LevelFinished || result == GameplayStateResultTypes.GameOver)
            {
                float delay = _projectRepository.GetProjectDataConfig().GameEndTransitionDelay;
                await UniTask.Delay(TimeSpan.FromSeconds(delay));
            }

            var sessionResult = new GameSessionResultDTO(_session, result);
            await UniTask.WhenAll(_gameEndListeners.Select(handler => handler.GameEnd(sessionResult)));

            FinishState(result);
        }

        #endregion
    }
}