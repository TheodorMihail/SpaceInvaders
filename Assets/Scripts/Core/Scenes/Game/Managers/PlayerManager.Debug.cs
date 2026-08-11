#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;

namespace SpaceInvaders.Scenes.Game
{
    public partial class PlayerManager : IDebugCommandProvider
    {
        public IReadOnlyList<DebugCommand> GetDebugCommands()
        {
            return new[]
            {
                new DebugCommand(DebugKeys.KillPlayer, "Kill player", DebugKillPlayer)
            };
        }

        private void DebugKillPlayer()
        {
            // No ship exists before it spawns or after it dies.
            if (_playerInstance == null)
            {
                return;
            }

            this.LogWarning("Debug: Destroying player");
            _playerInstance.TakeDamage(_playerInstance.Stats.CurrentHealth);
        }
    }
}
#endif
