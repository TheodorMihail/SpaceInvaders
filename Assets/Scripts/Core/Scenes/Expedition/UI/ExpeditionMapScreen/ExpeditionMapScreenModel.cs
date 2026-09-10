using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Expedition
{
    public class ExpeditionMapScreenModel : Model
    {
        [Inject] private readonly IExpeditionRunManager _expeditionRunManager;

        /// <summary>Never null here: the map is only reachable while an expedition is under way.</summary>
        public IExpeditionState Expedition => _expeditionRunManager.CurrentExpedition;
    }
}
