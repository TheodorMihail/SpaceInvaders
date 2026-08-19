using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>Anything a shot can damage.</summary>
    public interface IDamageableTarget
    {
        void TakeDamage(AttackSourceDTO source);
    }

    /// <summary>
    /// Marks a collider as belonging to a damageable target, so whatever hits it can reach the target
    /// without assuming where in the hierarchy the collider sits.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class BaseHitboxComponent : MonoBehaviour
    {
        /// <summary>Null once the owner is gone, so a hit resolves against nothing rather than a live
        /// reference that throws on use.</summary>
        public abstract IDamageableTarget Target { get; }

        /// <summary>Whether a shot from the given object flies the same colours and should pass through.</summary>
        public abstract bool IsSameTeamAs(GameObject other);
    }
}
