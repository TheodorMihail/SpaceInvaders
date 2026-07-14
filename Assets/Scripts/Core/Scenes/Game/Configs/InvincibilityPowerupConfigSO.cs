using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "InvincibilityPowerupConfig", menuName = "SpaceInvaders/Powerups/Invincibility Powerup Config")]
    public class InvincibilityPowerupConfigSO : PowerupConfigSO
    {
        public override PowerupTypes PowerupType => PowerupTypes.Invincibility;
    }
}
