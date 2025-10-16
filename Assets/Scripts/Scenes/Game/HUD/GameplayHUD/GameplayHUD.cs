using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Game
{
    public class GameplayHUD : HUD<GameplayHUDModel, GameplayHUDView, GameplayHUDController>
    {
        public void UpdateLives(int lives)
        {
            _controller?.UpdateLives(lives);
        }

        public void UpdateScore(int score)
        {
            _controller?.UpdateScore(score);
        }
    }
}
