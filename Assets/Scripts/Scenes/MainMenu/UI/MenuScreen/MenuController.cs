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
        }

        public override void Dispose()
        {
            _view.OnPlayGameButtonClicked -= HandlePlayGameButtonClicked;
            _view.OnQuitGameButtonClicked -= HandleQuitGameButtonClicked;
            base.Dispose();
        }

        private void HandlePlayGameButtonClicked()
        {
            CloseScreenWithResult(new MenuScreen.MenuScreenResult
            {
                Result = MenuScreen.ResultType.PlayGame
            });
        }

        private void HandleQuitGameButtonClicked()
        {
            CloseScreenWithResult(new MenuScreen.MenuScreenResult
            {
                Result = MenuScreen.ResultType.QuitGame
            });
        }
    }
}