using BaseArchitecture.Core;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
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
                    
                        if (finishedState.paramsList.TryGetParam<MenuScreen.MenuScreenResult>(out var menuResult))
                        {
                            if(menuResult.Result == MenuScreen.ResultTypes.QuitGame)
                            {
#if UNITY_EDITOR
                                UnityEditor.EditorApplication.isPlaying = false;
#else
                                UnityEngine.Application.Quit();
#endif
                            }
                        }
                        else if (finishedState.paramsList.TryGetParam<LevelSelectionScreen.LevelSelectionScreenResult>(out var levelResult))
                        {
                            var session = new GameSessionDTO(GameModeTypes.Campaign, levelResult.LevelSelected);
                            _scenesManager.LoadScene(SceneTypes.Game.ToString(), session);
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