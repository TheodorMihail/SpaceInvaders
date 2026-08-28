#if UNITY_EDITOR || DEVELOPMENT_BUILD
using BaseArchitecture.Core;

namespace SpaceInvaders.Project
{
    public partial class LevelProgressManager
    {
        public void DebugClearLevelProgress()
        {
            _data.Levels.Clear();
            GetOrCreateLevelProgress(1).Unlocked = true;
            SaveData();
            this.LogWarning("Debug: Level progress cleared.");
        }
    }
}
#endif
