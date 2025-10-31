using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;
using static SpaceInvaders.Scenes.Game.LevelFinishedScreen;
using static SpaceInvaders.Scenes.Game.GameOverScreen;
using static SpaceInvaders.Scenes.Game.GameplayState;
using static SpaceInvaders.Scenes.Game.GameStateMachine;

namespace SpaceInvaders.Scenes.Game
{
    public class GameOverState : BaseState<GameStateIds>
    {
        public override GameStateIds Id => GameStateIds.GameOver;

        [Inject] private readonly IUIManager _uiManager;
        [Inject] private readonly IScenesManager _sceneManager;

        public override void OnEnter(params object[] paramsList)
        {
            base.OnEnter();

            GameplayStateResult result = (GameplayStateResult)paramsList[0];
            ShowGameOver(result);
        }

        private async void ShowGameOver(GameplayStateResult result)
        {
            switch (result)
            {
                case GameplayStateResult.GameOver:
                    GameOverScreenResult gameOverResult = await _uiManager.ShowScreen<GameOverScreen, GameOverScreenResult>();
                    switch(gameOverResult.Result)
                    {
                        case GameOverScreen.ResultType.MainMenu:
                            _sceneManager.LoadScene(SceneType.MainMenu.ToString());
                            break;
                        case GameOverScreen.ResultType.Restart:
                            _sceneManager.LoadScene(SceneType.Game.ToString());
                            break;
                    }
                    break;

                case GameplayStateResult.LevelFinished:
                    LevelFinishedScreenResult gameFinishedResult = await _uiManager.ShowScreen<LevelFinishedScreen, LevelFinishedScreenResult>();
                    switch(gameFinishedResult.Result)
                    {
                        case LevelFinishedScreen.ResultType.MainMenu:
                            _sceneManager.LoadScene(SceneType.MainMenu.ToString());
                            break;
                        case LevelFinishedScreen.ResultType.NextLevel:
                            break;
                    }
                    break;
            }
        }
    }
}