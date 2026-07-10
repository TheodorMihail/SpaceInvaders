using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Game
{
    public class AnnouncerScreen : Screen<AnnouncerModel, AnnouncerView, AnnouncerController>
    {
        public struct AnnouncerScreenParams
        {
            public string DisplayText { get; set; }
        }
    }
}
