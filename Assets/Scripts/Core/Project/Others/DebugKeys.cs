#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;

namespace SpaceInvaders.Project
{
    /// <summary>Every debug hotkey, in one place so the whole allocation is visible at a glance.
    /// Duplicates are rejected at startup when the command table is built.</summary>
    public static class DebugKeys
    {
        //In game
        public const Key DestroyAllEnemies = Key.F1;
        public const Key KillPlayer = Key.F2;

        //Everywhere
        public const Key AddCurrency = Key.F5;
        public const Key AddRandomItem = Key.F6;
        public const Key ClearCurrency = Key.F8;
        public const Key ClearEquipment = Key.F9;
        public const Key ClearInventory = Key.F10;
        public const Key ClearTalents = Key.F11;
        public const Key ClearLevelProgress = Key.F12;
    }
}
#endif
