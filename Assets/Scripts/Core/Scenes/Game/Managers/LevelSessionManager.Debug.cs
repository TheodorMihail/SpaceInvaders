#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace SpaceInvaders.Scenes.Game
{
    public partial class LevelSessionManager
    {
        public void DebugDestroyAllEnemies()
        {
            _enemiesService.DebugDestroyAllEnemies();
        }

        public void DebugSpawnHazard()
        {
            _hazardsService.DebugSpawnFirstHazard();
        }
    }
}
#endif
