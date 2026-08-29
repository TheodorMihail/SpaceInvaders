#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Expedition
{
    /// <summary>
    /// The cheats for a run. The manager owns the actions; this only decides which the Expedition
    /// scene exposes and on which key.
    /// </summary>
    public class ExpeditionDebugCommands : IDebugCommandProvider
    {
        [Inject] private readonly ExpeditionRunManager _expeditionRunManager;

        public IReadOnlyList<DebugCommandDTO> GetDebugCommands()
        {
            return new[]
            {
                new DebugCommandDTO(DebugKeys.Expedition.RerollMap, "Reroll the map", _expeditionRunManager.DebugRerollMap),
                new DebugCommandDTO(DebugKeys.Expedition.AbandonRun, "Abandon the run", _expeditionRunManager.DebugAbandonRun)
            };
        }
    }
}
#endif
