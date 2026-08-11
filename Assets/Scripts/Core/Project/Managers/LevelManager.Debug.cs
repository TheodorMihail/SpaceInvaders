#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using BaseArchitecture.Core;

namespace SpaceInvaders.Project
{
    public partial class LevelManager : IDebugCommandProvider
    {
        public IReadOnlyList<DebugCommandDTO> GetDebugCommands()
        {
            return new[]
            {
                new DebugCommandDTO(DebugKeys.ClearLevelProgress, "Clear level progress", DebugClearLevelProgress)
            };
        }

        private void DebugClearLevelProgress()
        {
            _data.Levels.Clear();
            GetOrCreateLevelProgress(1).Unlocked = true;
            SaveData();
            this.LogWarning("Debug: Level progress cleared.");
        }
    }
}
#endif
