using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using Zenject;
using static SpaceInvaders.Scenes.Game.VictoryScreen;
using static SpaceInvaders.Scenes.Game.GameOverScreen;
using static SpaceInvaders.Scenes.Game.GameplayState;
using static SpaceInvaders.Scenes.Game.GameStateMachine;

namespace SpaceInvaders.Scenes.Game
{
    public class GameOverState : BaseState<GameStateTypes>
    {
        public enum GameOverStateResultTypes
        {
            Restart,
            MainMenu,
            NextLevel
        }

        [Inject] private readonly IUIManager _uiManager;

        public override GameStateTypes Id => GameStateTypes.GameOver;

        public override void OnEnter(params object[] paramsList)
        {
            base.OnEnter();

            GameplayStateResultTypes result = (GameplayStateResultTypes)paramsList[0];
            ShowGameOver(result).Forget();
        }

        private async UniTask ShowGameOver(GameplayStateResultTypes result)
        {
            switch (result)
            {
                case GameplayStateResultTypes.GameOver:
                    GameOverScreenResult gameOverResult = await _uiManager.ShowScreen<GameOverScreen, GameOverScreenResult>();
                    switch (gameOverResult.Result)
                    {
                        case GameOverScreen.ResultTypes.MainMenu:
                            FinishState(GameOverStateResultTypes.MainMenu);
                            break;
                        case GameOverScreen.ResultTypes.Restart:
                            FinishState(GameOverStateResultTypes.Restart);
                            break;
                    }
                    break;

                case GameplayStateResultTypes.LevelFinished:
                    VictoryScreenResult victoryResult = await _uiManager.ShowScreen<VictoryScreen, VictoryScreenResult>();
                    switch (victoryResult.Result)
                    {
                        case VictoryScreen.ResultTypes.MainMenu:
                            FinishState(GameOverStateResultTypes.MainMenu);
                            break;
                        case VictoryScreen.ResultTypes.NextLevel:
                            FinishState(GameOverStateResultTypes.NextLevel);
                            break;
                        // Replaying a cleared level reloads the scene, exactly as it does after a loss.
                        case VictoryScreen.ResultTypes.Retry:
                            FinishState(GameOverStateResultTypes.Restart);
                            break;
                    }
                    break;
            }
            
            
        }
    }
}