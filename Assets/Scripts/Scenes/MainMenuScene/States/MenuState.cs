using Base.Systems;
using static SpaceInvaders.Scenes.MainMenu.MainMenuStateMachine;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class MenuState : BaseState<MainMenuStateIds>
    {
        public override MainMenuStateIds Id => MainMenuStateIds.Menu;

        private readonly IUIManager _uiManager;

        public MenuState(IUIManager uiManager)
        {
            _uiManager = uiManager;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            ShowMenuScreen();
        }

        private async void ShowMenuScreen()
        {
            var result = await _uiManager.ShowScreen<MenuScreen, MenuScreen.MenuScreenResult>();
            FinishState(result);
        }
    }
}