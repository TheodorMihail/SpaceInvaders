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
            this.CloseScreenWithResult(_screen, new MenuScreen.MenuScreenResult
            {
                State = MenuScreen.ResultType.PlayGame
            });
        }

        private void HandleQuitGameButtonClicked()
        {
            this.CloseScreenWithResult(_screen, new MenuScreen.MenuScreenResult
            {
                State = MenuScreen.ResultType.QuitGame
            });
        }
    }
}