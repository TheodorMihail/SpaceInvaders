using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Expedition.ExpeditionLobbyScreen;

namespace SpaceInvaders.Scenes.Expedition
{
    public class ExpeditionLobbyScreenModel : Model, IModelWithParams<ExpeditionLobbyScreenParams>
    {
        public bool HasActiveRun { get; set; } = false;

        public void InitializeWithParameters(ExpeditionLobbyScreenParams parameters)
        {
            HasActiveRun = parameters.HasActiveRun;
        }
    }
}
