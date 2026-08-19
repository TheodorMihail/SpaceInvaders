#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;

namespace SpaceInvaders.Scenes.Game
{
    public partial class HazardsService : IDebugCommandProvider
    {
        public IReadOnlyList<DebugCommandDTO> GetDebugCommands()
        {
            return new[]
            {
                new DebugCommandDTO(DebugKeys.SpawnHazard, "Spawn a hazard", DebugSpawnHazard)
            };
        }

        /// <summary>Sends in the first authored hazard, so a level with none still has something to look at.</summary>
        private void DebugSpawnHazard()
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
