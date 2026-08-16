using BaseArchitecture.Core;
using Zenject;
using static SpaceInvaders.Scenes.Preload.PreloadStateMachine;

namespace SpaceInvaders.Scenes.Preload
{
    public class BootState : BaseState<PreloadStateTypes>
    {
        [Inject] private readonly IUIManager _uiManager;

        public override PreloadStateTypes Id => PreloadStateTypes.BootState;

        public override async void OnEnter(params object[] paramsList)
        {
            base.OnEnter();

            await _uiManager.ShowScreen<BootScreen>();
            FinishState();
        }
    }
}