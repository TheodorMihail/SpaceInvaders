#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;

namespace SpaceInvaders.Scenes.Game
{
    public partial class EnemiesManager : IDebugCommandProvider
    {
        public IReadOnlyList<DebugCommand> GetDebugCommands()
        {
            return new[]
            {
                new DebugCommand(DebugKeys.DestroyAllEnemies, "Destroy all enemies", DebugDestroyAllEnemies)
            };
        }

        private void DebugDestroyAllEnemies()
        {
            // The list only exists once the game has initialized.
            if (_spawnedEnemies == null)
            {
                return;
            }

            this.LogWarning("Debug: Destroying all enemies");

            // Routed through the destroyed callback so the wave still advances.
            for (int i = _spawnedEnemies.Count - 1; i >= 0; i--)
            {
                OnEnemyDestroyedCallback(_spawnedEnemies[i]);
            }
        }
    }
}
#endif
