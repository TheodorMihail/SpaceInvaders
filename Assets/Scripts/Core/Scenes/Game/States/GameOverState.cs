using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using Zenject;
using static SpaceInvaders.Scenes.Game.LevelFinishedScreen;
using static SpaceInvaders.Scenes.Game.GameOverScreen;
using static SpaceInvaders.Scenes.Game.GameplayState;
using static SpaceInvaders.Scenes.Game.GameStateMachine;

namespace SpaceInvaders.Scenes.Game
{
    public class GameOverState : BaseState<GameStateIds>
    {
        public enum GameOverStateResult
        {
            Restart,
            MainMenu,
            NextLevel
        }

        public override GameStateIds Id => GameStateIds.GameOver;

        [Inject] private readonly IUIManager _uiManager;

        public override void OnEnter(params object[] paramsList)
        {
            base.OnEnter();

            GameplayStateResult result = (GameplayStateResult)paramsList[0];
            ShowGameOver(result).Forget();
        }

        private async UniTask ShowGameOver(GameplayStateResult result)
        {
            switch (result)
            {
                case GameplayStateResult.GameOver:
                    GameOverScreenResult gameOverResult = await _uiManager.ShowScreen<GameOverScreen, GameOverScreenResult>();
                    switch (gameOverResult.Result)
                    {
                        case GameOverScreen.ResultType.MainMenu:
                            FinishState(GameOverStateResult.MainMenu);
                            break;
                        case GameOverScreen.ResultType.Restart:
                            FinishState(GameOverStateResult.Restart);
                            break;
                    }
                    break;

                case GameplayStateResult.LevelFinished:
                    LevelFinishedScreenResult gameFinishedResult = await _uiManager.ShowScreen<LevelFinishedScreen, LevelFinishedScreenResult>();
                    switch (gameFinishedResult.Result)
                    {
                        case LevelFinishedScreen.ResultType.MainMenu:
                            FinishState(GameOverStateResult.MainMenu);
                            break;
                        case LevelFinishedScreen.ResultType.NextLevel:
                            FinishState(GameOverStateResult.NextLevel);
                            break;
                    }
                    break;
            }
            
            
        }
    }
}