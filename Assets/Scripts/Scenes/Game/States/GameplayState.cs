using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.Game.GameStateMachine;

namespace SpaceInvaders.Scenes.Game
{
    public class GameplayState : BaseState<GameStateIds>
    {
        public override GameStateIds Id => GameStateIds.Playing;

        private readonly IUIManager _uiManager;

        public GameplayState(IUIManager uiManager)
        {
            _uiManager = uiManager;
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }
    }
}