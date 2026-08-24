using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;
using static SpaceInvaders.Scenes.Preload.PreloadStateMachine;

namespace SpaceInvaders.Scenes.Preload
{
    public class SplashState : BaseState<PreloadStateTypes>
    {
        public override PreloadStateTypes Id => PreloadStateTypes.SplashState;

        [Inject] private readonly IPlatformManager _platformManager;
        [Inject] private readonly IUIManager _uiManager;

        public override async void OnEnter(params object[] paramsList)
        {
            base.OnEnter();

            _platformManager.ApplyFrameRateCap();

            await _uiManager.ShowScreen<SplashScreen>();
            FinishState();
        }
    }
}