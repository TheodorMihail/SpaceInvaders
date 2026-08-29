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
            _view.Initialize(_model.Nodes);
            _view.OnNodeClicked += HandleNodeClicked;
            _view.OnBackButtonClicked += HandleBackButtonClicked;
        }

        public override void Dispose()
        {
            _view.OnNodeClicked -= HandleNodeClicked;
            _view.OnBackButtonClicked -= HandleBackButtonClicked;
            base.Dispose();
        }

        /// <summary>Moving is an interaction inside the map, not a way out of it, so the screen stays
        /// open and only the node states change.</summary>
        private void HandleNodeClicked(int nodeId)
        {
            _expeditionRunManager.EnterNode(nodeId);
            _view.Refresh(_model.Nodes);
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
