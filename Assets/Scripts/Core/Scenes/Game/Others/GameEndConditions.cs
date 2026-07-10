using System;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class LevelCompletedCondition : IGameEndCondition, IInitializable, IDisposable
    {
        [Inject] private readonly ILevelManager _levelManager;

        public event Action<GameplayStateResult> ConditionMet;

        public void Initialize()
        {
            _levelManager.OnLevelCompleted += OnLevelCompleted;
        }

        public void Dispose()
        {
            _levelManager.OnLevelCompleted -= OnLevelCompleted;
        }

        private void OnLevelCompleted(int levelNumber)
        {
            ConditionMet?.Invoke(GameplayStateResult.LevelFinished);
        }
    }

    public class PlayerDestroyedCondition : IGameEndCondition, IInitializable, IDisposable
    {
        [Inject] private readonly IPlayerManager _playerManager;

        public event Action<GameplayStateResult> ConditionMet;

        public void Initialize()
        {
            _playerManager.OnPlayerDestroyed += OnPlayerDestroyed;
        }

        public void Dispose()
        {
            _playerManager.OnPlayerDestroyed -= OnPlayerDestroyed;
        }

        private void OnPlayerDestroyed()
        {
            ConditionMet?.Invoke(GameplayStateResult.GameOver);
        }
    }
}
