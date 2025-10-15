using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Preload.PreloadStateMachine;

namespace SpaceInvaders.Scenes.Preload
{
    public class SplashState : BaseState<PreloadStateIds>
    {
        public override PreloadStateIds Id => PreloadStateIds.SplashState;

        private readonly IUIManager _uiManager;

        public SplashState(IUIManager uiManager)
        {
            _uiManager = uiManager;
        }

        public override async void OnEnter()
        {
            base.OnEnter();

            await _uiManager.ShowScreen<SplashScreen>();
            FinishState();
        }
    }
}