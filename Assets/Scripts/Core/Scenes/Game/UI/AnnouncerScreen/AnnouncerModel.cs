using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Game.AnnouncerScreen;

namespace SpaceInvaders.Scenes.Game
{
    public class AnnouncerModel : Model, IModelWithParams<AnnouncerScreenParams>
    {
        public float AnimationDurationSeconds { get; } = 1f;

        public string DisplayText { get; private set; }

        public void InitializeWithParameters(AnnouncerScreenParams parameters)
        {
            DisplayText = parameters.DisplayText;
        }
    }
}
