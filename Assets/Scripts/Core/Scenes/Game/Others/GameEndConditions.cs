using System;
using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class LevelCompletedCondition : IGameEndCondition, IInitializable, IDisposable
    {
        [Inject] private readonly IMessageBus _messageBus;

        public event Action<GameplayStateResult> ConditionMet;

        public void Initialize()
        {
            _messageBus.Subscribe<LevelCompletedMessage>(OnLevelCompleted);
        }

        public void Dispose()
        {
            _messageBus.Unsubscribe<LevelCompletedMessage>(OnLevelCompleted);
        }

        private void OnLevelCompleted(LevelCompletedMessage message)
        {
            ConditionMet?.Invoke(GameplayStateResult.LevelFinished);
        }
    }

    public class PlayerDestroyedCondition : IGameEndCondition, IInitializable, IDisposable
    {
        [Inject] private readonly IMessageBus _messageBus;

        public event Action<GameplayStateResult> ConditionMet;

        public void Initialize()
        {
            _messageBus.Subscribe<PlayerDestroyedMessage>(OnPlayerDestroyed);
        }

        public void Dispose()
        {
            _messageBus.Unsubscribe<PlayerDestroyedMessage>(OnPlayerDestroyed);
        }

        private void OnPlayerDestroyed(PlayerDestroyedMessage message)
        {
            ConditionMet?.Invoke(GameplayStateResult.GameOver);
        }
    }
}
