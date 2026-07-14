using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "DamageBoostPowerupConfig", menuName = "SpaceInvaders/Powerups/Damage Boost Powerup Config")]
    public class DamageBoostPowerupConfigSO : PowerupConfigSO
    {
        [SerializeField] private float _damageMultiplierBonus = 0.5f;

        public float DamageMultiplierBonus => _damageMultiplierBonus;
        public override PowerupTypes PowerupType => PowerupTypes.DamageBoost;
    }
}
