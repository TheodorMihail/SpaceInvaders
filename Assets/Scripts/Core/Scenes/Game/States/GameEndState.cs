using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Project;
using Zenject;
using static SpaceInvaders.Scenes.Game.VictoryScreen;
using static SpaceInvaders.Scenes.Game.GameOverScreen;
using static SpaceInvaders.Scenes.Game.GameStateMachine;

namespace SpaceInvaders.Scenes.Game
{
    public class GameEndState : BaseState<GameStateTypes>
    {
        public enum GameEndStateResultTypes
        {
            Restart,

            /// <summary>Leaving the run, which lands in whichever hub the mode owns.</summary>
            ReturnToHub,
            NextLevel
        }

        [Inject] private readonly IUIManager _uiManager;

        public override GameStateTypes Id => GameStateTypes.GameEnd;

        public override void OnEnter(params object[] paramsList)
        {
            base.OnEnter();

            paramsList.TryGetParam(out GameSessionResultDTO sessionResult);
            paramsList.TryGetParam(out GameEndResolutionDTO resolution, default, 1);

            ShowResultScreen(sessionResult, resolution.Options).Forget();
        }

        /// <summary>The mode decides the buttons, and offering none means no screen at all.</summary>
        private async UniTask ShowResultScreen(GameSessionResultDTO sessionResult, GameEndOptionTypes options)
        {
            if (options == GameEndOptionTypes.None)
            {
                FinishState(GameEndStateResultTypes.ReturnToHub);
                return;
            }

            switch (sessionResult.Result)
            {
                case GameplayStateResultTypes.GameOver:
                    GameOverScreenResult gameOverResult = await _uiManager
                        .ShowScreen<GameOverScreen, GameOverScreenParams, GameOverScreenResult>(
                            new GameOverScreenParams { Options = options });

                    switch (gameOverResult.Result)
                    {
                        case GameOverScreen.ResultTypes.MainMenu:
                            FinishState(GameEndStateResultTypes.ReturnToHub);
                            break;
                        case GameOverScreen.ResultTypes.Restart:
                            FinishState(GameEndStateResultTypes.Restart);
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
                            FinishState(GameEndStateResultTypes.ReturnToHub);
                            break;
                        case VictoryScreen.ResultTypes.NextLevel:
                            FinishState(GameEndStateResultTypes.NextLevel);
                            break;
                        // Replaying a cleared level reloads the scene, same as restarting after a loss.
                        case VictoryScreen.ResultTypes.Retry:
                            FinishState(GameEndStateResultTypes.Restart);
                            break;
                    }
                    break;
            }
        }
    }
}
