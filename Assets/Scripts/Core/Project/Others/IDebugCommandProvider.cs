#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace SpaceInvaders.Project
{
    /// <summary>Implemented by anything exposing debug cheats. Providers are collected by the
    /// active scene's debug manager, which owns the dispatching.</summary>
    public interface IDebugCommandProvider
    {
        IReadOnlyList<DebugCommandDTO> GetDebugCommands();
    }

    public readonly struct DebugCommandDTO
    {
        public readonly Key Key;
        public readonly string Label;
        public readonly Action Action;

        public DebugCommandDTO(Key key, string label, Action action)
        {
            Key = key;
            Label = label;
            Action = action;
        }
    }
}
#endif
