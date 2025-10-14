using Base.Systems;
using System.Collections.Generic;
using UnityEngine;
using static SpaceInvaders.Scenes.MainMenu.MainMenuStateMachine;
using static SpaceInvaders.Scenes.MainMenu.MenuScreen;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class MainMenuStateMachine : BaseStateMachine<MainMenuStateIds>
    {
        public enum MainMenuStateIds
        {
            Menu
        }

        protected override MainMenuStateIds DefaultStateId => MainMenuStateIds.Menu;

        private IScenesManager _scenesManager;

        public MainMenuStateMachine(IList<IState<MainMenuStateIds>> mainMenuStates,
            IScenesManager scenesManager) : base(mainMenuStates)
        {
            _scenesManager = scenesManager;
        }

        protected override void OnStateFinished((MainMenuStateIds stateId, object[] paramsList) finishedState)
        {
            switch (finishedState.stateId)
            {
                case MainMenuStateIds.Menu:

                    MenuScreenResult result = (MenuScreenResult)finishedState.paramsList[0];
                    switch (result.State)
                    {
                        case ResultType.PlayGame:
                            _scenesManager.LoadScene(ScenesManager.SceneType.Game.ToString());
                            break;
                        case ResultType.QuitGame:
                            Application.Quit();
                            break;
                    }
                    break;
            }
        }
    }
}