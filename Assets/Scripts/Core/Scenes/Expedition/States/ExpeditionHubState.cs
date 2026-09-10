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
        [Inject] private readonly IList<ISceneEnterListener> _sceneEnterListeners;

        public override void OnEnter(params object[] paramsList)
        {
            base.OnEnter();

            TriggerSceneEnter().Forget();
            ShowScreens(paramsList);
        }

        private UniTask TriggerSceneEnter()
        {
            return UniTask.WhenAll(_sceneEnterListeners.Select(listener => listener.SceneEnter(SceneTypes.Expedition)));
        }

        /// <summary>A run that just ended is reported before the lobby.</summary>
        private async void ShowScreens(object[] paramsList)
        {
            if (paramsList.TryGetParam(out ExpeditionRunResultDTO runResult))
            {
                await _uiManager.ShowScreen<ExpeditionSummaryScreen, ExpeditionSummaryScreen.ExpeditionSummaryScreenParams>(
                    new ExpeditionSummaryScreen.ExpeditionSummaryScreenParams { RunResult = runResult });
            }

            ShowLobbyScreen();
        }

        private async void ShowLobbyScreen()
        {
            var parameters = new ExpeditionLobbyScreen.ExpeditionLobbyScreenParams
            {
                HasActiveExpedition = _expeditionRunManager.CurrentExpedition != null
            };

            var result = await _uiManager.ShowScreen<ExpeditionLobbyScreen, ExpeditionLobbyScreen.ExpeditionLobbyScreenParams, 
                    ExpeditionLobbyScreen.ExpeditionLobbyScreenResult>(parameters);

            if (result.Result == ExpeditionLobbyScreen.ResultTypes.NewRun)
            {
                _expeditionRunManager.StartNewExpedition();
            }

            FinishState(result);
        }
    }
}
