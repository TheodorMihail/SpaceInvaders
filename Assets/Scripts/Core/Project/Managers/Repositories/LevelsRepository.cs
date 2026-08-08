using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Project
{
    public interface ILevelsRepository
    {
        bool TryGetLevelConfig(int level, out LevelConfigSO config);
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

        public bool TryGetLevelConfig(int level, out LevelConfigSO config)
        {
            return TryGet($"Level {level}", out config);
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
