#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;

namespace SpaceInvaders.Scenes.Game
{
    public partial class LevelSessionManager : IDebugCommandProvider
    {
        public IReadOnlyList<DebugCommandDTO> GetDebugCommands()
        {
            return new[]
            {
                new DebugCommandDTO(DebugKeys.DestroyAllEnemies, "Destroy all enemies", DebugDestroyAllEnemies),
                new DebugCommandDTO(DebugKeys.SpawnHazard, "Spawn a hazard", DebugSpawnHazard)
            };
        }

        private void DebugDestroyAllEnemies()
        {
            _enemiesService.DebugDestroyAllEnemies();
        }

        private void DebugSpawnHazard()
        {
            _hazardsService.DebugSpawnFirstHazard();
        }
    }
}
#endif
