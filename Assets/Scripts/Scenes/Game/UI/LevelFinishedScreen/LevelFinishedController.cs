using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Game
{
    public class LevelFinishedController : Controller<LevelFinishedScreen, LevelFinishedModel, LevelFinishedView>
    {
        public LevelFinishedController(LevelFinishedScreen screen, LevelFinishedModel model, LevelFinishedView view) : base(screen, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _view.Initialize(_model.AllLevelsComplete);
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
            CloseScreenWithResult(new LevelFinishedScreen.LevelFinishedScreenResult
            {
                Result = LevelFinishedScreen.ResultType.NextLevel
            });
        }

        private void HandleMainMenuButtonClicked()
        {
            CloseScreenWithResult(new LevelFinishedScreen.LevelFinishedScreenResult
            {
                Result = LevelFinishedScreen.ResultType.MainMenu
            });
        }
    }
}
