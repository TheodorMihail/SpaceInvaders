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
    /// Turns facts on the bus into screen shake and hit stop. Nothing in gameplay knows the camera
    /// is being shaken.
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

        /// <summary>The level advance never reloads the scene, so a shake left running would carry into
        /// the next level with the camera still off its mark.</summary>
        public void GameEnd()
        {
            _cameraManager.ResetShake();
        }

        /// <summary>Enemies take hits constantly, so only the player's own damage is worth a shake.</summary>
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

        /// <summary>Ordinary kills are deliberately left alone: waves run large enough that shaking on
        /// every one reads as noise rather than impact.</summary>
        private void OnEnemyDestroyed(EnemyDestroyedMessage message)
        {
            if (message.Category != EnemyCategoryTypes.Boss)
            {
                return;
            }

            _cameraManager.AddScreenShake(_settings.BossDestroyedShake);
        }

        /// <summary>Held until the boss is actually on screen. Spawning happens while it is still above
        /// the view with a long entry ahead of it, so reacting there plays the whole moment to nobody.</summary>
        private void OnBossEntered(BossEnteredMessage message)
        {
            _cameraManager.AddScreenShake(_settings.BossEnteredShake);
            _timeManager.ApplySlowMotion(_settings.BossEnteredSlowMotion, _settings.SlowMotionTimeScale);
        }

        /// <summary>No shake here: the level is won, so the moment is worth dwelling on rather than
        /// hitting.</summary>
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
