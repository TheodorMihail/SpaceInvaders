using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Project;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public enum LevelTypes
    {
        Normal,
        Boss
    }

    [Serializable]
    public struct WaveConfigDTO
    {
        [SerializeField] private List<WaveFormationDTO> _wavesFormation;
        [SerializeField] private float _timeBetweenSpawns;
        [SerializeField] private float _entrySpeed;
        [SerializeField] private List<WaveHazardDTO> _waveHazards;

        public List<WaveFormationDTO> WavesFormation => _wavesFormation ?? new List<WaveFormationDTO>();
        public float TimeBetweenSpawns => _timeBetweenSpawns;
        public float EntrySpeed => _entrySpeed;
        public List<WaveHazardDTO> WaveHazards => _waveHazards ?? new List<WaveHazardDTO>();

        [Serializable]
        public struct WaveFormationDTO
        {
            /// <summary>Formation grid cell, offset into the spawn container on spawn.</summary>
            [FormerlySerializedAs("Position")]
            public Vector2Int GridPosition;
            public EnemyTypes EnemyType;
        }

        /// <summary>One hazard this wave keeps sending in, and how often.</summary>
        [Serializable]
        public struct WaveHazardDTO
        {
            public HazardTypes HazardType;

            [Tooltip("Wait before the first one arrives, so a wave never opens on a hazard.")]
            public float FirstSpawnDelay;

            [Tooltip("Seconds between arrivals, rolled per hazard. A max of 0 sends exactly one.")]
            public float MinSpawnInterval;
            public float MaxSpawnInterval;
        }
    }

    public interface ILevelSessionManager : IInitializable, IDisposable, IGameInitializeListener,
        IGameStartListener, IGameEndListener
    {
        int CurrentLevelNumber { get; }
        int TotalScore { get; }
    }

    /// <summary>
    /// Runs the level's waves in order, starting the next one once the current wave is cleared, and
    /// awards stars on completion. Owns the hazard cadence for the wave that is running.
    /// </summary>
    public partial class LevelSessionManager : ILevelSessionManager
    {
        [Inject] private readonly ILevelsRepository _levelsRepository;
        [Inject] private readonly IShipsRepository _shipsRepository;
        [Inject] private readonly IPlayerManager _playerManager;
        [Inject] private readonly ILevelProgressManager _levelProgressManager;
        [Inject] private readonly IMessageBus _messageBus;

        [Inject] private readonly IEnemiesService _enemiesService;
        [Inject] private readonly IHazardsService _hazardsService;
        [Inject] private readonly IScoreService _scoreService;
        [Inject] private readonly IImpactFeedbackService _impactFeedbackService;

        public int CurrentLevelNumber { get; private set; }
        public int TotalScore => _scoreService.TotalScore;

        private int _currentWaveNumber;
        private LevelConfigSO _currentLevelConfigSo;

        public void Initialize()
        {
            _messageBus.Subscribe<AllEnemiesDestroyedMessage>(OnAllEnemiesDestroyedCallback);
            _levelProgressManager.RegisterSession(this);
            _scoreService.Initialize();
            _impactFeedbackService.Initialize();
        }

        public void Dispose()
        {
            _messageBus.Unsubscribe<AllEnemiesDestroyedMessage>(OnAllEnemiesDestroyedCallback);
            _levelProgressManager.UnregisterSession(this);
            _hazardsService.StopHazards();
            _enemiesService.GameEnd();
            _scoreService.Dispose();
            _impactFeedbackService.Dispose();
        }

        public UniTask GameInitialize()
        {
            _enemiesService.GameInitialize();
            _scoreService.GameInitialize();
            return UniTask.CompletedTask;
        }

        public UniTask GameStart(int levelNumber)
        {
            CurrentLevelNumber = levelNumber;

            if (!_levelsRepository.TryGetLevelConfig(levelNumber, out LevelConfigSO levelConfig))
            {
                return UniTask.CompletedTask;
            }

            StartLevel(levelConfig);
            return UniTask.CompletedTask;
        }

        public UniTask GameEnd()
        {
            _hazardsService.StopHazards();
            _enemiesService.GameEnd();
            _scoreService.GameEnd();
            _impactFeedbackService.GameEnd();
            return UniTask.CompletedTask;
        }

        private void OnAllEnemiesDestroyedCallback(AllEnemiesDestroyedMessage message)
        {
            StartNextWave();
        }

        private void StartLevel(LevelConfigSO levelConfig)
        {
            if (CurrentLevelNumber > _levelProgressManager.MaxLevelNumber)
            {
                this.LogError($"Level {CurrentLevelNumber} is out of range! Max levels: {_levelProgressManager.MaxLevelNumber}");
                return;
            }

            _currentLevelConfigSo = levelConfig;
            _currentWaveNumber = 0;

            _messageBus.Publish(new LevelStartedMessage(CurrentLevelNumber, _currentLevelConfigSo.LevelName));
            StartNextWave();
        }

        private void StartNextWave()
        {
            if (_currentWaveNumber >= _currentLevelConfigSo.WavesConfigs.Count)
            {
                // Nothing left to survive, so the level should not keep throwing hazards during the
                // delay before the run actually ends.
                _hazardsService.StopHazards();
                AwardLevelStars();
                _messageBus.Publish(new LevelCompletedMessage(CurrentLevelNumber));
                return;
            }

            WaveConfigDTO wave = _currentLevelConfigSo.WavesConfigs[_currentWaveNumber];
            _enemiesService.SpawnEnemies(wave).Forget();
            _hazardsService.StartWaveHazards(wave);
            _currentWaveNumber++;

            _messageBus.Publish(new WaveStartedMessage(_currentWaveNumber, _currentLevelConfigSo.WavesConfigs.Count, WaveContainsBoss(wave)));
            this.Log($"Wave {_currentWaveNumber} started!");
        }

        private bool WaveContainsBoss(WaveConfigDTO wave)
        {
            foreach (WaveConfigDTO.WaveFormationDTO formation in wave.WavesFormation)
            {
                if (!_shipsRepository.TryGetEnemyConfig(formation.EnemyType, out var enemyConfig))
                {
                    continue;
                }

                if (enemyConfig.Category == EnemyCategoryTypes.Boss)
                {
                    return true;
                }
            }

            return false;
        }

        private void AwardLevelStars()
        {
            ShipStats stats = _playerManager.PlayerStats;
            int stars = CalculateStars(stats.CumulativeDamageTaken, _currentLevelConfigSo.ThreeStarMaxDamage,
                    _levelsRepository.GetTwoStarDamageMultiplier());

            _levelProgressManager.RecordLevelResult(CurrentLevelNumber, stars);
        }

        /// <summary>Star rating based on total damage taken against the level's three-star threshold.</summary>
        private static int CalculateStars(int damageTaken, int threeStarMaxDamage, float twoStarDamageMultiplier)
        {
            if (damageTaken <= threeStarMaxDamage)
            {
                return 3;
            }

            if (damageTaken <= threeStarMaxDamage * twoStarDamageMultiplier)
            {
                return 2;
            }

            return 1;
        }
    }
}
