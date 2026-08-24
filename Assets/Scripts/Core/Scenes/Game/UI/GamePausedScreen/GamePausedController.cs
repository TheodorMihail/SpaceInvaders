using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class GamePausedController : Controller<GamePausedScreen, GamePausedModel, GamePausedView>
    {
        [Inject] private readonly IInputManager _inputManager;

        public GamePausedController(GamePausedScreen screen, GamePausedModel model, GamePausedView view) : base(screen, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _view.Setup(_model.MusicVolume, _model.SfxVolume);

            _view.OnResumeButtonClicked += HandleResumeButtonClicked;
            _view.OnRestartButtonClicked += HandleRestartButtonClicked;
            _view.OnQuitButtonClicked += HandleQuitButtonClicked;
            _view.OnMusicVolumeChanged += HandleMusicVolumeChanged;
            _view.OnSfxVolumeChanged += HandleSfxVolumeChanged;

            _inputManager.OnPause += HandleResumeButtonClicked;
        }

        public override void Dispose()
        {
            _view.OnResumeButtonClicked -= HandleResumeButtonClicked;
            _view.OnRestartButtonClicked -= HandleRestartButtonClicked;
            _view.OnQuitButtonClicked -= HandleQuitButtonClicked;
            _view.OnMusicVolumeChanged -= HandleMusicVolumeChanged;
            _view.OnSfxVolumeChanged -= HandleSfxVolumeChanged;

            _inputManager.OnPause -= HandleResumeButtonClicked;
            base.Dispose();
        }

        private void HandleResumeButtonClicked()
        {
            CloseScreenWithResult(new GamePausedScreen.GamePausedScreenResult
            {
                Result = GamePausedScreen.ResultTypes.Resume
            });
        }

        private void HandleRestartButtonClicked()
        {
            CloseScreenWithResult(new GamePausedScreen.GamePausedScreenResult
            {
                Result = GamePausedScreen.ResultTypes.Restart
            });
        }

        private void HandleQuitButtonClicked()
        {
            CloseScreenWithResult(new GamePausedScreen.GamePausedScreenResult
            {
                Result = GamePausedScreen.ResultTypes.Quit
            });
        }

        private void HandleMusicVolumeChanged(float volume)
        {
            _model.SetMusicVolume(volume);
        }

        private void HandleSfxVolumeChanged(float volume)
        {
            _model.SetSfxVolume(volume);
        }
    }
}
