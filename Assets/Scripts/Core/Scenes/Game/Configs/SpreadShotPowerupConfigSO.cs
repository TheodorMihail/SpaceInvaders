using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "SpreadShotPowerupConfig", menuName = "SpaceInvaders/Powerups/Spread Shot Powerup Config")]
    public class SpreadShotPowerupConfigSO : PowerupConfigSO
    {
        [SerializeField] private int _extraShotCount = 2;
        [SerializeField] private float _spreadAngleDegrees = 15f;

        public int ExtraShotCount => _extraShotCount;
        public float SpreadAngleDegrees => _spreadAngleDegrees;
        public override PowerupTypes PowerupType => PowerupTypes.SpreadShot;
    }
}
