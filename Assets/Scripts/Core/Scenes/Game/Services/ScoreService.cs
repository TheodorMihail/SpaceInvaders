using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface IScoreService
    {
        int TotalScore { get; }

        void Initialize();
        void Dispose();
        void GameInitialize();
    }

    /// <summary>Accumulates score from destroyed enemies. Where it goes is decided elsewhere.</summary>
    public class ScoreService : IScoreService
    {
        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly IShipsRepository _shipsRepository;

        public int TotalScore { get; private set; }

        public void Initialize()
        {
            _messageBus.Subscribe<EnemyDestroyedMessage>(OnEnemyDestroyedCallback);
        }

        public void Dispose()
        {
            _messageBus.Unsubscribe<EnemyDestroyedMessage>(OnEnemyDestroyedCallback);
        }

        /// <summary>Reset here, not on game end: the result screens still read the score after that.</summary>
        public void GameInitialize()
        {
            TotalScore = 0;
        }

        private void OnEnemyDestroyedCallback(EnemyDestroyedMessage message)
        {
            if (!_shipsRepository.TryGetEnemyConfig(message.Type, out var enemyConfig))
            {
                return;
            }

            int reward = enemyConfig.ScoreReward;
            TotalScore += reward;
            _messageBus.Publish(new ScoreChangedMessage(TotalScore, reward));
        }
    }
}
