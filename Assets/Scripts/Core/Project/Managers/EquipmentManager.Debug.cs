#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using BaseArchitecture.Core;

namespace SpaceInvaders.Project
{
    public partial class EquipmentManager : IDebugCommandProvider
    {
        public IReadOnlyList<DebugCommandDTO> GetDebugCommands()
        {
            return new[]
            {
                new DebugCommandDTO(DebugKeys.ClearEquipment, "Clear equipment", DebugClearEquipment)
            };
        }

        private void DebugClearEquipment()
        {
            _data.Slots.Clear();
            SaveData();
            this.LogWarning("Debug: Equipment cleared.");
        }
    }
}
#endif
