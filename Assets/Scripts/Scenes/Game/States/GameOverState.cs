using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Game.GameStateMachine;

namespace SpaceInvaders.Scenes.Game
{
    public class GameOverState : BaseState<GameStateIds>
    {
        public override GameStateIds Id => GameStateIds.GameOver;

        private readonly IUIManager _uiManager;

        public GameOverState(IUIManager uiManager)
        {
            _uiManager = uiManager;
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }
    }
}