using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "DamageBoostPowerupConfig", menuName = "SpaceInvaders/Powerups/Damage Boost Powerup Config")]
    public class DamageBoostPowerupConfigSO : PowerupConfigSO
    {
        [SerializeField] private float _bonus = 0.5f;
        [SerializeField] private ShipStatValueTypes _valueType = ShipStatValueTypes.Percentage;

        public float Bonus => _bonus;
        public ShipStatValueTypes ValueType => _valueType;
        public override PowerupTypes PowerupType => PowerupTypes.DamageBoost;
    }
}
