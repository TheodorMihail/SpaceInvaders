using System;
using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class GameplayHUDController : Controller<GameplayHUD, GameplayHUDModel, GameplayHUDView>
    {
        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly IEnemiesManager _enemiesManager;
        [Inject] private readonly IRepositoryManager _repositoryManager;

        public GameplayHUDController(GameplayHUD hud, GameplayHUDModel model, GameplayHUDView view)
            : base(hud, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _enemiesManager.EnemyDestroyed += OnEnemyDestroyedCallback;
            _messageBus.Subscribe<GameEndedMessage>(OnGameEnded);
            _view.Setup(_model.LevelNumber);
        }

        public override void Dispose()
        {
            base.Dispose();
            _enemiesManager.EnemyDestroyed -= OnEnemyDestroyedCallback;
            _messageBus.Unsubscribe<GameEndedMessage>(OnGameEnded);
        }

        private void OnEnemyDestroyedCallback(string enemyID)
        {
            var enemyType = Enum.Parse<EnemyTypes>(enemyID);
            var enemyConfig = _repositoryManager.GetEnemyConfig(enemyType);
            UpdateScore(enemyConfig.ScoreReward);
        }

        private void UpdateScore(int score)
        {
            score += _model.Score;
            _model.Score = score;
            _view.UpdateScore(score);
        }

        private void OnGameEnded(GameEndedMessage message)
        {
            Close();
        }
    }
}
