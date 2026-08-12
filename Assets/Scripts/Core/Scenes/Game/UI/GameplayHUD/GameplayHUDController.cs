using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class GameplayHUDController : Controller<GameplayHUD, GameplayHUDModel, GameplayHUDView>
    {
        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly IPowerupsRepository _powerupsRepository;
        [Inject] private readonly IPauseService _pauseService;
        [Inject] private readonly ICameraManager _cameraManager;

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
            _messageBus.Subscribe<ShipDamagedMessage>(OnShipDamagedCallback);

            _view.OnPauseButtonClicked += OnPauseButtonClicked;

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
            _messageBus.Unsubscribe<ShipDamagedMessage>(OnShipDamagedCallback);

            _view.OnPauseButtonClicked -= OnPauseButtonClicked;
        }

        private void OnPauseButtonClicked()
        {
            _pauseService.Pause();
        }

        private void OnEnemyDestroyedCallback(EnemyDestroyedMessage message)
        {
            if (message.Category == EnemyCategoryTypes.Boss)
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
            if (!_powerupsRepository.TryGetPowerupConfig(message.Type, out var config))
            {
                return;
            }

            if (message.Duration > 0)
            {
                _view.ShowPowerupActivated(message.Type, config.Icon, message.Duration);
            }
        }

        private void OnPowerupExpiredCallback(PowerupExpiredMessage message)
        {
            _view.HidePowerupIndicator(message.Type);
        }

        private void OnShipDamagedCallback(ShipDamagedMessage message)
        {
            if (!message.IsCritical)
            {
                return;
            }

            _view.ShowCritIndicator(
                _cameraManager.GetScreenPoint(message.WorldPosition),
                _model.CritIndicatorDuration);
        }

        private void OnGameEnded(GameEndedMessage message)
        {
            Close();
        }
    }
}
