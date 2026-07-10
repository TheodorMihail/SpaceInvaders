using BaseArchitecture.Core;
using SpaceInvaders.Project;
using System.Collections.Generic;
using Zenject;
using static SpaceInvaders.Scenes.MainMenu.MainMenuStateMachine;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class MainMenuStateMachine : BaseStateMachine<MainMenuStateIds>
    {
        public enum MainMenuStateIds
        {
            Menu
        }

        protected override MainMenuStateIds DefaultStateId => MainMenuStateIds.Menu;

        [Inject] private IScenesManager _scenesManager;

        public MainMenuStateMachine(IList<IState<MainMenuStateIds>> mainMenuStates) : base(mainMenuStates)
        {
        }

        protected override void OnStateFinished((MainMenuStateIds stateId, object[] paramsList) finishedState)
        {
            try
            {
                switch (finishedState.stateId)
                {
                    case MainMenuStateIds.Menu:
                    
                        if (finishedState.paramsList.TryGetParam<MenuScreen.ResultType>(out var menuResult))
                        {
                            if(menuResult == MenuScreen.ResultType.QuitGame)
                            {
#if UNITY_EDITOR
                                UnityEditor.EditorApplication.isPlaying = false;
#else
                                Application.Quit();
#endif
                            }
                        }
                        else if (finishedState.paramsList.TryGetParam<LevelSelectionScreen.LevelSelectionScreenResult>(out var levelResult))
                        {
                            _scenesManager.LoadScene(SceneType.Game.ToString(), levelResult.LevelSelected);
                        }
                            
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