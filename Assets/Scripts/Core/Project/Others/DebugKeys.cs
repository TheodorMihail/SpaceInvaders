#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;

namespace SpaceInvaders.Project
{
    /// <summary>Every debug hotkey, in one place. Duplicates are rejected at startup when the
    /// command table is built.</summary>
    public static class DebugKeys
    {
        //In game
        public const Key DestroyAllEnemies = Key.F1;
        public const Key KillPlayer = Key.F2;
        public const Key SpawnHazard = Key.F3;
        public const Key SpawnPowerup = Key.F4;

        //Everywhere
        public const Key AddCurrency = Key.F5;
        public const Key AddRandomItem = Key.F6;
        public const Key ClearCurrency = Key.F8;
        public const Key ClearEquipment = Key.F9;
        public const Key ClearInventory = Key.F10;
        public const Key ClearTalents = Key.F11;
        public const Key ClearLevelProgress = Key.F12;
        public const Key TakeScreenshot = Key.Backquote;
    }
}
#endif
