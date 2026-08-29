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

        public override void OnEnter(params object[] paramsList)
        {
            base.OnEnter();

            ShowMapScreen();
        }

        /// <summary>The map stays open for the whole run: walking it is handled inside the screen, and
        /// only leaving closes this.</summary>
        private async void ShowMapScreen()
        {
            var result = await _uiManager.ShowScreen<ExpeditionMapScreen, ExpeditionMapScreen.ExpeditionMapScreenResult>();
            FinishState(result);
        }
    }
}
