using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>Makes a hazard shootable, without it having to be a ship to take a hit.</summary>
    public class HazardHitboxComponent : BaseHitboxComponent
    {
        private const string EnemyTag = "Enemy";

        [SerializeField] private BaseHazardBehaviourComponent _hazard;

        public override IDamageableTarget Target => _hazard != null ? _hazard : null;

        /// <summary>Resolved on the way up when left unassigned, so the collider can sit anywhere.</summary>
        private void Awake()
        {
            if (_hazard == null)
            {
                _hazard = GetComponentInParent<BaseHazardBehaviourComponent>();
            }
        }

        /// <summary>Enemy fire passes straight through: a wave should be able to neither farm a
        /// hazard's payout nor deny it to the player.</summary>
        public override bool IsSameTeamAs(GameObject other)
        {
            return other.CompareTag(EnemyTag);
        }
    }
}
