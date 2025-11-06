using System.Collections.Generic;
using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Game
{
    public interface IRepositoryManager
    {
        LevelConfigSO GetLevelConfig(LevelTypes levelType);
        PlayerSpaceshipConfigSO GetPlayerConfig(PlayerTypes playerType);
        EnemySpaceshipConfigSO GetEnemyConfig(EnemyTypes enemyType);
        int GetLevelsCount();
    }

    public class RepositoryManager : Repository, IRepositoryManager
    {
        private int _levelsCount;

        public RepositoryManager(
            List<LevelConfigSO> levelsConfigs,
            List<PlayerSpaceshipConfigSO> playersConfigs,
            List<EnemySpaceshipConfigSO> enemiesConfigs)
        {
            AddObjects(levelsConfigs);
            AddObjects(playersConfigs);
            AddObjects(enemiesConfigs);
            _levelsCount = levelsConfigs.Count;
        }


        public LevelConfigSO GetLevelConfig(LevelTypes levelType)
        {
            return GetObject(levelType.ToString()) as LevelConfigSO;
        }

        public PlayerSpaceshipConfigSO GetPlayerConfig(PlayerTypes playerType)
        {
            return GetObject(playerType.ToString()) as PlayerSpaceshipConfigSO;
        }

        public EnemySpaceshipConfigSO GetEnemyConfig(EnemyTypes enemyType)
        {
            return GetObject(enemyType.ToString()) as EnemySpaceshipConfigSO;
        }

        public int GetLevelsCount()
        {
            return _levelsCount;
        }
    }
}
