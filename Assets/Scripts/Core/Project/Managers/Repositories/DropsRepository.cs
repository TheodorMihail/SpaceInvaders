using System;
using System.Collections.Generic;
using BaseArchitecture.Core;

namespace SpaceInvaders.Project
{
    public interface IDropsRepository
    {
        IReadOnlyList<DropCategoryWeightDTO> GetDropCategoryWeights(DropTableTypes tableType);
    }

    public class DropsRepository : Repository, IDropsRepository
    {
        public DropsRepository(DropsDataConfigSO dropsDataConfigSO)
        {
            AddObjects(dropsDataConfigSO.DropTableConfigs);
        }

        /// <summary>A table nobody authored drops nothing, which is the safe way to be wrong.</summary>
        public IReadOnlyList<DropCategoryWeightDTO> GetDropCategoryWeights(DropTableTypes tableType)
        {
            return TryGet(tableType.ToString(), out DropTableConfigSO config)
                ? config.CategoryWeights
                : Array.Empty<DropCategoryWeightDTO>();
        }
    }
}
