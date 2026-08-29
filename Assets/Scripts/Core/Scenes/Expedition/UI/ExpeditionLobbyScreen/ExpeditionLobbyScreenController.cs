using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Expedition
{
    public class ExpeditionLobbyScreenController : Controller<ExpeditionLobbyScreen, ExpeditionLobbyScreenModel, ExpeditionLobbyScreenView>
    {
        public ExpeditionLobbyScreenController(ExpeditionLobbyScreen screen, ExpeditionLobbyScreenModel model, ExpeditionLobbyScreenView view)
            : base(screen, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _view.Initialize(_model.HasActiveRun);
            _view.OnContinueButtonClicked += HandleContinueButtonClicked;
            _view.OnNewRunButtonClicked += HandleNewRunButtonClicked;
            _view.OnBackButtonClicked += HandleBackButtonClicked;
        }

        public override void Dispose()
        {
            _view.OnContinueButtonClicked -= HandleContinueButtonClicked;
            _view.OnNewRunButtonClicked -= HandleNewRunButtonClicked;
            _view.OnBackButtonClicked -= HandleBackButtonClicked;
            base.Dispose();
        }

        private void HandleContinueButtonClicked()
        {
            CloseScreenWithResult(new ExpeditionLobbyScreen.ExpeditionLobbyScreenResult
            {
                Result = ExpeditionLobbyScreen.ResultTypes.Continue
            });
        }

        private void HandleNewRunButtonClicked()
        {
            CloseScreenWithResult(new ExpeditionLobbyScreen.ExpeditionLobbyScreenResult
            {
                Result = ExpeditionLobbyScreen.ResultTypes.NewRun
            });
        }

        private void HandleBackButtonClicked()
        {
            CloseScreenWithResult(new ExpeditionLobbyScreen.ExpeditionLobbyScreenResult
            {
                Result = ExpeditionLobbyScreen.ResultTypes.Back
            });
        }
    }
}
