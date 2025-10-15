using BaseArchitecture.Core;
using System.Collections.Generic;
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

        private IScenesManager _scenesManager;
        private IErrorManager _errorManager;

        public GameStateMachine(IList<IState<GameStateIds>> gameStates,
            IScenesManager scenesManager, IErrorManager errorManager) : base(gameStates)
        {
            _scenesManager = scenesManager;
            _errorManager = errorManager;
        }

        
        protected override void OnStateFinished((GameStateIds stateId, object[] paramsList) finishedState)
        {
            try
            {
                switch (finishedState.stateId)
                {
                    case GameStateIds.Playing:
                        break;
                    case GameStateIds.GameOver:
                        break;
                }
            }
            catch (System.Exception ex)
            {
                _errorManager.ShowErrorDialog($"Failed to transition from {finishedState.stateId}. Please restart the game.");
                _errorManager.LogError<GameStateMachine>($"State transition failed from {finishedState.stateId}", ex);
            }
        }
    }
}