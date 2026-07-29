using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Game.GameOverScreen;

namespace SpaceInvaders.Scenes.Game
{
    public class GameOverScreen : Screen<GameOverModel, GameOverView, GameOverController>, IScreenWithResult<GameOverScreenResult>
    {
        public enum ResultTypes
        {
            Restart,
            MainMenu
        }

        public struct GameOverScreenResult : IScreenResult
        {
            public ResultTypes Result { get; set; }
        }

        private GameOverScreenResult _result;

        public GameOverScreenResult GetResult()
        {
            return _result;
        }

        public void SetResult(GameOverScreenResult result)
        {
            _result = result;
        }
    }
}
