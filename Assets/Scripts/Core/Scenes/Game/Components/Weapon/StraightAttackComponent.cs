using System.Collections.Generic;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>A single shot straight down the attack's own direction.</summary>
    public class StraightAttackComponent : BaseShipAttackComponent
    {
        public override IEnumerable<Vector3> GetShotDirections()
        {
            yield return BaseDirection.normalized;
        }
    }
}
