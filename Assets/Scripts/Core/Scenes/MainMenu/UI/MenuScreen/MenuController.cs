using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class MenuController : Controller<MenuScreen, MenuModel, MenuView>
    {
        public MenuController(MenuScreen screen, MenuModel model, MenuView view) : base(screen, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _view.OnCampaignButtonClicked += HandleCampaignButtonClicked;
            _view.OnSettingsButtonClicked += HandleSettingsButtonClicked;
            _view.OnQuitGameButtonClicked += HandleQuitGameButtonClicked;
        }

        public override void Dispose()
        {
            _view.OnCampaignButtonClicked -= HandleCampaignButtonClicked;
            _view.OnSettingsButtonClicked -= HandleSettingsButtonClicked;
            _view.OnQuitGameButtonClicked -= HandleQuitGameButtonClicked;
            base.Dispose();
        }

        private void HandleCampaignButtonClicked()
        {
            CloseScreenWithResult(new MenuScreen.MenuScreenResult
            {
                Result = MenuScreen.ResultTypes.PlayCampaign
            });
        }

        private void HandleSettingsButtonClicked()
        {
            CloseScreenWithResult(new MenuScreen.MenuScreenResult
            {
                Result = MenuScreen.ResultTypes.OpenSettings
            });
        }

        private void HandleQuitGameButtonClicked()
        {
            CloseScreenWithResult(new MenuScreen.MenuScreenResult
            {
                Result = MenuScreen.ResultTypes.QuitGame
            });
        }
    }
}
