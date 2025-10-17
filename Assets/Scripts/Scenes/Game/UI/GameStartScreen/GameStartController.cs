using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class GameStartController : Controller<GameStartScreen, GameStartModel, GameStartView>
    {
        [Inject] private readonly IInputService _inputService;

        public GameStartController(GameStartScreen screen, GameStartModel model, GameStartView view)
            : base(screen, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _inputService.OnAnyKeyPress += HandleGameStartTrigger;
        }

        public override void Dispose()
        {
            base.Dispose();
            _inputService.OnAnyKeyPress -= HandleGameStartTrigger;
        }

        private async void HandleGameStartTrigger()
        {
            _inputService.OnAnyKeyPress -= HandleGameStartTrigger;
            await _view.StartCountdownAnimation(_model.CountdownSeconds);
            Close();
        }
    }
}
