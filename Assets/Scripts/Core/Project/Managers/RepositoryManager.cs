using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Project
{
    public interface IRepositoryManager
    {
        LevelConfigSO GetLevelConfig(int level);
        IReadOnlyList<LevelConfigSO> GetLevelConfigs();
        PlayerSpaceshipConfigSO GetPlayerConfig(PlayerTypes playerType);
        EnemySpaceshipConfigSO GetEnemyConfig(EnemyTypes enemyType);
        PowerupConfigSO GetPowerupConfig(PowerupTypes powerupType);
        IReadOnlyList<PowerupConfigSO> GetAllPowerupConfigs();
        float GetPowerupDropChance();
        int GetLevelsCount();
    }

    public class RepositoryManager : Repository, IRepositoryManager
    {
        private readonly float _globalPowerupDropChance;

        public RepositoryManager(
            List<LevelConfigSO> levelsConfigs,
            List<PlayerSpaceshipConfigSO> playersConfigs,
            List<EnemySpaceshipConfigSO> enemiesConfigs,
            List<PowerupConfigSO> powerupConfigs,
            float globalPowerupDropChance)
        {
            AddObjects(levelsConfigs);
            AddObjects(playersConfigs);
            AddObjects(enemiesConfigs);
            AddObjects(powerupConfigs);
            _globalPowerupDropChance = globalPowerupDropChance;
        }

        public LevelConfigSO GetLevelConfig(int level) => Get<LevelConfigSO>($"Level {level}");

        public IReadOnlyList<LevelConfigSO> GetLevelConfigs() => GetAll<LevelConfigSO>().ToArray();

        public PlayerSpaceshipConfigSO GetPlayerConfig(PlayerTypes playerType) =>
            Get<PlayerSpaceshipConfigSO>(playerType.ToString());

        public EnemySpaceshipConfigSO GetEnemyConfig(EnemyTypes enemyType) =>
            Get<EnemySpaceshipConfigSO>(enemyType.ToString());

        public PowerupConfigSO GetPowerupConfig(PowerupTypes powerupType) =>
            Get<PowerupConfigSO>(powerupType.ToString());

        public IReadOnlyList<PowerupConfigSO> GetAllPowerupConfigs() => GetAll<PowerupConfigSO>().ToArray();

        public float GetPowerupDropChance() => _globalPowerupDropChance;

        public int GetLevelsCount() => GetAll<LevelConfigSO>().Count();
    }
}
