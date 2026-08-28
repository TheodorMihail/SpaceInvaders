#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;

namespace SpaceInvaders.Project
{
    /// <summary>
    /// Every debug hotkey, grouped by the scene that owns it. Keys repeat across groups on purpose:
    /// a scene only ever builds the commands from its own group plus the global ones, and duplicates
    /// within one scene are rejected at startup when the command table is built.
    /// </summary>
    public static class DebugKeys
    {
        /// <summary>Available in every scene, since capturing the screen is a tool rather than a cheat.</summary>
        public static class Global
        {
            public const Key TakeScreenshot = Key.Backquote;
        }

        /// <summary>Campaign progression, so only reachable from the Campaign scene.</summary>
        public static class Campaign
        {
            public const Key AddCurrency = Key.F1;
            public const Key ClearCurrency = Key.F2;
            public const Key AddRandomItem = Key.F3;
            public const Key ClearInventory = Key.F4;
            public const Key ClearEquipment = Key.F5;
            public const Key ClearTalents = Key.F6;
            public const Key ClearLevelProgress = Key.F7;
        }

        /// <summary>A running level, so only reachable from the Game scene.</summary>
        public static class Gameplay
        {
            public const Key DestroyAllEnemies = Key.F1;
            public const Key KillPlayer = Key.F2;
            public const Key SpawnHazard = Key.F3;
            public const Key SpawnPowerup = Key.F4;
        }
    }
}
#endif
