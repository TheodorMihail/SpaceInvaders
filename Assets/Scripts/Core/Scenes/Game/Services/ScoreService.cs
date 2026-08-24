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
        void GameEnd();
    }

    /// <summary>Accumulates score from destroyed enemies and converts it to currency on game end.</summary>
    public class ScoreService : IScoreService
    {
        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly IShipsRepository _shipsRepository;
        [Inject] private readonly ICurrencyManager _currencyManager;

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

        public void GameEnd()
        {
            _currencyManager.AddCurrency(TotalScore);
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
