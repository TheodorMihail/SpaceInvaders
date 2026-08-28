using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Campaign.CampaignScreen;

namespace SpaceInvaders.Scenes.Campaign
{
    public class CampaignScreen : Screen<CampaignScreenModel, CampaignScreenView, CampaignScreenController>, IScreenWithResult<CampaignScreenResult>
    {
        public enum ResultTypes
        {
            PlayCampaign,
            OpenTalentTree,
            OpenInventory,
            Back
        }

        public struct CampaignScreenResult : IScreenResult
        {
            public ResultTypes Result { get; set; }
        }

        private CampaignScreenResult _result;

        public CampaignScreenResult GetResult()
        {
            return _result;
        }

        public void SetResult(CampaignScreenResult result)
        {
            _result = result;
        }
    }
}
