using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Enemy that leaves smaller ships behind when it dies, so killing it is only half the job. What
    /// it splits into is authored, so the same component covers any number of tiers.
    /// </summary>
    public class SplittingEnemySpaceshipBehaviourComponent : EnemySpaceshipBehaviourComponent
    {
        [Header("Split On Death")]
        [Tooltip("Ships left behind where this one dies. Whatever it splits into should not split again.")]
        [SerializeField] private EnemySpawnDTO _splitSpawn;

        /// <summary>Asks before the base raises its destroyed event, so the manager counts the children
        /// while the wave still looks occupied. The other order advances the level out from under them.</summary>
        protected override void Destroy()
        {
            RaiseSpawnRequest(new EnemySpawnRequestDTO(_splitSpawn, LocalPosition));

            base.Destroy();
        }
    }
}
