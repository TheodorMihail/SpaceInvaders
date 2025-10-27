using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Game.WaveStartScreen;

namespace SpaceInvaders.Scenes.Game
{
    public class WaveStartScreen : ScreenWithParams<WaveStartModel, WaveStartView, WaveStartController, WaveStartScreenParams>
    {
        public struct WaveStartScreenParams : IScreenParam
        {
            public int WaveNumber { get; set; }
        }
    }
}
