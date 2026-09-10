#if UNITY_EDITOR || DEVELOPMENT_BUILD
using BaseArchitecture.Core;

namespace SpaceInvaders.Project
{
    public partial class ExpeditionRunManager
    {
        public void DebugRerollMap()
        {
            StartNewExpedition();
            this.LogWarning($"Debug: Map rerolled with seed {_data.RunInProgress.Seed}.");
        }

        public void DebugAbandonExpedition()
        {
            AbandonExpedition();
            this.LogWarning("Debug: Expedition abandoned.");
        }
    }
}
#endif
