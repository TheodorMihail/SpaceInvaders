using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "RapidFirePowerupConfig", menuName = "SpaceInvaders/Powerups/Rapid Fire Powerup Config")]
    public class RapidFirePowerupConfigSO : PowerupConfigSO
    {
        [SerializeField] private float _fireRateMultiplierBonus = -0.5f;

        public float FireRateMultiplierBonus => _fireRateMultiplierBonus;
        public override PowerupTypes PowerupType => PowerupTypes.RapidFire;
    }
}
