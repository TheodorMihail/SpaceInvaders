using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class GameplayHUDController : Controller<GameplayHUD, GameplayHUDModel, GameplayHUDView>
    {
        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly IRepositoryManager _repositoryManager;

        public GameplayHUDController(GameplayHUD hud, GameplayHUDModel model, GameplayHUDView view)
            : base(hud, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            _messageBus.Subscribe<EnemyDestroyedMessage>(OnEnemyDestroyedCallback);
            _messageBus.Subscribe<ScoreChangedMessage>(OnScoreChangedCallback);
            _messageBus.Subscribe<BossSpawnedMessage>(OnBossSpawnedCallback);
            _messageBus.Subscribe<BossHealthChangedMessage>(OnBossHealthChangedCallback);
            _messageBus.Subscribe<PowerupActivatedMessage>(OnPowerupActivatedCallback);
            _messageBus.Subscribe<PowerupExpiredMessage>(OnPowerupExpiredCallback);
            _messageBus.Subscribe<GameEndedMessage>(OnGameEnded);

            _view.Setup(_model.LevelNumber);
        }

        public override void Dispose()
        {
            base.Dispose();

            _messageBus.Unsubscribe<EnemyDestroyedMessage>(OnEnemyDestroyedCallback);
            _messageBus.Unsubscribe<ScoreChangedMessage>(OnScoreChangedCallback);
            _messageBus.Unsubscribe<BossSpawnedMessage>(OnBossSpawnedCallback);
            _messageBus.Unsubscribe<BossHealthChangedMessage>(OnBossHealthChangedCallback);
            _messageBus.Unsubscribe<PowerupActivatedMessage>(OnPowerupActivatedCallback);
            _messageBus.Unsubscribe<PowerupExpiredMessage>(OnPowerupExpiredCallback);
            _messageBus.Unsubscribe<GameEndedMessage>(OnGameEnded);
        }

        private void OnEnemyDestroyedCallback(EnemyDestroyedMessage message)
        {
            if (message.Category == EnemyCategory.Boss)
            {
                _view.ShowBossHealthBar(false);
            }
        }

        private void OnScoreChangedCallback(ScoreChangedMessage message)
        {
            _model.Score = message.TotalScore;
            _view.UpdateScore(message.TotalScore);
        }

        private void OnBossSpawnedCallback(BossSpawnedMessage message)
        {
            _view.InitializeBossHealthBar(message.MaxHealth);
        }

        private void OnBossHealthChangedCallback(BossHealthChangedMessage message)
        {
            _view.UpdateBossHealth(message.CurrentHealth);
        }

        private void OnPowerupActivatedCallback(PowerupActivatedMessage message)
        {
            var config = _repositoryManager.GetPowerupConfig(message.Type);

            if (message.Duration > 0)
            {
                _view.ShowPowerupActivated(message.Type, config.Icon, message.Duration);
            }
        }

        private void OnPowerupExpiredCallback(PowerupExpiredMessage message)
        {
            _view.HidePowerupIndicator(message.Type);
        }

        private void OnGameEnded(GameEndedMessage message)
        {
            Close();
        }
    }
}
