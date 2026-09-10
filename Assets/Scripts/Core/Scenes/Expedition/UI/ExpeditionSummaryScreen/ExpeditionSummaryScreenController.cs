using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Expedition
{
    public class ExpeditionSummaryScreenController : Controller<ExpeditionSummaryScreen, ExpeditionSummaryScreenModel, ExpeditionSummaryScreenView>
    {
        public ExpeditionSummaryScreenController(ExpeditionSummaryScreen screen, ExpeditionSummaryScreenModel model,
            ExpeditionSummaryScreenView view) : base(screen, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _view.Initialize(_model.Result, _model.DepthReached);
            _view.OnContinueButtonClicked += HandleContinueButtonClicked;
        }

        public override void Dispose()
        {
            _view.OnContinueButtonClicked -= HandleContinueButtonClicked;
            base.Dispose();
        }

        private void HandleContinueButtonClicked()
        {
            Close();
        }
    }
}
