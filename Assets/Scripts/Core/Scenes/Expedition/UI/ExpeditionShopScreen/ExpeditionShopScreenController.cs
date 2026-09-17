using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Expedition
{
    public class ExpeditionShopScreenController : Controller<ExpeditionShopScreen, ExpeditionShopScreenModel, ExpeditionShopScreenView>
    {
        [Inject] private readonly IExpeditionRunManager _expeditionRunManager;

        public ExpeditionShopScreenController(ExpeditionShopScreen screen, ExpeditionShopScreenModel model,
            ExpeditionShopScreenView view) : base(screen, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            _view.OnBuyClicked += HandleBuyClicked;
            _view.OnRepairClicked += HandleRepairClicked;
            _view.OnDoneClicked += HandleDoneClicked;
            _view.Setup();
        }

        public override void Dispose()
        {
            _view.OnBuyClicked -= HandleBuyClicked;
            _view.OnRepairClicked -= HandleRepairClicked;
            _view.OnDoneClicked -= HandleDoneClicked;
            base.Dispose();
        }

        /// <summary>Nothing is shown for a refused purchase: the button is only live on an offer the
        /// player can afford.</summary>
        private void HandleBuyClicked(string instanceId)
        {
            if (_expeditionRunManager.TryBuyShopOffer(instanceId))
            {
                _view.ApplyPurchase(instanceId);
            }
        }

        private void HandleRepairClicked()
        {
            if (_expeditionRunManager.TryRepair())
            {
                _view.ApplyRepair();
            }
        }

        /// <summary>Leaving drops the shelf, so the node cannot be shopped a second time.</summary>
        private void HandleDoneClicked()
        {
            _expeditionRunManager.CloseShop();
            Close();
        }
    }
}
