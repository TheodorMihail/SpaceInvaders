using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class GameStartController : Controller<GameStartScreen, GameStartModel, GameStartView>
    {
        [Inject] private readonly IInputManager _inputManager;

        public GameStartController(GameStartScreen screen, GameStartModel model, GameStartView view)
            : base(screen, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _inputManager.OnAnyKeyPress += HandleGameStartTrigger;
        }

        public override void Dispose()
        {
            base.Dispose();
            _inputManager.OnAnyKeyPress -= HandleGameStartTrigger;
        }

        private async void HandleGameStartTrigger()
        {
            _inputManager.OnAnyKeyPress -= HandleGameStartTrigger;
            
            try
            {
                await _view.StartCountdownAnimation(_model.CountdownSeconds, _model.CountdownEndDelayTimerSeconds);
            }
            catch {}

            Close();
        }
    }
}
