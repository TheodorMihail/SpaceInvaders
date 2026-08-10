#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using System.Text;
using BaseArchitecture.Core;
using UnityEngine.InputSystem;
using Zenject;

namespace SpaceInvaders.Project
{
    /// <summary>Every debug hotkey, in one place so the whole allocation is visible at a glance.
    /// Duplicates are rejected at startup when the command table is built.</summary>
    public static class DebugKeys
    {
        //In game
        public const Key DestroyAllEnemies = Key.F1;
        public const Key KillPlayer = Key.F2;

        //In menu
        public const Key AddCurrency = Key.F5;
        public const Key AddRandomItem = Key.F6;
        public const Key ClearCurrency = Key.F8;
        public const Key ClearEquipment = Key.F9;
        public const Key ClearInventory = Key.F10;
        public const Key ClearTalents = Key.F11;
        public const Key ClearLevelProgress = Key.F12;
    }

    /// <summary>Owns the debug hotkey table for the scene it is bound in and dispatches presses to
    /// the provider that declared them. Bound per scene, so the table holds the project-scope
    /// providers plus whatever that scene contributes.</summary>
    public class DebugManager : IInitializable, ITickable
    {
        [Inject] private readonly List<IDebugCommandProvider> _providers;

        private readonly List<DebugCommandDTO> _commands = new List<DebugCommandDTO>();

        public void Initialize()
        {
            foreach (IDebugCommandProvider provider in _providers)
            {
                foreach (DebugCommandDTO command in provider.GetDebugCommands())
                {
                    if (_commands.Exists(existing => existing.Key == command.Key))
                    {
                        this.LogError($"Debug key {command.Key} is already bound. '{command.Label}' ignored.");
                        continue;
                    }

                    _commands.Add(command);
                }
            }

            LogCommands();
        }

        /// <summary>Lists everything bound in this scene, so the active keymap never has to be
        /// looked up in code.</summary>
        private void LogCommands()
        {
            var sorted = new List<DebugCommandDTO>(_commands);
            sorted.Sort((first, second) => first.Key.CompareTo(second.Key));

            var builder = new StringBuilder($"Debug commands ({sorted.Count}):");
            foreach (DebugCommandDTO command in sorted)
            {
                builder.AppendLine();
                builder.Append($"  {command.Key} - {command.Label}");
            }

            this.Log(builder.ToString());
        }

        public void Tick()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            foreach (DebugCommandDTO command in _commands)
            {
                if (Keyboard.current[command.Key].wasPressedThisFrame)
                {
                    this.LogWarning($"Debug: {command.Label} ({command.Key})");
                    command.Action();
                }
            }
        }
    }
}
#endif
