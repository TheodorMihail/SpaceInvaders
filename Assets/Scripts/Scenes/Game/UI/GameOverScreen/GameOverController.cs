using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Game
{
    public class GameOverController : Controller<GameOverScreen, GameOverModel, GameOverView>
    {
        public GameOverController(GameOverScreen screen, GameOverModel model, GameOverView view) : base(screen, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _view.OnRestartButtonClicked += HandleRestartButtonClicked;
            _view.OnMainMenuButtonClicked += HandleMainMenuButtonClicked;
        }

        public override void Dispose()
        {
            _view.OnRestartButtonClicked -= HandleRestartButtonClicked;
            _view.OnMainMenuButtonClicked -= HandleMainMenuButtonClicked;
            base.Dispose();
        }

        private void HandleRestartButtonClicked()
        {
            CloseScreenWithResult(new GameOverScreen.GameOverScreenResult
            {
                Result = GameOverScreen.ResultType.Restart
            });
        }

        private void HandleMainMenuButtonClicked()
        {
            CloseScreenWithResult(new GameOverScreen.GameOverScreenResult
            {
                Result = GameOverScreen.ResultType.MainMenu
            });
        }
    }
}
