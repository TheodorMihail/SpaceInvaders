using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Marks a collider as belonging to a ship, so anything that hits it can reach the ship without
    /// assuming where in the hierarchy the collider sits.
    /// </summary>
    public class ShipHitboxComponent : BaseHitboxComponent
    {
        [SerializeField] private BaseSpaceshipBehaviourComponent _ship;

        /// <summary>Goes through Unity's null check, so a destroyed ship returns a real null instead
        /// of an interface reference that throws on use.</summary>
        public ISpaceship Ship => _ship != null ? _ship : null;

        public override IDamageableTarget Target => Ship;

        /// <summary>Resolved on the way up when left unassigned, so existing prefabs keep working.</summary>
        private void Awake()
        {
            if (_ship == null)
            {
                _ship = GetComponentInParent<BaseSpaceshipBehaviourComponent>();
            }
        }

        /// <summary>Reads the tag off the ship root, so the collider object never needs tagging.</summary>
        public override bool IsSameTeamAs(GameObject other)
        {
            return _ship != null && _ship.gameObject.CompareTag(other.tag);
        }
    }
}
