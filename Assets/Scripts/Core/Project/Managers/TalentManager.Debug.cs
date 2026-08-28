#if UNITY_EDITOR || DEVELOPMENT_BUILD
using BaseArchitecture.Core;

namespace SpaceInvaders.Project
{
    public partial class TalentManager
    {
        public void DebugClearTalents()
        {
            _data.Talents.Clear();
            SaveData();
            this.LogWarning("Debug: Talents cleared.");
        }
    }
}
#endif
