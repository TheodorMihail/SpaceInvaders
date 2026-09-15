using SpaceInvaders.Scenes.Game;
using UnityEngine;

namespace SpaceInvaders.Project
{
    [CreateAssetMenu(fileName = "CampaignRunDataConfig", menuName = "SpaceInvaders/Game Modes/Campaign Run Data Config")]
    public class CampaignRunDataConfigSO : GameModeRunDataConfigSO
    {
        public override GameModeTypes Mode => GameModeTypes.Campaign;
    }
}
