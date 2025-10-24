using BaseArchitecture.Core;
using Zenject;
using static SpaceInvaders.Scenes.Game.GameFinishedScreen;
using static SpaceInvaders.Scenes.Game.GameOverScreen;
using static SpaceInvaders.Scenes.Game.GameplayState;
using static SpaceInvaders.Scenes.Game.GameStateMachine;

namespace SpaceInvaders.Scenes.Game
{
    public class GameOverState : BaseState<GameStateIds>
    {
        public override GameStateIds Id => GameStateIds.GameOver;

        [Inject] private readonly IUIManager _uiManager;

        public override async void OnEnter(params object[] paramsList)
        {
            base.OnEnter();

            FinishStateResult result = (FinishStateResult)paramsList[0];
            ShowGameOver(result);
        }

        private async void ShowGameOver(FinishStateResult result)
        {
            switch (result)
            {
                case FinishStateResult.GameOver:
                    GameOverScreenResult gameOverResult = await _uiManager.ShowScreen<GameOverScreen, GameOverScreenResult>();
                    // TODO: Handle restart or main menu based on gameOverResult
                    break;
                case FinishStateResult.GameFinished:
                    GameFinishedScreenResult gameFinishedResult = await _uiManager.ShowScreen<GameFinishedScreen, GameFinishedScreenResult>();
                    // TODO: Handle next level or main menu based on gameFinishedResult
                    break;
            }
        }
    }
}