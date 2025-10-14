using Base.Systems;

namespace SpaceInvaders.Scenes.GamePreload
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
            await _view.PlayLogoAnimation(_model.AnimationSimulationTimerSeconds);
            CloseScreen();
        }
    }
}