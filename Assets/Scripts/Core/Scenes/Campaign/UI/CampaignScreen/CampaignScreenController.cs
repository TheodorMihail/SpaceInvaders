using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Campaign
{
    public class CampaignScreenController : Controller<CampaignScreen, CampaignScreenModel, CampaignScreenView>
    {
        public CampaignScreenController(CampaignScreen screen, CampaignScreenModel model, CampaignScreenView view) : base(screen, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _view.OnPlayButtonClicked += HandlePlayButtonClicked;
            _view.OnTalentsButtonClicked += HandleTalentsButtonClicked;
            _view.OnInventoryButtonClicked += HandleInventoryButtonClicked;
            _view.OnBackButtonClicked += HandleBackButtonClicked;
        }

        public override void Dispose()
        {
            _view.OnPlayButtonClicked -= HandlePlayButtonClicked;
            _view.OnTalentsButtonClicked -= HandleTalentsButtonClicked;
            _view.OnInventoryButtonClicked -= HandleInventoryButtonClicked;
            _view.OnBackButtonClicked -= HandleBackButtonClicked;
            base.Dispose();
        }

        private void HandlePlayButtonClicked()
        {
            CloseScreenWithResult(new CampaignScreen.CampaignScreenResult
            {
                Result = CampaignScreen.ResultTypes.PlayCampaign
            });
        }

        private void HandleTalentsButtonClicked()
        {
            CloseScreenWithResult(new CampaignScreen.CampaignScreenResult
            {
                Result = CampaignScreen.ResultTypes.OpenTalentTree
            });
        }

        private void HandleInventoryButtonClicked()
        {
            CloseScreenWithResult(new CampaignScreen.CampaignScreenResult
            {
                Result = CampaignScreen.ResultTypes.OpenInventory
            });
        }

        private void HandleBackButtonClicked()
        {
            CloseScreenWithResult(new CampaignScreen.CampaignScreenResult
            {
                Result = CampaignScreen.ResultTypes.Back
            });
        }
    }
}
