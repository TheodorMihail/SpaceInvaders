using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Project;
using Zenject;
using static SpaceInvaders.Scenes.Game.VictoryScreen;
using static SpaceInvaders.Scenes.Game.GameOverScreen;
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
        [Inject] private readonly IGameModeManager _gameModeManager;

        public override GameStateTypes Id => GameStateTypes.GameOver;

        public override void OnEnter(params object[] paramsList)
        {
            base.OnEnter();

            var sessionResult = (GameSessionResultDTO)paramsList[0];
            ShowGameOver(sessionResult).Forget();
        }

        /// <summary>Which buttons a result screen offers is the mode's decision.</summary>
        private async UniTask ShowGameOver(GameSessionResultDTO sessionResult)
        {
            GameOverOptionTypes options = _gameModeManager.GetGameOverOptions(sessionResult);

            switch (sessionResult.Result)
            {
                case GameplayStateResultTypes.GameOver:
                    GameOverScreenResult gameOverResult = await _uiManager
                        .ShowScreen<GameOverScreen, GameOverScreenParams, GameOverScreenResult>(
                            new GameOverScreenParams { Options = options });

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
                    VictoryScreenResult victoryResult = await _uiManager
                        .ShowScreen<VictoryScreen, VictoryScreenParams, VictoryScreenResult>(
                            new VictoryScreenParams { Options = options });

                    switch (victoryResult.Result)
                    {
                        case VictoryScreen.ResultTypes.MainMenu:
                            FinishState(GameOverStateResultTypes.MainMenu);
                            break;
                        case VictoryScreen.ResultTypes.NextLevel:
                            FinishState(GameOverStateResultTypes.NextLevel);
                            break;
                        // Replaying a cleared level reloads the scene, same as restarting after a loss.
                        case VictoryScreen.ResultTypes.Retry:
                            FinishState(GameOverStateResultTypes.Restart);
                            break;
                    }
                    break;
            }
        }
    }
}
