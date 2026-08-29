using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Expedition.ExpeditionLobbyScreen;

namespace SpaceInvaders.Scenes.Expedition
{
    public class ExpeditionLobbyScreen : Screen<ExpeditionLobbyScreenModel, ExpeditionLobbyScreenView, ExpeditionLobbyScreenController>,
        IScreenWithResult<ExpeditionLobbyScreenResult>
    {
        public enum ResultTypes
        {
            Continue,
            NewRun,
            Back
        }

        public struct ExpeditionLobbyScreenParams
        {
            public bool HasActiveRun { get; set; }
        }

        public struct ExpeditionLobbyScreenResult : IScreenResult
        {
            public ResultTypes Result { get; set; }
        }

        private ExpeditionLobbyScreenResult _result;

        public ExpeditionLobbyScreenResult GetResult()
        {
            return _result;
        }

        public void SetResult(ExpeditionLobbyScreenResult result)
        {
            _result = result;
        }
    }
}
