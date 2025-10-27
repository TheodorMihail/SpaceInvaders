using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Game.GameFinishedScreen;

namespace SpaceInvaders.Scenes.Game
{
    public class GameFinishedScreen : Screen<GameFinishedModel, GameFinishedView, GameFinishedController>, IScreenWithResult<GameFinishedScreenResult>
    {
        public enum ResultType
        {
            NextLevel,
            MainMenu
        }

        public struct GameFinishedScreenResult : IScreenResult
        {
            public ResultType Result { get; set; }
        }

        private GameFinishedScreenResult _result;
        public GameFinishedScreenResult GetResult() => _result;
        public void SetResult(GameFinishedScreenResult result) => _result = result;
    }
}
