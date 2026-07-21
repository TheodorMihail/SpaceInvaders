using BaseArchitecture.Core;
using SpaceInvaders.Project;
using System.Collections.Generic;
using Zenject;
using static SpaceInvaders.Scenes.Game.GameOverState;
using static SpaceInvaders.Scenes.Game.GameStateMachine;

namespace SpaceInvaders.Scenes.Game
{
    public class GameStateMachine : BaseStateMachine<GameStateIds>
    {
        public enum GameStateIds
        {
            Playing,
            GameOver
        }

        protected override GameStateIds DefaultStateId => GameStateIds.Playing;

        [Inject] private readonly IScenesManager _scenesManager;

        private int _currentLevelNumber;

        public GameStateMachine(IList<IState<GameStateIds>> gameStates) : base(gameStates)
        {
        }
        
        public override void Initialize()
        {
            _scenesManager.PendingSceneParams.TryGetParam(out _currentLevelNumber, 1);
            SetState(DefaultStateId, _currentLevelNumber);
        }

        protected override void OnStateFinished((GameStateIds stateId, object[] paramsList) finishedState)
        {
            try
            {
                switch (finishedState.stateId)
                {
                    case GameStateIds.Playing:
                        GameplayStateResult result = (GameplayStateResult)finishedState.paramsList[0];
                        SetState(GameStateIds.GameOver, result);
                        break;

                    case GameStateIds.GameOver:
                        GameOverStateResult gameOverResult = (GameOverStateResult)finishedState.paramsList[0];
                        switch (gameOverResult)
                        {
                            case GameOverStateResult.MainMenu:
                                _scenesManager.LoadScene(SceneType.MainMenu.ToString());
                                break;
                            case GameOverStateResult.Restart:
                                _scenesManager.LoadScene(SceneType.Game.ToString(), _currentLevelNumber);
                                break;
                            case GameOverStateResult.NextLevel:
                                _currentLevelNumber++;
                                SetState(GameStateIds.Playing, _currentLevelNumber);
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