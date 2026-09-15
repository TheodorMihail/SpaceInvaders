#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace SpaceInvaders.Scenes.Game
{
    public partial class LevelSessionManager
    {
        /// <summary>Skips every wave left. The level still ends through the normal completion path, so
        /// scoring, hazards and the result all behave as they do on a level played out.</summary>
        public void DebugFinishLevel()
        {
            if (_currentLevelConfigSo == null)
            {
                return;
            }

            _currentWaveNumber = _currentLevelConfigSo.WavesConfigs.Count;

            // Clearing the field advances the wave on its own, but only when something was on it, so an
            // empty field is advanced here instead. Exactly one of the two ends the level.
            bool hasEnemies = _enemiesService.EnemiesAlive > 0;
            _enemiesService.DebugDestroyAllEnemies();

            if (!hasEnemies)
            {
                StartNextWave();
            }
        }

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
