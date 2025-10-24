using BaseArchitecture.Core;
using System.Collections.Generic;
using Zenject;
using static SpaceInvaders.Scenes.Game.GameplayState;
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

        [Inject] private IScenesManager _scenesManager;
        [Inject] private IErrorManager _errorManager;

        public GameStateMachine(IList<IState<GameStateIds>> gameStates) : base(gameStates)
        {
        }

        
        protected override void OnStateFinished((GameStateIds stateId, object[] paramsList) finishedState)
        {
            try
            {
                switch (finishedState.stateId)
                {
                    case GameStateIds.Playing:
                        FinishStateResult result = (FinishStateResult)finishedState.paramsList[0];
                        SetState(GameStateIds.GameOver, result);
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