using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "RapidFirePowerupConfig", menuName = "SpaceInvaders/Powerups/Rapid Fire Powerup Config")]
    public class RapidFirePowerupConfigSO : PowerupConfigSO
    {
        [SerializeField] private float _bonus = -0.5f;
        [SerializeField] private ShipStatValueTypes _valueType = ShipStatValueTypes.Percentage;

        public float Bonus => _bonus;
        public ShipStatValueTypes ValueType => _valueType;
        public override PowerupTypes PowerupType => PowerupTypes.RapidFire;
    }
}
