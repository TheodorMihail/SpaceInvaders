using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "UnlimitedAmmoPowerupConfig", menuName = "SpaceInvaders/Powerups/Unlimited Ammo Powerup Config")]
    public class UnlimitedAmmoPowerupConfigSO : PowerupConfigSO
    {
        public override PowerupTypes PowerupType => PowerupTypes.UnlimitedAmmo;
    }
}
