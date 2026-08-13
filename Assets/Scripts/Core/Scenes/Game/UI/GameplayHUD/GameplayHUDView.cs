using System;
using System.Collections.Generic;
using System.Threading;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
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

        [SerializeField] private AmmoUIComponent _ammoUI;

        [SerializeField] private LootCounterUIComponent _lootCounterPrefab;
        [SerializeField] private Transform _lootCountersContainer;

        [SerializeField] private HealthBarUIComponent _bossHealthBar;
        [SerializeField] private PowerupIndicatorUIComponent _powerupIndicatorPrefab;
        [SerializeField] private Transform _powerupIndicatorsContainer;

        [SerializeField] private CritIndicatorUIComponent _critIndicatorPrefab;
        [SerializeField] private Transform _critIndicatorsContainer;

        [SerializeField] private Button _pauseButton;

        private CancellationTokenSource _scoreCancellationTokenSource;
        private readonly Dictionary<PowerupTypes, PowerupIndicatorUIComponent> _activePowerupIndicators = new();
        private readonly Dictionary<ItemRarityTypes, LootCounterUIComponent> _activeLootCounters = new();
        private int _currentScore = 0;

        public event Action OnPauseButtonClicked;

        private void Awake()
        {
            _pauseButton.onClick.AddListener(() => OnPauseButtonClicked?.Invoke());
        }

        private void OnDestroy()
        {
            _scoreCancellationTokenSource?.CancelAndDispose();
        }

        public void Setup(int levelNumber)
        {
            _levelText.text = string.Format(_levelString, levelNumber);
            FormatScore(0);
        }

        public void ShowAmmo(bool show)
        {
            if (_ammoUI == null)
            {
                return;
            }

            _ammoUI.Show(show);
        }

        public void UpdateAmmo(int currentAmmo, int maxAmmo)
        {
            if (_ammoUI == null)
            {
                return;
            }

            _ammoUI.UpdateAmmo(currentAmmo, maxAmmo);
        }

        public void ShowReloading(float duration)
        {
            if (_ammoUI == null)
            {
                return;
            }

            _ammoUI.ShowReloading(duration);
        }

        /// <summary>The counter is created on the first pickup of its rarity, so tiers the run has
        /// not dropped yet never take up space.</summary>
        public void UpdateLootCounter(ItemRarityTypes rarity, Sprite icon, int count)
        {
            if (_lootCounterPrefab == null)
            {
                return;
            }

            if (_activeLootCounters.TryGetValue(rarity, out LootCounterUIComponent counter))
            {
                counter.SetCount(count);
                return;
            }

            counter = _objectPooling.Get(_lootCounterPrefab, _lootCountersContainer);
            _activeLootCounters[rarity] = counter;
            counter.Initialize(icon, count);
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

        private void OnCritIndicatorFinished(CritIndicatorUIComponent indicator)
        {
            indicator.OnFinished -= OnCritIndicatorFinished;
            _objectPooling.Return(indicator);
        }
    }
}
