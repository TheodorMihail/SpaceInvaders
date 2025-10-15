using BaseArchitecture.Core;
using SpaceInvaders.Project;
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
        private IErrorManager _errorManager;

        public MainMenuStateMachine(IList<IState<MainMenuStateIds>> mainMenuStates,
            IScenesManager scenesManager, IErrorManager errorManager) : base(mainMenuStates)
        {
            _scenesManager = scenesManager;
            _errorManager = errorManager;
        }

        protected override void OnStateFinished((MainMenuStateIds stateId, object[] paramsList) finishedState)
        {
            try
            {
                switch (finishedState.stateId)
                {
                    case MainMenuStateIds.Menu:

                        MenuScreenResult result = (MenuScreenResult)finishedState.paramsList[0];
                        switch (result.State)
                        {
                            case ResultType.PlayGame:
                                _scenesManager.LoadScene(SceneNames.Game);
                                break;
                            case ResultType.QuitGame:
                                Application.Quit();
                                break;
                        }
                        
                    break;
                }
            }
            catch (System.Exception ex)
            {
                _errorManager.ShowErrorDialog($"Failed to transition from {finishedState.stateId}. Please restart the game.");
                _errorManager.LogError<MainMenuStateMachine>($"State transition failed from {finishedState.stateId}", ex);
            }
        }
    }
}