using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Game.GameplayHUD;

namespace SpaceInvaders.Scenes.Game
{
    public class GameplayHUDModel : Model, IModelWithParams<GameplayHUDParams>
    {
        public int Score { get; set; } = 0;
        public int LevelNumber { get; set; } = 0;
        public float CritIndicatorDuration { get; set; } = 0.75f;

        public void InitializeWithParameters(GameplayHUDParams parameters)
        {
            LevelNumber = parameters.LevelNumber;
        }
    }
}
