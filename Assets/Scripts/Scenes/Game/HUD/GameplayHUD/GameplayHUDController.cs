using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Game
{
    public class GameplayHUDController : Controller<GameplayHUD, GameplayHUDModel, GameplayHUDView>
    {
        public GameplayHUDController(GameplayHUD hud, GameplayHUDModel model, GameplayHUDView view)
            : base(hud, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            RefreshUI();
        }

        public void UpdateLives(int lives)
        {
            _model.Lives = lives;
            _view.UpdateLives(lives);
        }

        public void UpdateScore(int score)
        {
            _model.Score = score;
            _view.UpdateScore(score);
        }

        private void RefreshUI()
        {
            _view.UpdateLives(_model.Lives);
            _view.UpdateScore(_model.Score);
        }
    }
}
