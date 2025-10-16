using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Game
{
    public class GameStartController : Controller<GameStartScreen, GameStartModel, GameStartView>
    {
        public GameStartController(GameStartScreen screen, GameStartModel model, GameStartView view)
            : base(screen, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _view.OnStartButtonClicked += HandleStartButtonClicked;
        }

        public override void Dispose()
        {
            base.Dispose();
            _view.OnStartButtonClicked -= HandleStartButtonClicked;
        }

        private async void HandleStartButtonClicked()
        {
            _view.OnStartButtonClicked -= HandleStartButtonClicked;
            await _view.StartCountdownAnimation(_model.CountdownSeconds);
            CloseScreen();
        }
    }
}
