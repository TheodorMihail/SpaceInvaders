using BaseArchitecture.Core;
using SpaceInvaders.Project;
using System.Collections.Generic;
using Zenject;
using static SpaceInvaders.Scenes.MainMenu.MainMenuStateMachine;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class MainMenuStateMachine : BaseStateMachine<MainMenuStateTypes>
    {
        public enum MainMenuStateTypes
        {
            Menu
        }

        [Inject] private readonly IScenesManager _scenesManager;

        protected override MainMenuStateTypes DefaultStateId => MainMenuStateTypes.Menu;

        public MainMenuStateMachine(IList<IState<MainMenuStateTypes>> mainMenuStates) : base(mainMenuStates)
        {
        }

        protected override void OnStateFinished((MainMenuStateTypes stateId, object[] paramsList) finishedState)
        {
            try
            {
                switch (finishedState.stateId)
                {
                    case MainMenuStateTypes.Menu:

                        if (!finishedState.paramsList.TryGetParam<MenuScreen.MenuScreenResult>(out var menuResult))
                        {
                            break;
                        }

                        if (menuResult.Result == MenuScreen.ResultTypes.QuitGame)
                        {
#if UNITY_EDITOR
                            UnityEditor.EditorApplication.isPlaying = false;
#else
                            UnityEngine.Application.Quit();
#endif
                            break;
                        }

                        // Each mode owns a scene, so the menu only has to pick which one to load.
                        SceneTypes modeScene = menuResult.Result == MenuScreen.ResultTypes.PlayExpedition
                            ? SceneTypes.Expedition
                            : SceneTypes.Campaign;

                        _scenesManager.LoadScene(modeScene.ToString());
                        break;
                }
            }
            catch (System.Exception ex)
            {
                this.LogError($"State transition failed from {finishedState.stateId}", ex);
            }
        }
    }
}
