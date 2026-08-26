using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Enemy that spawns smaller ships when it dies. What it splits into is authored, so the same
    /// component covers any number of tiers.
    /// </summary>
    public class SplittingEnemySpaceshipBehaviourComponent : EnemySpaceshipBehaviourComponent
    {
        [Header("Split On Death")]
        [Tooltip("Ships left behind where this one dies. Whatever it splits into should not split again.")]
        [SerializeField] private EnemySpawnDTO _splitSpawn;

        /// <summary>Requests the split before the destroyed event, so the children are counted while
        /// the wave is still active. The other order advances the level before they spawn.</summary>
        protected override void Destroy()
        {
            RaiseSpawnRequest(new EnemySpawnRequestDTO(_splitSpawn, LocalPosition));

            base.Destroy();
        }
    }
}
