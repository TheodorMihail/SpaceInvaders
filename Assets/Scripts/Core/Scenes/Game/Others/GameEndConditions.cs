using System;
using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>Message adapters that signal the end of the game.</summary>
    public class LevelCompletedCondition : IGameEndCondition
    {
        [Inject] private readonly IMessageBus _messageBus;

        public event Action<GameplayStateResultTypes> ConditionMet;

        public void GameStart(GameSessionDTO session)
        {
            _messageBus.Subscribe<LevelCompletedMessage>(OnLevelCompleted);
        }

        public void GameEnd()
        {
            _messageBus.Unsubscribe<LevelCompletedMessage>(OnLevelCompleted);
        }

        private void OnLevelCompleted(LevelCompletedMessage message)
        {
            ConditionMet?.Invoke(GameplayStateResultTypes.LevelFinished);
        }
    }

    public class PlayerDestroyedCondition : IGameEndCondition
    {
        [Inject] private readonly IMessageBus _messageBus;

        public event Action<GameplayStateResultTypes> ConditionMet;

        public void GameStart(GameSessionDTO session)
        {
            _messageBus.Subscribe<PlayerDestroyedMessage>(OnPlayerDestroyed);
        }

        public void GameEnd()
        {
            _messageBus.Unsubscribe<PlayerDestroyedMessage>(OnPlayerDestroyed);
        }

        private void OnPlayerDestroyed(PlayerDestroyedMessage message)
        {
            ConditionMet?.Invoke(GameplayStateResultTypes.GameOver);
        }
    }
}
