using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class SettingsController : Controller<SettingsScreen, SettingsModel, SettingsView>
    {
        public SettingsController(SettingsScreen screen, SettingsModel model, SettingsView view) : base(screen, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _view.Setup(_model.MusicVolume, _model.SfxVolume);

            _view.OnBackClicked += HandleBackClicked;
            _view.OnMusicVolumeChanged += HandleMusicVolumeChanged;
            _view.OnSfxVolumeChanged += HandleSfxVolumeChanged;
        }

        public override void Dispose()
        {
            _view.OnBackClicked -= HandleBackClicked;
            _view.OnMusicVolumeChanged -= HandleMusicVolumeChanged;
            _view.OnSfxVolumeChanged -= HandleSfxVolumeChanged;
            base.Dispose();
        }

        private void HandleBackClicked()
        {
            Close();
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
