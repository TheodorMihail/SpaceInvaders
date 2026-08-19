using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Project
{
    public interface IHazardsRepository
    {
        bool TryGetHazardConfig(HazardTypes hazardType, out HazardConfigSO config);
        IReadOnlyList<HazardConfigSO> GetAllHazardConfigs();
    }

    public class HazardsRepository : Repository, IHazardsRepository
    {
        public HazardsRepository(HazardsDataConfigSO hazardsDataConfigSO)
        {
            AddObjects(hazardsDataConfigSO.HazardConfigs);
            AddObject(hazardsDataConfigSO);
        }

        public bool TryGetHazardConfig(HazardTypes hazardType, out HazardConfigSO config)
        {
            return TryGet(hazardType.ToString(), out config);
        }

        public IReadOnlyList<HazardConfigSO> GetAllHazardConfigs()
        {
            return GetAll<HazardConfigSO>().ToArray();
        }
    }
}
