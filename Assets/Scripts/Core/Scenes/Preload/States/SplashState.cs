using BaseArchitecture.Core;
using Zenject;
using static SpaceInvaders.Scenes.Preload.PreloadStateMachine;

namespace SpaceInvaders.Scenes.Preload
{
    public class SplashState : BaseState<PreloadStateTypes>
    {
        public override PreloadStateTypes Id => PreloadStateTypes.SplashState;

        [Inject] private readonly IUIManager _uiManager;

        public override async void OnEnter(params object[] paramsList)
        {
            base.OnEnter();

            await _uiManager.ShowScreen<SplashScreen>();
            FinishState();
        }
    }
}