using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Game.AnnouncerScreen;

namespace SpaceInvaders.Scenes.Game
{
    public class AnnouncerScreen : ScreenWithParams<AnnouncerModel, AnnouncerView, AnnouncerController, AnnouncerScreenParams>
    {
        public struct AnnouncerScreenParams : IScreenParam
        {
            public string DisplayText { get; set; }
        }
    }
}
