using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "MoveSpeedTalentConfig", menuName = "SpaceInvaders/Talents/Move Speed Talent Config")]
    public class MoveSpeedTalentConfigSO : TalentConfigSO
    {
        public override TalentTypes TalentType => TalentTypes.MoveSpeed;

        public override void ApplyBonus(ShipStats stats, float totalBonusDelta)
        {
            stats.UpdateMoveSpeedMultiplier(totalBonusDelta);
        }
    }
}
