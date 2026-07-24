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
        float GetTwoStarDamageMultiplier();
        int GetLevelsCount();
        ProjectDataConfigSO GetProjectDataConfig();
        TalentConfigSO GetTalentConfig(TalentTypes talentType);
        IReadOnlyList<TalentConfigSO> GetAllTalentConfigs();
    }

    public class RepositoryManager : Repository, IRepositoryManager
    {
        public RepositoryManager(
            LevelsDataConfigSO levelsDataConfigSO,
            PlayerDataConfigSO playerDataConfigSO,
            EnemyDataConfigSO enemyDataConfigSO,
            PowerupsDataConfigSO powerupsDataConfigSO,
            ProjectDataConfigSO projectDataConfigSO,
            TalentsDataConfigSO talentsDataConfigSO)
        {
            AddObjects(levelsDataConfigSO.LevelsConfigs);
            AddObjects(playerDataConfigSO.PlayerConfigs);
            AddObjects(enemyDataConfigSO.EnemyConfigs);
            AddObjects(powerupsDataConfigSO.PowerupConfigs);
            AddObjects(talentsDataConfigSO.TalentConfigs);

            AddObject(levelsDataConfigSO);
            AddObject(playerDataConfigSO);
            AddObject(enemyDataConfigSO);
            AddObject(powerupsDataConfigSO);
            AddObject(projectDataConfigSO);
            AddObject(talentsDataConfigSO);
        }

        public LevelConfigSO GetLevelConfig(int level)
        {
            return Get<LevelConfigSO>($"Level {level}");
        }

        public IReadOnlyList<LevelConfigSO> GetLevelConfigs()
        {
            return GetAll<LevelConfigSO>().ToArray();
        }

        public PlayerSpaceshipConfigSO GetPlayerConfig(PlayerTypes playerType)
        {
            return Get<PlayerSpaceshipConfigSO>(playerType.ToString());
        }

        public EnemySpaceshipConfigSO GetEnemyConfig(EnemyTypes enemyType)
        {
            return Get<EnemySpaceshipConfigSO>(enemyType.ToString());
        }

        public PowerupConfigSO GetPowerupConfig(PowerupTypes powerupType)
        {
            return Get<PowerupConfigSO>(powerupType.ToString());
        }

        public IReadOnlyList<PowerupConfigSO> GetAllPowerupConfigs()
        {
            return GetAll<PowerupConfigSO>().ToArray();
        }

        public float GetPowerupDropChance()
        {
            return Get<PowerupsDataConfigSO>(nameof(PowerupsDataConfigSO)).GlobalPowerupDropChance;
        }

        public float GetTwoStarDamageMultiplier()
        {
            return Get<LevelsDataConfigSO>(nameof(LevelsDataConfigSO)).TwoStarDamageMultiplier;
        }

        public int GetLevelsCount()
        {
            return GetAll<LevelConfigSO>().Count();
        }

        public ProjectDataConfigSO GetProjectDataConfig()
        {
            return Get<ProjectDataConfigSO>(nameof(ProjectDataConfigSO));
        }

        public TalentConfigSO GetTalentConfig(TalentTypes talentType)
        {
            return Get<TalentConfigSO>(talentType.ToString());
        }

        public IReadOnlyList<TalentConfigSO> GetAllTalentConfigs()
        {
            return GetAll<TalentConfigSO>().ToArray();
        }
    }
}
