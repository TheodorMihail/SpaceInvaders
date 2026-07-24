using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "FireRateTalentConfig", menuName = "SpaceInvaders/Talents/Fire Rate Talent Config")]
    public class FireRateTalentConfigSO : TalentConfigSO
    {
        public override TalentTypes TalentType => TalentTypes.FireRate;

        public override void ApplyBonus(ShipStats stats, float totalBonusDelta)
        {
            stats.FireRateStat.AddBonus(-totalBonusDelta);
        }
    }
}
