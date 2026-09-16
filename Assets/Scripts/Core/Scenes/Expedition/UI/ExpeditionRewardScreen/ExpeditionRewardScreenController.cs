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
            _view.Initialize(_model.GetOffers());
            _view.OnTalentClicked += HandleTalentClicked;

            if (_model.Choices.Count == 0)
            {
                HandleTalentClicked(null);
            }
        }

        public override void Dispose()
        {
            _view.OnTalentClicked -= HandleTalentClicked;
            base.Dispose();
        }

        /// <summary>Spends the pending card either way, so an offer with nothing to draw cannot leave
        /// one pending forever.</summary>
        private void HandleTalentClicked(string talentId)
        {
            _expeditionRunManager.GrantTalent(talentId);
            Close();
        }
    }
}
