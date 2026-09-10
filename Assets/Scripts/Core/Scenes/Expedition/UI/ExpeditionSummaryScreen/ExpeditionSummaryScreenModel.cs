using BaseArchitecture.Core;
using SpaceInvaders.Project;
using static SpaceInvaders.Scenes.Expedition.ExpeditionSummaryScreen;

namespace SpaceInvaders.Scenes.Expedition
{
    public class ExpeditionSummaryScreenModel : Model, IModelWithParams<ExpeditionSummaryScreenParams>
    {
        public ExpeditionRunResultTypes Result { get; set; } = ExpeditionRunResultTypes.Defeated;
        public int DepthReached { get; set; } = 0;

        public void InitializeWithParameters(ExpeditionSummaryScreenParams parameters)
        {
            Result = parameters.RunResult.Result;
            DepthReached = parameters.RunResult.DepthReached;
        }
    }
}
