using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "HealPowerupConfig", menuName = "SpaceInvaders/Powerups/Heal Powerup Config")]
    public class HealPowerupConfigSO : PowerupConfigSO
    {
        [SerializeField] private int _healAmount = 25;

        public int HealAmount => _healAmount;
        public override PowerupTypes PowerupType => PowerupTypes.Heal;
    }
}
