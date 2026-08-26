using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>A single shot led straight at wherever the player currently is.</summary>
    public class AimedAttackComponent : BaseShipAttackComponent
    {
        [Inject] private readonly IPlayerManager _playerManager;

        private const float MinAimDistanceSqr = 0.0001f;

        public override IEnumerable<Vector3> GetShotDirections()
        {
            // A shot can be selected on the frame the player dies, and the wave keeps firing after.
            if (!_playerManager.TryGetPlayerWorldPosition(out Vector3 playerWorldPosition))
            {
                yield return BaseDirection.normalized;
                yield break;
            }

            // Both sides in world space: an attack hangs off the weapon, so its local position is
            // measured against that rather than against whatever the player's is.
            Vector3 toPlayer = playerWorldPosition - transform.position;
            toPlayer.y = 0f;

            // No direction to aim when overlapping the player, so fall back to the default.
            yield return toPlayer.sqrMagnitude > MinAimDistanceSqr
                ? toPlayer.normalized
                : BaseDirection.normalized;
        }
    }
}
