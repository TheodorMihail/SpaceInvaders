#if UNITY_EDITOR || DEVELOPMENT_BUILD
using BaseArchitecture.Core;

namespace SpaceInvaders.Project
{
    public partial class EquipmentManager
    {
        public void DebugClearEquipment()
        {
            _data.Slots.Clear();
            SaveData();
            this.LogWarning("Debug: Equipment cleared.");
        }
    }
}
#endif
