using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Project;
using Zenject;
using static SpaceInvaders.Scenes.MainMenu.MainMenuStateMachine;

namespace SpaceInvaders.Scenes.MainMenu
{
    /// <summary>The hub: it picks a game mode or opens settings, and owns no progression of its own.</summary>
    public class MenuState : BaseState<MainMenuStateTypes>
    {
        public override MainMenuStateTypes Id => MainMenuStateTypes.Menu;

        [Inject] private readonly IUIManager _uiManager;
        [Inject] private readonly IList<ISceneEnterListener> _sceneEnterListeners;

        public override void OnEnter(params object[] paramsList)
        {
            base.OnEnter();

            TriggerSceneEnter().Forget();
            ShowMenuScreen();
        }

        private UniTask TriggerSceneEnter()
        {
            return UniTask.WhenAll(_sceneEnterListeners.Select(listener => listener.SceneEnter(SceneTypes.MainMenu)));
        }

        private async void ShowMenuScreen()
        {
            var result = await _uiManager.ShowScreen<MenuScreen, MenuScreen.MenuScreenResult>();

            switch (result.Result)
            {
                case MenuScreen.ResultTypes.OpenSettings:
                    await _uiManager.ShowScreen<SettingsScreen>();
                    ShowMenuScreen();
                    break;
                default:
                    FinishState(result);
                    break;
            }
        }
    }
}
