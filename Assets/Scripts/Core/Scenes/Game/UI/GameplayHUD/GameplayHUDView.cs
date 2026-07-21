using System.Collections.Generic;
using System.Threading;
using BaseArchitecture.Core;
using TMPro;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    [AddressablePath("HUD/GameplayHUDView")]
    public class GameplayHUDView : View
    {
        [Inject] private readonly IObjectPooling _objectPooling;

        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _levelText;

        [SerializeField] private string _scoreString = "Score: {0}";
        [SerializeField] private string _levelString = "Level: {0}";

        [SerializeField] private HealthBarUIComponent _bossHealthBar;
        [SerializeField] private PowerupIndicatorComponent _powerupIndicatorPrefab;
        [SerializeField] private Transform _powerupIndicatorsContainer;

        private CancellationTokenSource _scoreCancellationTokenSource;
        private readonly Dictionary<PowerupTypes, PowerupIndicatorComponent> _activePowerupIndicators = new();
        private int _currentScore = 0;

        public void Setup(int levelNumber)
        {
            _levelText.text = string.Format(_levelString, levelNumber);
            FormatScore(0);
        }

        public void InitializeBossHealthBar(int maxHealth)
        {
            _bossHealthBar.Initialize(maxHealth, maxHealth);
            ShowBossHealthBar(true);
        }

        public void UpdateBossHealth(int currentHealth)
        {
            _bossHealthBar.UpdateHealth(currentHealth);
        }

        public void ShowBossHealthBar(bool show)
        {
            _bossHealthBar.gameObject.SetActive(show);
        }

        public async void UpdateScore(int score)
        {
            _scoreCancellationTokenSource?.CancelAndDispose();
            _scoreCancellationTokenSource = new CancellationTokenSource();

            await _scoreText.CountdownAsync(_currentScore, score, 0.5f, FormatScore, _scoreCancellationTokenSource);
            _currentScore = score;
        }

        private void FormatScore(int score)
        {
            _scoreText.text = string.Format(_scoreString, score);
        }

        public void ShowPowerupActivated(PowerupTypes type, Sprite icon, float duration)
        {
            if (!_activePowerupIndicators.TryGetValue(type, out var indicator))
            {
                indicator = _objectPooling.Get(_powerupIndicatorPrefab, _powerupIndicatorsContainer);
                _activePowerupIndicators[type] = indicator;
            }

            indicator.Initialize(icon, duration);
        }

        public void HidePowerupIndicator(PowerupTypes type)
        {
            if (!_activePowerupIndicators.TryGetValue(type, out var indicator))
            {
                return;
            }

            _activePowerupIndicators.Remove(type);
            _objectPooling.Return(indicator);
        }

        private void OnDestroy()
        {
            _scoreCancellationTokenSource?.CancelAndDispose();
        }
    }
}
