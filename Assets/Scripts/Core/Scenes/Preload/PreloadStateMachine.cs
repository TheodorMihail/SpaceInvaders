using BaseArchitecture.Core;
using SpaceInvaders.Project;
using System.Collections.Generic;
using Zenject;
using static SpaceInvaders.Scenes.Preload.PreloadStateMachine;

namespace SpaceInvaders.Scenes.Preload
{
    public class PreloadStateMachine : BaseStateMachine<PreloadStateTypes>
    {
        public enum PreloadStateTypes
        {
            SplashState,
            BootState
        }

        [Inject] private readonly IScenesManager _scenesManager;

        protected override PreloadStateTypes DefaultStateId => PreloadStateTypes.SplashState;

        public PreloadStateMachine(IList<IState<PreloadStateTypes>> preloadStates) : base(preloadStates)
        {
        }

        protected override void OnStateFinished((PreloadStateTypes stateId, object[] paramsList) finishedState)
        {
            try
            {
                switch (finishedState.stateId)
                {
                    case PreloadStateTypes.SplashState:
                        SetState(PreloadStateTypes.BootState);
                        break;
                    case PreloadStateTypes.BootState:
                        _scenesManager.LoadScene(SceneTypes.MainMenu.ToString());
                        break;
                }
            }
            catch (System.Exception ex)
            {
                this.LogError($"State transition failed from {finishedState.stateId}", ex);
            }
        }
    }
}