using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class LevelSelectionController : Controller<LevelSelectionScreen, LevelSelectionModel, LevelSelectionView>
    {
        [Inject] private readonly IRepositoryManager _repository;

        public LevelSelectionController(LevelSelectionScreen uiComponent, LevelSelectionModel model, LevelSelectionView view)
            : base(uiComponent, model, view)
        {
        }
        
        public override void Initialize()
        {
            base.Initialize();
            _view.OnLevelSelectedClicked += OnLevelSelectedClicked;
            _view.OnBackClicked += OnBackClicked;
            _view.SetupLevels(_repository.GetLevelConfigs());
        }

        public override void Dispose()
        {
            _view.OnLevelSelectedClicked -= OnLevelSelectedClicked;
            _view.OnBackClicked -= OnBackClicked;
            base.Dispose();
        }

        private void OnLevelSelectedClicked(int levelSelected)
        {
            CloseScreenWithResult(new LevelSelectionScreen.LevelSelectionScreenResult
            {
                LevelSelected = levelSelected
            });
        }

        private void OnBackClicked()
        {
            CloseScreenWithResult(new LevelSelectionScreen.LevelSelectionScreenResult
            {
                Back = true
            });
        }
    }
}
