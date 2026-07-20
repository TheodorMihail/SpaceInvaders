using System;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class GameplayHUDController : Controller<GameplayHUD, GameplayHUDModel, GameplayHUDView>
    {
        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly IEnemiesManager _enemiesManager;
        [Inject] private readonly IRepositoryManager _repositoryManager;
        [Inject] private readonly IPowerupManager _powerupManager;

        public GameplayHUDController(GameplayHUD hud, GameplayHUDModel model, GameplayHUDView view)
            : base(hud, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            _enemiesManager.EnemyDestroyed += OnEnemyDestroyedCallback;
            _enemiesManager.OnBossHealthChanged += OnBossHealthChangedCallback;
            _powerupManager.PowerupActivated += OnPowerupActivatedCallback;
            _powerupManager.PowerupExpired += OnPowerupExpiredCallback;

            _messageBus.Subscribe<GameEndedMessage>(OnGameEnded);
            _view.Setup(_model.LevelNumber);
        }

        public override void Dispose()
        {
            base.Dispose();

            _enemiesManager.EnemyDestroyed -= OnEnemyDestroyedCallback;
            _enemiesManager.OnBossHealthChanged -= OnBossHealthChangedCallback;
            _powerupManager.PowerupActivated -= OnPowerupActivatedCallback;
            _powerupManager.PowerupExpired -= OnPowerupExpiredCallback;

            _messageBus.Unsubscribe<GameEndedMessage>(OnGameEnded);
        }

        private void OnEnemyDestroyedCallback(string enemyID, Vector3 position)
        {
            var enemyType = Enum.Parse<EnemyTypes>(enemyID);
            var enemyConfig = _repositoryManager.GetEnemyConfig(enemyType);
            UpdateScore(enemyConfig.ScoreReward);

            if (enemyConfig.Category == EnemyCategory.Boss)
            {
                _view.ShowBossHealthBar(false);
            }
        }

        private void UpdateScore(int score)
        {
            score += _model.Score;
            _model.Score = score;
            _view.UpdateScore(score);
        }

        private void OnBossHealthChangedCallback(int currentHealth, int maxHealth)
        {
            _view.UpdateBossHealth(currentHealth, maxHealth);
        }

        private void OnPowerupActivatedCallback(PowerupTypes type, float duration)
        {
            var config = _repositoryManager.GetPowerupConfig(type);
            
            if (duration > 0)
            {
                _view.ShowPowerupActivated(type, config.Icon, duration);
            }
        }

        private void OnPowerupExpiredCallback(PowerupTypes type)
        {
            _view.HidePowerupIndicator(type);
        }

        private void OnGameEnded(GameEndedMessage message)
        {
            Close();
        }
    }
}
