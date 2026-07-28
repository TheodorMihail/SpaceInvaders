using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Game.LevelFinishedScreen;

namespace SpaceInvaders.Scenes.Game
{
    public class LevelFinishedScreen : Screen<LevelFinishedModel, LevelFinishedView, LevelFinishedController>, IScreenWithResult<LevelFinishedScreenResult>
    {
        public enum ResultTypes
        {
            NextLevel,
            MainMenu
        }

        public struct LevelFinishedScreenResult : IScreenResult
        {
            public ResultTypes Result { get; set; }
        }

        private LevelFinishedScreenResult _result;

        public LevelFinishedScreenResult GetResult()
        {
            return _result;
        }

        public void SetResult(LevelFinishedScreenResult result)
        {
            _result = result;
        }
    }
}
