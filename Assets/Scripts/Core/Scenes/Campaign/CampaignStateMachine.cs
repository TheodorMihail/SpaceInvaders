using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using Zenject;
using static SpaceInvaders.Scenes.Campaign.CampaignStateMachine;

namespace SpaceInvaders.Scenes.Campaign
{
    public class CampaignStateMachine : BaseStateMachine<CampaignStateTypes>
    {
        public enum CampaignStateTypes
        {
            Hub
        }

        [Inject] private readonly IScenesManager _scenesManager;

        protected override CampaignStateTypes DefaultStateId => CampaignStateTypes.Hub;

        public CampaignStateMachine(IList<IState<CampaignStateTypes>> campaignStates) : base(campaignStates)
        {
        }

        /// <summary>This scene is what launches a Campaign run, so it is what builds the session.</summary>
        protected override void OnStateFinished((CampaignStateTypes stateId, object[] paramsList) finishedState)
        {
            try
            {
                switch (finishedState.stateId)
                {
                    case CampaignStateTypes.Hub:

                        if (finishedState.paramsList.TryGetParam<LevelSelectionScreen.LevelSelectionScreenResult>(out var levelResult))
                        {
                            var session = new GameSessionDTO(GameModeTypes.Campaign, levelResult.LevelSelected);
                            _scenesManager.LoadScene(SceneTypes.Game.ToString(), session);
                        }
                        else
                        {
                            _scenesManager.LoadScene(SceneTypes.MainMenu.ToString());
                        }

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
