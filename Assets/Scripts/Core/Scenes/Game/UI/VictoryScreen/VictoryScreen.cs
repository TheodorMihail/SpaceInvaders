using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Game.VictoryScreen;

namespace SpaceInvaders.Scenes.Game
{
    public class VictoryScreen : Screen<VictoryScreenModel, VictoryScreenView, VictoryScreenController>, IScreenWithResult<VictoryScreenResult>
    {
        public enum ResultTypes
        {
            NextLevel,
            Retry,
            MainMenu
        }

        public struct VictoryScreenResult : IScreenResult
        {
            public ResultTypes Result { get; set; }
        }

        private VictoryScreenResult _result;

        public VictoryScreenResult GetResult()
        {
            return _result;
        }

        public void SetResult(VictoryScreenResult result)
        {
            _result = result;
        }
    }
}
