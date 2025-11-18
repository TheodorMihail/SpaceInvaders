using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Game
{
    public class AnnouncerController : Controller<AnnouncerScreen, AnnouncerModel, AnnouncerView>
    {
        public AnnouncerController(AnnouncerScreen screen, AnnouncerModel model, AnnouncerView view)
            : base(screen, model, view)
        {
        }

        public override async void Initialize()
        {
            base.Initialize();

            _view.SetDisplayText(_model.DisplayText);
            await _view.PlayAnimation(_model.AnimationDurationSeconds);

            Close();
        }
    }
}
