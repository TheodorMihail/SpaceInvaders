using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface IImpactFeedbackService
    {
        void Initialize();
        void Dispose();
        void GameEnd();
    }

    /// <summary>
    /// Converts gameplay messages into screen shake and hit stop, so gameplay code does not touch the
    /// camera itself.
    /// </summary>
    public class ImpactFeedbackService : IImpactFeedbackService
    {
        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly IGameRepository _gameRepository;
        [Inject] private readonly ICameraManager _cameraManager;
        [Inject] private readonly ITimeManager _timeManager;

        private ImpactFeedbackSettings _settings;

        public void Initialize()
        {
            _settings = _gameRepository.GetGameDataConfig().ImpactFeedback;

            _messageBus.Subscribe<ShipDamagedMessage>(OnShipDamaged);
            _messageBus.Subscribe<PlayerDestroyedMessage>(OnPlayerDestroyed);
            _messageBus.Subscribe<EnemyDestroyedMessage>(OnEnemyDestroyed);
            _messageBus.Subscribe<BossEnteredMessage>(OnBossEntered);
            _messageBus.Subscribe<HazardDestroyedMessage>(OnHazardDestroyed);
            _messageBus.Subscribe<LevelCompletedMessage>(OnLevelCompleted);
        }

        public void Dispose()
        {
            _messageBus.Unsubscribe<ShipDamagedMessage>(OnShipDamaged);
            _messageBus.Unsubscribe<PlayerDestroyedMessage>(OnPlayerDestroyed);
            _messageBus.Unsubscribe<EnemyDestroyedMessage>(OnEnemyDestroyed);
            _messageBus.Unsubscribe<BossEnteredMessage>(OnBossEntered);
            _messageBus.Unsubscribe<HazardDestroyedMessage>(OnHazardDestroyed);
            _messageBus.Unsubscribe<LevelCompletedMessage>(OnLevelCompleted);
        }

        /// <summary>The level advance never reloads the scene, so a running shake would carry into the
        /// next level.</summary>
        public void GameEnd()
        {
            _cameraManager.ResetShake();
        }

        /// <summary>Only player damage shakes: enemies are hit too often for it to read.</summary>
        private void OnShipDamaged(ShipDamagedMessage message)
        {
            if (!message.IsPlayer)
            {
                return;
            }

            _cameraManager.AddScreenShake(_settings.PlayerDamagedShake);
        }

        private void OnPlayerDestroyed(PlayerDestroyedMessage message)
        {
            _cameraManager.AddScreenShake(_settings.PlayerDestroyedShake);
            _timeManager.ApplySlowMotion(_settings.PlayerDestroyedSlowMotion, _settings.SlowMotionTimeScale);
        }

        /// <summary>Ordinary kills do not shake: waves are large enough that it would be constant.</summary>
        private void OnEnemyDestroyed(EnemyDestroyedMessage message)
        {
            if (message.Category != EnemyCategoryTypes.Boss)
            {
                return;
            }

            _cameraManager.AddScreenShake(_settings.BossDestroyedShake);
        }

        /// <summary>Held until the boss is on screen: it spawns above the view with a long entry
        /// ahead of it, so reacting on spawn would play off screen.</summary>
        private void OnBossEntered(BossEnteredMessage message)
        {
            _cameraManager.AddScreenShake(_settings.BossEnteredShake);
            _timeManager.ApplySlowMotion(_settings.BossEnteredSlowMotion, _settings.SlowMotionTimeScale);
        }

        /// <summary>Slow motion only, no shake: the level is won rather than hit.</summary>
        private void OnLevelCompleted(LevelCompletedMessage message)
        {
            _timeManager.ApplySlowMotion(_settings.LevelCompletedSlowMotion, _settings.SlowMotionTimeScale);
        }

        private void OnHazardDestroyed(HazardDestroyedMessage message)
        {
            _cameraManager.AddScreenShake(_settings.HazardDestroyedShake);
        }
    }
}
