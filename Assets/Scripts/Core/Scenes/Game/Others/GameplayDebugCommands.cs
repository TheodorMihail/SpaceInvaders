#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// The cheats for a running level. The managers own the actions, so they keep reaching their own
    /// private state; this only decides which of them the Game scene exposes and on which key.
    /// </summary>
    public class GameplayDebugCommands : IDebugCommandProvider
    {
        [Inject] private readonly LevelSessionManager _levelSessionManager;
        [Inject] private readonly PlayerManager _playerManager;
        [Inject] private readonly LootManager _lootManager;

        public IReadOnlyList<DebugCommandDTO> GetDebugCommands()
        {
            return new[]
            {
                new DebugCommandDTO(DebugKeys.Gameplay.DestroyAllEnemies, "Destroy all enemies", _levelSessionManager.DebugDestroyAllEnemies),
                new DebugCommandDTO(DebugKeys.Gameplay.KillPlayer, "Kill player", _playerManager.DebugKillPlayer),
                new DebugCommandDTO(DebugKeys.Gameplay.SpawnHazard, "Spawn a hazard", _levelSessionManager.DebugSpawnHazard),
                new DebugCommandDTO(DebugKeys.Gameplay.SpawnPowerup, "Spawn the next powerup", _lootManager.DebugSpawnPowerup)
            };
        }
    }
}
#endif
