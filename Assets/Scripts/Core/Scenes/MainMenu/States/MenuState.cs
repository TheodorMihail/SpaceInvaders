using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using Zenject;
using static SpaceInvaders.Scenes.MainMenu.MainMenuStateMachine;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class MenuState : BaseState<MainMenuStateIds>
    {
        public override MainMenuStateIds Id => MainMenuStateIds.Menu;

        [Inject] private readonly IUIManager _uiManager;
        [Inject] private readonly IList<IMenuEnterListener> _menuEnterListeners;

        public override void OnEnter(params object[] paramsList)
        {
            base.OnEnter();
            TriggerMenuEnter().Forget();
            ShowMenuScreen();
        }

        private UniTask TriggerMenuEnter()
        {
            return UniTask.WhenAll(_menuEnterListeners.Select(listener => listener.MenuEnter()));
        }

        private async void ShowMenuScreen()
        {
            var result = await _uiManager.ShowScreen<MenuScreen, MenuScreen.MenuScreenResult>();

            switch (result.Result)
            {
                case MenuScreen.ResultType.QuitGame:
                    FinishState(result);
                    break;
                case MenuScreen.ResultType.OpenTalentTree:
                    await _uiManager.ShowScreen<TalentTreeScreen>();
                    ShowMenuScreen();
                    break;
                case MenuScreen.ResultType.OpenInventory:
                    await _uiManager.ShowScreen<InventoryScreen>();
                    ShowMenuScreen();
                    break;
                default:
                    ShowLevelSelectionScreen();
                    break;
            }
        }

        private async void ShowLevelSelectionScreen()
        {
            var result = await _uiManager.ShowScreen<LevelSelectionScreen, LevelSelectionScreen.LevelSelectionScreenResult>();

            if (result.Back)
            {
                ShowMenuScreen();
            }
            else
            {
                FinishState(result);
            }
        }
    }
}