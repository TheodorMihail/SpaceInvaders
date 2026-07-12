using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Game
{
    public class GameplayHUD : HUD<GameplayHUDModel, GameplayHUDView, GameplayHUDController>
    {
        public struct GameplayHUDParams
        {
            public int LevelNumber { get; set; }
        }
    }
}
