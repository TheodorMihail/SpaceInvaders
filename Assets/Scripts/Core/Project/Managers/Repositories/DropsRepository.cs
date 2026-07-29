using System.Collections.Generic;
using BaseArchitecture.Core;

namespace SpaceInvaders.Project
{
    public interface IDropsRepository
    {
        IReadOnlyList<DropCategoryWeightDTO> GetAllDropCategoryWeights();
    }

    public class DropsRepository : Repository, IDropsRepository
    {
        public DropsRepository(DropTableConfigSO dropTableConfigSO)
        {
            AddObject(dropTableConfigSO);
        }

        public IReadOnlyList<DropCategoryWeightDTO> GetAllDropCategoryWeights()
        {
            TryGet(nameof(DropTableConfigSO), out DropTableConfigSO config);
            return config.CategoryWeights;
        }
    }
}
