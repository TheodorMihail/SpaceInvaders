#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using BaseArchitecture.Core;

namespace SpaceInvaders.Project
{
    public partial class TalentManager : IDebugCommandProvider
    {
        public IReadOnlyList<DebugCommand> GetDebugCommands()
        {
            return new[]
            {
                new DebugCommand(DebugKeys.ClearTalents, "Clear talents", DebugClearTalents)
            };
        }

        private void DebugClearTalents()
        {
            _data.Talents.Clear();
            SaveData();
            this.LogWarning("Debug: Talents cleared.");
        }
    }
}
#endif
