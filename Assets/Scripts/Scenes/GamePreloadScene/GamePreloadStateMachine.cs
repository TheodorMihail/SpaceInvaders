using Base.Systems;
using System.Collections.Generic;
using static SpaceInvaders.Scenes.GamePreload.GamePreloadStateMachine;

namespace SpaceInvaders.Scenes.GamePreload
{
    public class GamePreloadStateMachine : BaseStateMachine<GamePreloadStateIds>
    {
        public enum GamePreloadStateIds
        {
            SplashState,
            BootState
        }

        protected override GamePreloadStateIds DefaultStateId => GamePreloadStateIds.SplashState;

        private IScenesManager _scenesManager;
        private IErrorManager _errorManager;

        public GamePreloadStateMachine(IList<IState<GamePreloadStateIds>> preloadStates,
            IScenesManager scenesManager, IErrorManager errorManager) : base(preloadStates)
        {
            _scenesManager = scenesManager;
            _errorManager = errorManager;
        }

        protected override void OnStateFinished((GamePreloadStateIds stateId, object[] paramsList) finishedState)
        {
            try
            {
                switch (finishedState.stateId)
                {
                    case GamePreloadStateIds.SplashState:
                        SetState(GamePreloadStateIds.BootState);
                        break;
                    case GamePreloadStateIds.BootState:
                        _scenesManager.LoadScene(ScenesManager.SceneType.MainMenu.ToString());
                        break;
                }
            }
            catch (System.Exception ex)
            {
                _errorManager.ShowErrorDialog($"Failed to transition from {finishedState.stateId}. Please restart the game.");
                _errorManager.LogError<GamePreloadStateMachine>($"State transition failed from {finishedState.stateId}", ex);
            }
        }
    }
}