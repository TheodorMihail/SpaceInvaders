using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Game.GamePausedScreen;

namespace SpaceInvaders.Scenes.Game
{
    public class GamePausedScreen : Screen<GamePausedScreenModel, GamePausedScreenView, GamePausedScreenController>, IScreenWithResult<GamePausedScreenResult>
    {
        public enum ResultTypes
        {
            Resume,
            Restart,
            Quit
        }

        public struct GamePausedScreenResult : IScreenResult
        {
            public ResultTypes Result { get; set; }
        }

        private GamePausedScreenResult _result;

        public GamePausedScreenResult GetResult()
        {
            return _result;
        }

        public void SetResult(GamePausedScreenResult result)
        {
            _result = result;
        }
    }
}
