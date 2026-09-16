using SpaceInvaders.Scenes.Game;
using UnityEngine;

namespace SpaceInvaders.Project
{
    [CreateAssetMenu(fileName = "CampaignDataConfig", menuName = "SpaceInvaders/Game Modes/Campaign Data Config")]
    public class CampaignDataConfigSO : GameModeDataConfigSO
    {
        public override GameModeTypes Mode => GameModeTypes.Campaign;
    }
}
