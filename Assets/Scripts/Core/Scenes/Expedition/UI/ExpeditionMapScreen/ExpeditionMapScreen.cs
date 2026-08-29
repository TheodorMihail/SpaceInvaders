using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Expedition.ExpeditionMapScreen;

namespace SpaceInvaders.Scenes.Expedition
{
    public class ExpeditionMapScreen : Screen<ExpeditionMapScreenModel, ExpeditionMapScreenView, ExpeditionMapScreenController>,
        IScreenWithResult<ExpeditionMapScreenResult>
    {
        public struct ExpeditionMapScreenResult : IScreenResult
        {
            public bool Back { get; set; }
        }

        private ExpeditionMapScreenResult _result;

        public ExpeditionMapScreenResult GetResult()
        {
            return _result;
        }

        public void SetResult(ExpeditionMapScreenResult result)
        {
            _result = result;
        }
    }
}
