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

        [Inject] private readonly IScenesManager _scenesManager;

        protected override GameStateTypes DefaultStateId => GameStateTypes.Playing;

        private GameSessionDTO _currentSession;

        public GameStateMachine(IList<IState<GameStateTypes>> gameStates) : base(gameStates)
        {
        }

        /// <summary>The launching scene decides the session, so the mode is never assumed here.</summary>
        public override void Initialize()
        {
            _scenesManager.PendingSceneParams.TryGetParam(out _currentSession, new GameSessionDTO(GameModeTypes.Campaign, 1));
            SetState(DefaultStateId, _currentSession);
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
                                _scenesManager.LoadScene(SceneTypes.Game.ToString(), _currentSession);
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
                                _scenesManager.LoadScene(SceneTypes.Game.ToString(), _currentSession);
                                break;
                            // Re-enters gameplay without reloading the scene, so nothing is disposed
                            // or re-initialized. Per-run state must be reset on game end.
                            case GameOverStateResultTypes.NextLevel:
                                _currentSession = new GameSessionDTO(_currentSession.Mode, _currentSession.LevelNumber + 1);
                                SetState(GameStateTypes.Playing, _currentSession);
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