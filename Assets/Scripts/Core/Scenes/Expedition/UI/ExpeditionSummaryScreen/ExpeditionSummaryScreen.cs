using BaseArchitecture.Core;
using SpaceInvaders.Project;

namespace SpaceInvaders.Scenes.Expedition
{
    public class ExpeditionSummaryScreen : Screen<ExpeditionSummaryScreenModel, ExpeditionSummaryScreenView, ExpeditionSummaryScreenController>
    {
        public struct ExpeditionSummaryScreenParams
        {
            public ExpeditionRunResultDTO RunResult { get; set; }
        }
    }
}
