using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Game
{
    public interface IRepositoryManager
    {
        LevelConfigSO GetLevelConfig(int level);
        IReadOnlyList<LevelConfigSO> GetLevelConfigs();
        PlayerSpaceshipConfigSO GetPlayerConfig(PlayerTypes playerType);
        EnemySpaceshipConfigSO GetEnemyConfig(EnemyTypes enemyType);
        int GetLevelsCount();
    }

    public class RepositoryManager : Repository, IRepositoryManager
    {
        public RepositoryManager(
            List<LevelConfigSO> levelsConfigs,
            List<PlayerSpaceshipConfigSO> playersConfigs,
            List<EnemySpaceshipConfigSO> enemiesConfigs)
        {
            AddObjects(levelsConfigs);
            AddObjects(playersConfigs);
            AddObjects(enemiesConfigs);
        }

        public LevelConfigSO GetLevelConfig(int level) => Get<LevelConfigSO>($"Level {level}");

        public IReadOnlyList<LevelConfigSO> GetLevelConfigs() => GetAll<LevelConfigSO>().ToArray();

        public PlayerSpaceshipConfigSO GetPlayerConfig(PlayerTypes playerType) =>
            Get<PlayerSpaceshipConfigSO>(playerType.ToString());

        public EnemySpaceshipConfigSO GetEnemyConfig(EnemyTypes enemyType) =>
            Get<EnemySpaceshipConfigSO>(enemyType.ToString());

        public int GetLevelsCount() => GetAll<LevelConfigSO>().Count();
    }
}
