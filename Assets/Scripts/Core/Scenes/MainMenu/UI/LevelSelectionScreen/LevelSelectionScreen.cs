using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.MainMenu.LevelSelectionScreen;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class LevelSelectionScreen : Screen<LevelSelectionModel, LevelSelectionView, LevelSelectionController>, IScreenWithResult<LevelSelectionScreenResult>
    {
        public struct LevelSelectionScreenResult : IScreenResult
        {
            public int LevelSelected { get; set; }
            public bool Back { get; set; }
        }
        
        private LevelSelectionScreenResult _result;

        public LevelSelectionScreenResult GetResult()
        {
            return _result;
        }

        public void SetResult(LevelSelectionScreenResult result)
        {
            _result = result;
        }
    }
}
