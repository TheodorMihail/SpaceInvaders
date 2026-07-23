using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "DamageTalentConfig", menuName = "SpaceInvaders/Talents/Damage Talent Config")]
    public class DamageTalentConfigSO : TalentConfigSO
    {
        public override TalentTypes TalentType => TalentTypes.Damage;

        public override void ApplyBonus(ShipStats stats, float totalBonusDelta)
        {
            stats.UpdateDamageMultiplier(totalBonusDelta);
        }
    }
}
