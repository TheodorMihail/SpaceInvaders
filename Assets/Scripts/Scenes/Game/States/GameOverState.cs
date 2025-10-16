using BaseArchitecture.Core;
using Zenject;
using static SpaceInvaders.Scenes.Game.GameStateMachine;

namespace SpaceInvaders.Scenes.Game
{
    public class GameOverState : BaseState<GameStateIds>
    {
        public override GameStateIds Id => GameStateIds.GameOver;

        [Inject] private readonly IUIManager _uiManager;

        public override void OnEnter()
        {
            base.OnEnter();
        }
    }
}