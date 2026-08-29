#if UNITY_EDITOR || DEVELOPMENT_BUILD
using BaseArchitecture.Core;

namespace SpaceInvaders.Project
{
    public partial class ExpeditionRunManager
    {
        public void DebugRerollMap()
        {
            StartNewRun();
            this.LogWarning($"Debug: Map rerolled with seed {_data.Seed}.");
        }

        public void DebugAbandonRun()
        {
            AbandonRun();
            this.LogWarning("Debug: Run abandoned.");
        }
    }
}
#endif
