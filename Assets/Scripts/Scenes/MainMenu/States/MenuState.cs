using BaseArchitecture.Core;
using Zenject;
using static SpaceInvaders.Scenes.MainMenu.MainMenuStateMachine;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class MenuState : BaseState<MainMenuStateIds>
    {
        public override MainMenuStateIds Id => MainMenuStateIds.Menu;

        [Inject] private readonly IUIManager _uiManager;

        public override async void OnEnter(params object[] paramsList)
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