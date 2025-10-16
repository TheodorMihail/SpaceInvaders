using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Game
{
    public class GameplayHUDModel : Model
    {
        public int Lives { get; set; } = 3;
        public int Score { get; set; } = 0;
    }
}
