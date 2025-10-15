using Base.Systems;
using static SpaceInvaders.Scenes.GamePreload.GamePreloadStateMachine;

namespace SpaceInvaders.Scenes.GamePreload
{
    public class SplashState : BaseState<GamePreloadStateIds>
    {
        public override GamePreloadStateIds Id => GamePreloadStateIds.SplashState;

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