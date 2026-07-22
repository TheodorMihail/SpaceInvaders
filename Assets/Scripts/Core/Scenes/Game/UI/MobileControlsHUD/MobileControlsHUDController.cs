using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class MobileControlsHUDController : Controller<MobileControlsHUD, MobileControlsHUDModel, MobileControlsHUDView>
    {
        [Inject] private readonly IMessageBus _messageBus;

        public MobileControlsHUDController(MobileControlsHUD hud, MobileControlsHUDModel model, MobileControlsHUDView view)
            : base(hud, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            _messageBus.Subscribe<GameEndedMessage>(OnGameEnded);
        }

        public override void Dispose()
        {
            base.Dispose();

            _messageBus.Unsubscribe<GameEndedMessage>(OnGameEnded);
        }

        private void OnGameEnded(GameEndedMessage message)
        {
            Close();
        }
    }
}
