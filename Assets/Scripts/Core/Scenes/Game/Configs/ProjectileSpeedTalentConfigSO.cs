using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "ProjectileSpeedTalentConfig", menuName = "SpaceInvaders/Talents/Projectile Speed Talent Config")]
    public class ProjectileSpeedTalentConfigSO : TalentConfigSO
    {
        public override TalentTypes TalentType => TalentTypes.ProjectileSpeed;

        public override void ApplyBonus(ShipStats stats, float totalBonusDelta)
        {
            stats.ProjectileSpeedStat.AddBonus(totalBonusDelta);
        }
    }
}
