using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Project;
using Zenject;
using static SpaceInvaders.Scenes.Campaign.CampaignStateMachine;

namespace SpaceInvaders.Scenes.Campaign
{
    /// <summary>
    /// The whole Campaign flow: the hub screen and the progression screens reached from it. Mutual
    /// recursion rather than a navigation stack, matching the menu.
    /// </summary>
    public class CampaignHubState : BaseState<CampaignStateTypes>
    {
        public override CampaignStateTypes Id => CampaignStateTypes.Hub;

        [Inject] private readonly IUIManager _uiManager;
        [Inject] private readonly IList<ISceneEnterListener> _sceneEnterListeners;

        public override void OnEnter(params object[] paramsList)
        {
            base.OnEnter();

            TriggerSceneEnter().Forget();
            ShowCampaignScreen();
        }

        private UniTask TriggerSceneEnter()
        {
            return UniTask.WhenAll(_sceneEnterListeners.Select(listener => listener.SceneEnter(SceneTypes.Campaign)));
        }

        private async void ShowCampaignScreen()
        {
            var result = await _uiManager.ShowScreen<CampaignScreen, CampaignScreen.CampaignScreenResult>();

            switch (result.Result)
            {
                case CampaignScreen.ResultTypes.OpenTalentTree:
                    await _uiManager.ShowScreen<TalentTreeScreen>();
                    ShowCampaignScreen();
                    break;
                case CampaignScreen.ResultTypes.OpenInventory:
                    await _uiManager.ShowScreen<InventoryScreen>();
                    ShowCampaignScreen();
                    break;
                case CampaignScreen.ResultTypes.Back:
                    FinishState(result);
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
                ShowCampaignScreen();
            }
            else
            {
                FinishState(result);
            }
        }
    }
}
