using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Project;
using Zenject;
using static SpaceInvaders.Scenes.Expedition.ExpeditionStateMachine;

namespace SpaceInvaders.Scenes.Expedition
{
    /// <summary>
    /// The map and everything reached from it. Mutual recursion rather than a navigation stack,
    /// matching the menu and Campaign hubs.
    /// </summary>
    public class ExpeditionMapState : BaseState<ExpeditionStateTypes>
    {
        public override ExpeditionStateTypes Id => ExpeditionStateTypes.Map;

        [Inject] private readonly IUIManager _uiManager;
        [Inject] private readonly IExpeditionRunManager _expeditionRunManager;

        public override void OnEnter(params object[] paramsList)
        {
            base.OnEnter();

            ShowMapScreen();
        }

        /// <summary>Walking the map is handled inside its screen. A node that opens something of its
        /// own closes the map, which reopens behind it, so only leaving finishes this.</summary>
        private async void ShowMapScreen()
        {
            ExpeditionMapScreen.ExpeditionMapScreenResult result;

            do
            {
                await ShowNodeScreens();
                result = await _uiManager.ShowScreen<ExpeditionMapScreen, ExpeditionMapScreen.ExpeditionMapScreenResult>();
            }
            while (HasNodeScreens());

            FinishState(result);
        }

        /// <summary>Everything the node the player stands on still owes them, offered one screen at a
        /// time so closing the app between two of them keeps the rest.</summary>
        private async UniTask ShowNodeScreens()
        {
            while ((_expeditionRunManager.CurrentExpedition?.PendingTalentRewards ?? 0) > 0)
            {
                await _uiManager.ShowScreen<ExpeditionRewardScreen>();
            }

            if (_expeditionRunManager.CurrentExpedition?.HasOpenShop ?? false)
            {
                await _uiManager.ShowScreen<ExpeditionShopScreen>();
            }
        }

        private bool HasNodeScreens()
        {
            IExpeditionState expedition = _expeditionRunManager.CurrentExpedition;

            return expedition != null && (expedition.PendingTalentRewards > 0 || expedition.HasOpenShop);
        }
    }
}
