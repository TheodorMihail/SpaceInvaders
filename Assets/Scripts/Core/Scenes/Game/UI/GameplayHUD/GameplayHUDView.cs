using System;
using System.Collections.Generic;
using System.Threading;
using BaseArchitecture.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
        [SerializeField] private string _critIndicatorString = "!";

        [SerializeField] private GameObject _ammoContainer;
        [SerializeField] private Image _ammoIcon;
        [SerializeField] private TextMeshProUGUI _ammoText;
        [SerializeField] private Sprite _ammoSprite;
        [SerializeField] private Sprite _reloadingSprite;
        [SerializeField] private string _ammoString = "{0}/{1}";

        [SerializeField] private HealthBarUIComponent _bossHealthBar;
        [SerializeField] private PowerupIndicatorUIComponent _powerupIndicatorPrefab;
        [SerializeField] private Transform _powerupIndicatorsContainer;

        [SerializeField] private CritIndicatorUIComponent _critIndicatorPrefab;
        [SerializeField] private Transform _critIndicatorsContainer;

        [SerializeField] private Button _pauseButton;

        private CancellationTokenSource _scoreCancellationTokenSource;
        private CancellationTokenSource _reloadCancellationTokenSource;
        private readonly Dictionary<PowerupTypes, PowerupIndicatorUIComponent> _activePowerupIndicators = new();
        private int _currentScore = 0;

        public event Action OnPauseButtonClicked;

        private void Awake()
        {
            _pauseButton.onClick.AddListener(() => OnPauseButtonClicked?.Invoke());
        }

        private void OnDestroy()
        {
            _scoreCancellationTokenSource?.CancelAndDispose();
            _reloadCancellationTokenSource?.CancelAndDispose();
        }

        public void Setup(int levelNumber)
        {
            _levelText.text = string.Format(_levelString, levelNumber);
            FormatScore(0);
        }

        public void ShowAmmo(bool show)
        {
            if (_ammoContainer == null)
            {
                return;
            }

            _ammoContainer.SetActive(show);
        }

        public void UpdateAmmo(int currentAmmo, int maxAmmo)
        {
            _reloadCancellationTokenSource?.CancelAndDispose();
            _reloadCancellationTokenSource = null;

            SetAmmoIcon(_ammoSprite);

            if (_ammoText != null)
            {
                _ammoText.text = string.Format(_ammoString, currentAmmo, maxAmmo);
            }
        }

        /// <summary>Swaps to the reload icon and counts the remaining seconds down in place of the ammo.</summary>
        public async void ShowReloading(float duration)
        {
            _reloadCancellationTokenSource?.CancelAndDispose();
            _reloadCancellationTokenSource = new CancellationTokenSource();

            SetAmmoIcon(_reloadingSprite);

            if (_ammoText == null)
            {
                return;
            }

            await _ammoText.CountdownAsync(duration, 0, duration, null, _reloadCancellationTokenSource);
        }

        public async void UpdateScore(int score)
        {
            _scoreCancellationTokenSource?.CancelAndDispose();
            _scoreCancellationTokenSource = new CancellationTokenSource();

            await _scoreText.CountdownAsync(_currentScore, score, 0.5f, FormatScore, _scoreCancellationTokenSource);
            _currentScore = score;
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

        /// <summary>Position is in screen pixels, which a screen space overlay canvas takes directly.</summary>
        public void ShowCritIndicator(Vector3 screenPosition, float duration)
        {
            if (_critIndicatorPrefab == null)
            {
                return;
            }

            var indicator = _objectPooling.Get(_critIndicatorPrefab, _critIndicatorsContainer);
            indicator.OnFinished += OnCritIndicatorFinished;
            indicator.Initialize(_critIndicatorString, screenPosition, duration);
        }

        private void FormatScore(int score)
        {
            _scoreText.text = string.Format(_scoreString, score);
        }

        /// <summary>Hidden rather than left as a blank box when no sprite is authored.</summary>
        private void SetAmmoIcon(Sprite sprite)
        {
            if (_ammoIcon == null)
            {
                return;
            }

            _ammoIcon.sprite = sprite;
            _ammoIcon.enabled = sprite != null;
        }

        private void OnCritIndicatorFinished(CritIndicatorUIComponent indicator)
        {
            indicator.OnFinished -= OnCritIndicatorFinished;
            _objectPooling.Return(indicator);
        }
    }
}
