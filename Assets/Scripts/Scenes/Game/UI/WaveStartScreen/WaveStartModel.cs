using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Game.WaveStartScreen;

namespace SpaceInvaders.Scenes.Game
{
    public class WaveStartModel : Model, IModelWithParams<WaveStartScreenParams>
    {
        public float AnimationDurationSeconds { get; } = 1f;

        public int WaveNumber { get; set; }

        public void InitializeWithParameters(WaveStartScreenParams parameters)
        {
            WaveNumber = parameters.WaveNumber;
        }
    }
}
