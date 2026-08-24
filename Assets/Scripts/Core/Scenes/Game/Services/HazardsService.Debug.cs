#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;

namespace SpaceInvaders.Scenes.Game
{
    public partial class HazardsService
    {
        /// <summary>Sends in the first authored hazard, so a level with none still has something to look at.</summary>
        public void DebugSpawnFirstHazard()
        {
            IReadOnlyList<HazardConfigSO> configs = _hazardsRepository.GetAllHazardConfigs();

            if (configs.Count == 0 || configs[0].HazardPrefab == null)
            {
                this.LogWarning("Debug: no hazard config or prefab authored");
                return;
            }

            this.LogWarning($"Debug: Spawning hazard {configs[0].HazardType}");
            SpawnHazard(configs[0]);
        }
    }
}
#endif
