using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Game
{
    public class GameStartModel : Model
    {
        public int CountdownSeconds { get; private set; } = 3;
        public int CountdownEndDelayTimerSeconds { get; private set; } = 1;
    }
}
