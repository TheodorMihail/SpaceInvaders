using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Expedition
{
    public class ExpeditionMapScreenController : Controller<ExpeditionMapScreen, ExpeditionMapScreenModel, ExpeditionMapScreenView>
    {
        [Inject] private readonly IExpeditionRunManager _expeditionRunManager;

        public ExpeditionMapScreenController(ExpeditionMapScreen screen, ExpeditionMapScreenModel model, ExpeditionMapScreenView view)
            : base(screen, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _view.Initialize(_model.Expedition.Nodes, _model.Expedition.CurrentNodeId);
            _view.OnNodeClicked += HandleNodeClicked;
            _view.OnBackButtonClicked += HandleBackButtonClicked;
        }

        public override void Dispose()
        {
            _view.OnNodeClicked -= HandleNodeClicked;
            _view.OnBackButtonClicked -= HandleBackButtonClicked;
            base.Dispose();
        }

        /// <summary>Moving keeps the map open. A node with a screen of its own closes it instead, and
        /// it reopens once that screen is done with.</summary>
        private void HandleNodeClicked(int nodeId)
        {
            _expeditionRunManager.EnterNode(nodeId);

            if (_model.Expedition.IsLevelInProgress || _model.Expedition.HasOpenShop)
            {
                CloseScreenWithResult(new ExpeditionMapScreen.ExpeditionMapScreenResult
                {
                    Back = false
                });
                return;
            }

            _view.Refresh(_model.Expedition.Nodes, _model.Expedition.CurrentNodeId);
        }

        private void HandleBackButtonClicked()
        {
            CloseScreenWithResult(new ExpeditionMapScreen.ExpeditionMapScreenResult
            {
                Back = true
            });
        }
    }
}
