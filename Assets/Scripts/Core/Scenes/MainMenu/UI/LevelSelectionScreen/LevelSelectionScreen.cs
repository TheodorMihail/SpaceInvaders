using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.MainMenu.LevelSelectionScreen;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class LevelSelectionScreen : Screen<LevelSelectionModel, LevelSelectionView, LevelSelectionController>, IScreenWithResult<LevelSelectionScreenResult>
    {
        public struct LevelSelectionScreenResult : IScreenResult
        {
            public int LevelSelected { get; set; }
        }
        
        private LevelSelectionScreenResult _result;
        public LevelSelectionScreenResult GetResult() => _result;
        public void SetResult(LevelSelectionScreenResult result) => _result = result;
    }
}
