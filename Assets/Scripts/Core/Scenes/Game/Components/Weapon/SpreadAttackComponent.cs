using System.Collections.Generic;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>A fan of shots spread evenly either side of the attack's own direction.</summary>
    public class SpreadAttackComponent : BaseShipAttackComponent
    {
        [SerializeField] private float _spreadAngleDegrees = 20f;
        [SerializeField] private int _bulletCount = 3;

        public override IEnumerable<Vector3> GetShotDirections()
        {
            Vector3 baseDirection = BaseDirection.normalized;

            if (_bulletCount <= 1)
            {
                yield return baseDirection;
                yield break;
            }

            float step = (_spreadAngleDegrees * 2f) / (_bulletCount - 1);

            for (int i = 0; i < _bulletCount; i++)
            {
                float angle = -_spreadAngleDegrees + (step * i);
                yield return Quaternion.Euler(0f, angle, 0f) * baseDirection;
            }
        }
    }
}
