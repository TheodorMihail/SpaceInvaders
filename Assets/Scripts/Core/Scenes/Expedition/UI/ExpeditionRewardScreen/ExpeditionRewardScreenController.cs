using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Expedition
{
    public class ExpeditionRewardScreenController : Controller<ExpeditionRewardScreen, ExpeditionRewardScreenModel, ExpeditionRewardScreenView>
    {
        [Inject] private readonly IExpeditionRunManager _expeditionRunManager;

        public ExpeditionRewardScreenController(ExpeditionRewardScreen screen, ExpeditionRewardScreenModel model,
            ExpeditionRewardScreenView view) : base(screen, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            _model.DrawChoices();
            _view.Initialize(_model.GetChoices());
            _view.OnPerkClicked += HandlePerkClicked;

            if (_model.Choices.Count == 0)
            {
                HandlePerkClicked(null);
            }
        }

        public override void Dispose()
        {
            _view.OnPerkClicked -= HandlePerkClicked;
            base.Dispose();
        }

        /// <summary>Spends the pending card either way, so an offer with nothing to draw cannot leave
        /// one pending forever.</summary>
        private void HandlePerkClicked(string perkId)
        {
            _expeditionRunManager.GrantPerk(perkId);
            Close();
        }
    }
}
