using BaseArchitecture.Core;
using Zenject;
using static SpaceInvaders.Scenes.Game.GameStateMachine;

namespace SpaceInvaders.Scenes.Game
{
    public class GameplayState : BaseState<GameStateIds>
    {
        public override GameStateIds Id => GameStateIds.Playing;

        [Inject] private readonly IUIManager _uiManager;
        [Inject] private readonly IGameplayManager _gameplayManager;

        public override void OnEnter()
        {
            base.OnEnter();
            ShowGameStartMenu();
        }

        private async void ShowGameStartMenu()
        {
            _uiManager.ShowHUD<GameplayHUD>();
            await _uiManager.ShowScreen<GameStartScreen>();
            _gameplayManager.StartGame();
        }
    }
}