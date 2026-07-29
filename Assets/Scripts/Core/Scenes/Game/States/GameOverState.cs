using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using Zenject;
using static SpaceInvaders.Scenes.Game.LevelFinishedScreen;
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

        public override GameStateTypes Id => GameStateTypes.GameOver;

        [Inject] private readonly IUIManager _uiManager;

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
                    LevelFinishedScreenResult gameFinishedResult = await _uiManager.ShowScreen<LevelFinishedScreen, LevelFinishedScreenResult>();
                    switch (gameFinishedResult.Result)
                    {
                        case LevelFinishedScreen.ResultTypes.MainMenu:
                            FinishState(GameOverStateResultTypes.MainMenu);
                            break;
                        case LevelFinishedScreen.ResultTypes.NextLevel:
                            FinishState(GameOverStateResultTypes.NextLevel);
                            break;
                    }
                    break;
            }
            
            
        }
    }
}