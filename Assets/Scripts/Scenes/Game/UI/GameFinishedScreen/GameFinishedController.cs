using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Game
{
    public class GameFinishedController : Controller<GameFinishedScreen, GameFinishedModel, GameFinishedView>
    {
        public GameFinishedController(GameFinishedScreen screen, GameFinishedModel model, GameFinishedView view) : base(screen, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _view.OnNextLevelButtonClicked += HandleNextLevelButtonClicked;
            _view.OnMainMenuButtonClicked += HandleMainMenuButtonClicked;
        }

        public override void Dispose()
        {
            _view.OnNextLevelButtonClicked -= HandleNextLevelButtonClicked;
            _view.OnMainMenuButtonClicked -= HandleMainMenuButtonClicked;
            base.Dispose();
        }

        private void HandleNextLevelButtonClicked()
        {
            CloseScreenWithResult(new GameFinishedScreen.GameFinishedScreenResult
            {
                Result = GameFinishedScreen.ResultType.NextLevel
            });
        }

        private void HandleMainMenuButtonClicked()
        {
            CloseScreenWithResult(new GameFinishedScreen.GameFinishedScreenResult
            {
                Result = GameFinishedScreen.ResultType.MainMenu
            });
        }
    }
}
