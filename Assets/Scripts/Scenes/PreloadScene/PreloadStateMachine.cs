using BaseArchitecture.Core;
using SpaceInvaders.Project;
using System.Collections.Generic;
using static SpaceInvaders.Scenes.Preload.PreloadStateMachine;

namespace SpaceInvaders.Scenes.Preload
{
    public class PreloadStateMachine : BaseStateMachine<PreloadStateIds>
    {
        public enum PreloadStateIds
        {
            SplashState,
            BootState
        }

        protected override PreloadStateIds DefaultStateId => PreloadStateIds.SplashState;

        private IScenesManager _scenesManager;
        private IErrorManager _errorManager;

        public PreloadStateMachine(IList<IState<PreloadStateIds>> preloadStates,
            IScenesManager scenesManager, IErrorManager errorManager) : base(preloadStates)
        {
            _scenesManager = scenesManager;
            _errorManager = errorManager;
        }

        protected override void OnStateFinished((PreloadStateIds stateId, object[] paramsList) finishedState)
        {
            try
            {
                switch (finishedState.stateId)
                {
                    case PreloadStateIds.SplashState:
                        SetState(PreloadStateIds.BootState);
                        break;
                    case PreloadStateIds.BootState:
                        _scenesManager.LoadScene(SceneNames.MainMenu);
                        break;
                }
            }
            catch (System.Exception ex)
            {
                _errorManager.ShowErrorDialog($"Failed to transition from {finishedState.stateId}. Please restart the game.");
                _errorManager.LogError<PreloadStateMachine>($"State transition failed from {finishedState.stateId}", ex);
            }
        }
    }
}