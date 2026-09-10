using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
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
        [Inject] private readonly IGameModeManager _gameModeManager;

        protected override ExpeditionStateTypes DefaultStateId => ExpeditionStateTypes.Hub;

        public ExpeditionStateMachine(IList<IState<ExpeditionStateTypes>> expeditionStates) : base(expeditionStates)
        {
        }

        /// <summary>The run phase picks the entry state, so returning from a level needs no params.</summary>
        public override void Initialize()
        {
            // Before any screen reads progression, so it reads this mode's profile.
            _gameModeManager.InitializeGameMode(GameModeTypes.Expedition);

            // Mid-level on entry means the level was left, and a map missing a level cannot be finished.
            if (_expeditionRunManager.RunPhase == ExpeditionRunPhaseTypes.InLevel
                || _expeditionRunManager.HasMissingLevels)
            {
                _expeditionRunManager.AbandonRun();
            }

            SetState(GetEntryState());
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

                        // Picking a level node is the one way out of the map that is not going back.
                        if (_expeditionRunManager.TryGetCurrentNodeSession(out GameSessionDTO session)
                            && _expeditionRunManager.RunPhase == ExpeditionRunPhaseTypes.InLevel)
                        {
                            _scenesManager.LoadScene(SceneTypes.Game.ToString(), session);
                            break;
                        }

                        SetState(ExpeditionStateTypes.Hub);
                        break;
                }
            }
            catch (System.Exception ex)
            {
                this.LogError($"State transition failed from {finishedState.stateId}", ex);
            }
        }

        /// <summary>A finished run opens on the hub, which is where its summary is shown.</summary>
        private ExpeditionStateTypes GetEntryState()
        {
            switch (_expeditionRunManager.RunPhase)
            {
                case ExpeditionRunPhaseTypes.OnMap:
                case ExpeditionRunPhaseTypes.NodeCleared:
                    return ExpeditionStateTypes.Map;
                default:
                    return ExpeditionStateTypes.Hub;
            }
        }
    }
}
