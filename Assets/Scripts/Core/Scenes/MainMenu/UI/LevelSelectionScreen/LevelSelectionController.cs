using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class LevelSelectionController : Controller<LevelSelectionScreen, LevelSelectionModel, LevelSelectionView>
    {
        public LevelSelectionController(LevelSelectionScreen uiComponent, LevelSelectionModel model, LevelSelectionView view)
            : base(uiComponent, model, view)
        {
        }
        
        public override void Initialize()
        {
            base.Initialize();
            _view.OnLevelSelectedClicked += OnLevelSelectedClicked;
        }

        public override void Dispose()
        {
            _view.OnLevelSelectedClicked -= OnLevelSelectedClicked;
            base.Dispose();
        }

        private void OnLevelSelectedClicked(int levelSelected)
        {
            CloseScreenWithResult(new LevelSelectionScreen.LevelSelectionScreenResult
            {
                LevelSelected = levelSelected
            });
        }
    }
}
