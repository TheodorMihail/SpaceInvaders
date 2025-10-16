using BaseArchitecture.Core;
using Zenject;
using static SpaceInvaders.Scenes.Preload.PreloadStateMachine;

namespace SpaceInvaders.Scenes.Preload
{
    public class BootState : BaseState<PreloadStateIds>
    {
        public override PreloadStateIds Id => PreloadStateIds.BootState;

        [Inject] private readonly IUIManager _uiManager;

        public override async void OnEnter()
        {
            base.OnEnter();

            await _uiManager.ShowScreen<BootScreen>();
            FinishState();
        }
    }
}