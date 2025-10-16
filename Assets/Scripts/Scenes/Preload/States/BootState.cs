using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Preload.PreloadStateMachine;

namespace SpaceInvaders.Scenes.Preload
{
    public class BootState : BaseState<PreloadStateIds>
    {
        public override PreloadStateIds Id => PreloadStateIds.BootState;

        private readonly IUIManager _uiManager;

        public BootState(IUIManager uiManager)
        {
            _uiManager = uiManager;
        }

        public override async void OnEnter()
        {
            base.OnEnter();

            await _uiManager.ShowScreen<BootScreen>();
            FinishState();
        }
    }
}