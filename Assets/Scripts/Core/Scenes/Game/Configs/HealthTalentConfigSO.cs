using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "HealthTalentConfig", menuName = "SpaceInvaders/Talents/Health Talent Config")]
    public class HealthTalentConfigSO : TalentConfigSO
    {
        public override TalentTypes TalentType => TalentTypes.Health;

        public override void ApplyBonus(ShipStats stats, float totalBonusDelta)
        {
            stats.UpdateHealthMultiplier(totalBonusDelta);
        }
    }
}
