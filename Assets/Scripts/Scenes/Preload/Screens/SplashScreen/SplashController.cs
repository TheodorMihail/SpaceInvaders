using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Preload
{
    public class SplashController : Controller<SplashScreen, SplashModel, SplashView>
    {
        public SplashController(SplashScreen splashScreen, SplashModel model, SplashView view)
            : base(splashScreen, model, view)
        {
        }

        public override async void Initialize()
        {
            base.Initialize();
            await _view.PlayLogoAnimation(_model.AnimationSimulationTimerSeconds,
                _model.AnimationStartDelayTimerSeconds, _model.AnimationEndDelayTimerSeconds);
                
            CloseScreen();
        }
    }
}