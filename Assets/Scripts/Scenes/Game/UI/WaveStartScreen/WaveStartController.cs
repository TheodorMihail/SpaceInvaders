using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Game
{
    public class WaveStartController : Controller<WaveStartScreen, WaveStartModel, WaveStartView>
    {
        public WaveStartController(WaveStartScreen screen, WaveStartModel model, WaveStartView view)
            : base(screen, model, view)
        {
        }

        public override async void Initialize()
        {
            base.Initialize();

            _view.SetWaveNumber(_model.WaveNumber);
            await _view.PlayAnimation(_model.AnimationDurationSeconds);

            Close();
        }
    }
}
