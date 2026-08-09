using BaseArchitecture.Core;
using SpaceInvaders.Project;
using System.Collections.Generic;
using Zenject;
using static SpaceInvaders.Scenes.Game.GameOverState;
using static SpaceInvaders.Scenes.Game.GameStateMachine;

namespace SpaceInvaders.Scenes.Game
{
    public class GameStateMachine : BaseStateMachine<GameStateTypes>
    {
        public enum GameStateTypes
        {
            Playing,
            GameOver
        }

        protected override GameStateTypes DefaultStateId => GameStateTypes.Playing;

        [Inject] private readonly IScenesManager _scenesManager;

        private int _currentLevelNumber;

        public GameStateMachine(IList<IState<GameStateTypes>> gameStates) : base(gameStates)
        {
        }
        
        public override void Initialize()
        {
            _scenesManager.PendingSceneParams.TryGetParam(out _currentLevelNumber, 1);
            SetState(DefaultStateId, _currentLevelNumber);
        }

        protected override void OnStateFinished((GameStateTypes stateId, object[] paramsList) finishedState)
        {
            try
            {
                switch (finishedState.stateId)
                {
                    case GameStateTypes.Playing:
                        GameplayStateResultTypes result = (GameplayStateResultTypes)finishedState.paramsList[0];
                        switch (result)
                        {
                            // Quitting and restarting come from the pause screen, so they skip the
                            // game over flow entirely.
                            case GameplayStateResultTypes.Quit:
                                _scenesManager.LoadScene(SceneTypes.MainMenu.ToString());
                                break;
                            case GameplayStateResultTypes.Restart:
                                _scenesManager.LoadScene(SceneTypes.Game.ToString(), _currentLevelNumber);
                                break;
                            default:
                                SetState(GameStateTypes.GameOver, result);
                                break;
                        }

                        break;

                    case GameStateTypes.GameOver:
                        GameOverStateResultTypes gameOverResult = (GameOverStateResultTypes)finishedState.paramsList[0];
                        switch (gameOverResult)
                        {
                            case GameOverStateResultTypes.MainMenu:
                                _scenesManager.LoadScene(SceneTypes.MainMenu.ToString());
                                break;
                            case GameOverStateResultTypes.Restart:
                                _scenesManager.LoadScene(SceneTypes.Game.ToString(), _currentLevelNumber);
                                break;
                            // Re-enters gameplay without reloading the scene, so nothing is disposed
                            // or re-initialized. Per-run state must be reset on game end.
                            case GameOverStateResultTypes.NextLevel:
                                _currentLevelNumber++;
                                SetState(GameStateTypes.Playing, _currentLevelNumber);
                                break;
                        }
                    
                        break;
                }
            }
            catch (System.Exception ex)
            {
                this.LogError($"State transition failed from {finishedState.stateId}", ex);
            }
        }
    }
}