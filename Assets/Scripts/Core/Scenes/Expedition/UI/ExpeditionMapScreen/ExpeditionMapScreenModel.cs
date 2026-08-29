using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Expedition
{
    public class ExpeditionMapScreenModel : Model
    {
        [Inject] private readonly IExpeditionRunManager _expeditionRunManager;

        public IReadOnlyList<ExpeditionNodeEntry> Nodes => _expeditionRunManager.Nodes;
        public int CurrentNodeId => _expeditionRunManager.CurrentNodeId;
    }
}
