#if UNITY_EDITOR || DEVELOPMENT_BUILD
using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Game
{
    public partial class PlayerManager
    {
        public void DebugKillPlayer()
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
