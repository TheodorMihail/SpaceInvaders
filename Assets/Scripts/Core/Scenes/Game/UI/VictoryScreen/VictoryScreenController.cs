using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Game
{
    public class VictoryScreenController : Controller<VictoryScreen, VictoryScreenModel, VictoryScreenView>
    {
        public VictoryScreenController(VictoryScreen screen, VictoryScreenModel model, VictoryScreenView view) : base(screen, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _view.Initialize(_model.Options, _model.StarsEarned, _model.TotalScore, _model.GetCollectedItems());
            _view.OnNextLevelButtonClicked += HandleNextLevelButtonClicked;
            _view.OnRetryButtonClicked += HandleRetryButtonClicked;
            _view.OnMainMenuButtonClicked += HandleMainMenuButtonClicked;
        }

        public override void Dispose()
        {
            _view.OnNextLevelButtonClicked -= HandleNextLevelButtonClicked;
            _view.OnRetryButtonClicked -= HandleRetryButtonClicked;
            _view.OnMainMenuButtonClicked -= HandleMainMenuButtonClicked;
            base.Dispose();
        }

        private void HandleNextLevelButtonClicked()
        {
            CloseScreenWithResult(new VictoryScreen.VictoryScreenResult
            {
                Result = VictoryScreen.ResultTypes.NextLevel
            });
        }

        private void HandleRetryButtonClicked()
        {
            CloseScreenWithResult(new VictoryScreen.VictoryScreenResult
            {
                Result = VictoryScreen.ResultTypes.Retry
            });
        }

        private void HandleMainMenuButtonClicked()
        {
            CloseScreenWithResult(new VictoryScreen.VictoryScreenResult
            {
                Result = VictoryScreen.ResultTypes.MainMenu
            });
        }
    }
}
