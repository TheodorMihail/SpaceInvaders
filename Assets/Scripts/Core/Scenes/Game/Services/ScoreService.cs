using System;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface IScoreService : IInitializable, IDisposable, IGameEndListener
    {
        int TotalScore { get; }
    }

    public class ScoreService : IScoreService
    {
        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly IRepositoryManager _repositoryManager;
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

        public UniTask GameEnd()
        {
            _currencyManager.AddCurrency(TotalScore);
            return UniTask.CompletedTask;
        }

        private void OnEnemyDestroyedCallback(EnemyDestroyedMessage message)
        {
            int reward = _repositoryManager.GetEnemyConfig(message.Type).ScoreReward;
            TotalScore += reward;
            _messageBus.Publish(new ScoreChangedMessage(TotalScore, reward));
        }
    }
}
