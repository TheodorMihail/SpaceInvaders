using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;
using static SpaceInvaders.Scenes.Expedition.ExpeditionStateMachine;

namespace SpaceInvaders.Scenes.Expedition
{
    public class ExpeditionStateMachine : BaseStateMachine<ExpeditionStateTypes>
    {
        public enum ExpeditionStateTypes
        {
            Hub,
            Map
        }

        [Inject] private readonly IScenesManager _scenesManager;
        [Inject] private readonly IExpeditionRunManager _expeditionRunManager;

        protected override ExpeditionStateTypes DefaultStateId => ExpeditionStateTypes.Hub;

        public ExpeditionStateMachine(IList<IState<ExpeditionStateTypes>> expeditionStates) : base(expeditionStates)
        {
        }

        /// <summary>A run in progress opens straight onto the map, so returning from a level needs no
        /// scene parameters.</summary>
        public override void Initialize()
        {
            ExpeditionStateTypes entryState = _expeditionRunManager.HasActiveRun
                ? ExpeditionStateTypes.Map
                : ExpeditionStateTypes.Hub;

            SetState(entryState);
        }

        protected override void OnStateFinished((ExpeditionStateTypes stateId, object[] paramsList) finishedState)
        {
            try
            {
                switch (finishedState.stateId)
                {
                    case ExpeditionStateTypes.Hub:

                        // A new run has already been started by the hub, so both paths open the map.
                        if (finishedState.paramsList.TryGetParam<ExpeditionLobbyScreen.ExpeditionLobbyScreenResult>(out var lobbyResult)
                            && lobbyResult.Result != ExpeditionLobbyScreen.ResultTypes.Back)
                        {
                            SetState(ExpeditionStateTypes.Map);
                            break;
                        }

                        _scenesManager.LoadScene(SceneTypes.MainMenu.ToString());
                        break;

                    case ExpeditionStateTypes.Map:
                        SetState(ExpeditionStateTypes.Hub);
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
