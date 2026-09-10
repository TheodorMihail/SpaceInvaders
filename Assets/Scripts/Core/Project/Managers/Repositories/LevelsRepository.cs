using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Project
{
    public interface ILevelsRepository
    {
        /// <summary>The id a level is stored under, so nothing else has to know the key format.</summary>
        string GetLevelId(int level);

        bool TryGetLevelConfig(int level, out LevelConfigSO config);
        bool TryGetLevelConfigById(string levelId, out LevelConfigSO config);

        /// <summary>Silent, unlike TryGet, so saved data can be validated without logging a miss.</summary>
        bool ContainsLevelConfig(string levelId);

        IReadOnlyList<LevelConfigSO> GetLevelConfigs();
        int GetLevelsCount();
        float GetTwoStarDamageMultiplier();
    }

    public class LevelsRepository : Repository, ILevelsRepository
    {
        public LevelsRepository(LevelsDataConfigSO levelsDataConfigSO)
        {
            AddObjects(levelsDataConfigSO.LevelsConfigs);
            AddObject(levelsDataConfigSO);
        }

        public string GetLevelId(int level)
        {
            return $"Level {level}";
        }

        public bool TryGetLevelConfig(int level, out LevelConfigSO config)
        {
            return TryGet(GetLevelId(level), out config);
        }

        public bool TryGetLevelConfigById(string levelId, out LevelConfigSO config)
        {
            return TryGet(levelId, out config);
        }

        public bool ContainsLevelConfig(string levelId)
        {
            foreach (LevelConfigSO config in GetAll<LevelConfigSO>())
            {
                if (config.ObjectID == levelId)
                {
                    return true;
                }
            }

            return false;
        }

        public IReadOnlyList<LevelConfigSO> GetLevelConfigs()
        {
            return GetAll<LevelConfigSO>().ToArray();
        }

        public int GetLevelsCount()
        {
            return GetAll<LevelConfigSO>().Count();
        }

        public float GetTwoStarDamageMultiplier()
        {
            TryGet(nameof(LevelsDataConfigSO), out LevelsDataConfigSO config);
            return config.TwoStarDamageMultiplier;
        }
    }
}
