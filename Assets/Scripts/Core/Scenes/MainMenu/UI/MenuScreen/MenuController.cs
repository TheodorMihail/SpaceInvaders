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
            _view.OnPlayGameButtonClicked += HandlePlayGameButtonClicked;
            _view.OnQuitGameButtonClicked += HandleQuitGameButtonClicked;
            _view.OnTalentsButtonClicked += HandleTalentsButtonClicked;
            _view.OnInventoryButtonClicked += HandleInventoryButtonClicked;
            _view.OnSettingsButtonClicked += HandleSettingsButtonClicked;
        }

        public override void Dispose()
        {
            _view.OnPlayGameButtonClicked -= HandlePlayGameButtonClicked;
            _view.OnQuitGameButtonClicked -= HandleQuitGameButtonClicked;
            _view.OnTalentsButtonClicked -= HandleTalentsButtonClicked;
            _view.OnInventoryButtonClicked -= HandleInventoryButtonClicked;
            _view.OnSettingsButtonClicked -= HandleSettingsButtonClicked;
            base.Dispose();
        }

        private void HandlePlayGameButtonClicked()
        {
            CloseScreenWithResult(new MenuScreen.MenuScreenResult
            {
                Result = MenuScreen.ResultTypes.PlayGame
            });
        }

        private void HandleQuitGameButtonClicked()
        {
            CloseScreenWithResult(new MenuScreen.MenuScreenResult
            {
                Result = MenuScreen.ResultTypes.QuitGame
            });
        }

        private void HandleTalentsButtonClicked()
        {
            CloseScreenWithResult(new MenuScreen.MenuScreenResult
            {
                Result = MenuScreen.ResultTypes.OpenTalentTree
            });
        }

        private void HandleInventoryButtonClicked()
        {
            CloseScreenWithResult(new MenuScreen.MenuScreenResult
            {
                Result = MenuScreen.ResultTypes.OpenInventory
            });
        }

        private void HandleSettingsButtonClicked()
        {
            CloseScreenWithResult(new MenuScreen.MenuScreenResult
            {
                Result = MenuScreen.ResultTypes.OpenSettings
            });
        }
    }
}