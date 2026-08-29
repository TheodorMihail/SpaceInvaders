using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using Zenject;
using static SpaceInvaders.Scenes.Expedition.ExpeditionStateMachine;

namespace SpaceInvaders.Scenes.Expedition
{
    /// <summary>Where a run is started or resumed, and the only way back to the menu.</summary>
    public class ExpeditionHubState : BaseState<ExpeditionStateTypes>
    {
        public override ExpeditionStateTypes Id => ExpeditionStateTypes.Hub;

        [Inject] private readonly IUIManager _uiManager;
        [Inject] private readonly IExpeditionRunManager _expeditionRunManager;
        [Inject] private readonly IGameModeManager _gameModeManager;
        [Inject] private readonly IList<ISceneEnterListener> _sceneEnterListeners;

        public override void OnEnter(params object[] paramsList)
        {
            base.OnEnter();

            // Before any screen reads progression, so it reads the Expedition profile rather than Campaign's.
            _gameModeManager.InitializeGameMode(GameModeTypes.Expedition);

            TriggerSceneEnter().Forget();
            ShowLobbyScreen();
        }

        private UniTask TriggerSceneEnter()
        {
            return UniTask.WhenAll(_sceneEnterListeners.Select(listener => listener.SceneEnter(SceneTypes.Expedition)));
        }

        private async void ShowLobbyScreen()
        {
            var parameters = new ExpeditionLobbyScreen.ExpeditionLobbyScreenParams
            {
                HasActiveRun = _expeditionRunManager.HasActiveRun
            };

            var result = await _uiManager.ShowScreen<ExpeditionLobbyScreen, ExpeditionLobbyScreen.ExpeditionLobbyScreenParams, 
                    ExpeditionLobbyScreen.ExpeditionLobbyScreenResult>(parameters);

            if (result.Result == ExpeditionLobbyScreen.ResultTypes.NewRun)
            {
                _expeditionRunManager.StartNewRun();
            }

            FinishState(result);
        }
    }
}
