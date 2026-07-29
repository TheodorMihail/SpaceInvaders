using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Project;
using UnityEngine;
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

        public List<WaveFormationDTO> WavesFormation => _wavesFormation ?? new List<WaveFormationDTO>();
        public float TimeBetweenSpawns => _timeBetweenSpawns;
        public float EntrySpeed => _entrySpeed;

        [Serializable]
        public struct WaveFormationDTO
        {
            public Vector2Int Position;
            public EnemyTypes EnemyType;
        }
    }

    public interface ILevelSessionService : IInitializable, IDisposable, IGameStartListener
    {
        public int CurrentLevelNumber { get; }
    }

    public class LevelSessionService : ILevelSessionService
    {
        [Inject] private readonly ILevelsRepository _levelsRepository;
        [Inject] private readonly IShipsRepository _shipsRepository;
        [Inject] private readonly IEnemiesManager _enemiesManager;
        [Inject] private readonly IPlayerManager _playerManager;
        [Inject] private readonly ILevelManager _levelManager;
        [Inject] private readonly IMessageBus _messageBus;

        public int CurrentLevelNumber { get; private set; }

        private int _currentWaveNumber;
        private LevelConfigSO _currentLevelConfigSo;

        public void Initialize()
        {
            _messageBus.Subscribe<AllEnemiesDestroyedMessage>(OnAllEnemiesDestroyedCallback);
            _levelManager.RegisterSession(this);
        }

        public void Dispose()
        {
            _messageBus.Unsubscribe<AllEnemiesDestroyedMessage>(OnAllEnemiesDestroyedCallback);
            _levelManager.UnregisterSession(this);
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

        private void OnAllEnemiesDestroyedCallback(AllEnemiesDestroyedMessage message)
        {
            StartNextWave();
        }

        private void StartLevel(LevelConfigSO levelConfig)
        {
            if (CurrentLevelNumber > _levelManager.MaxLevelNumber)
            {
                this.LogError($"Level {CurrentLevelNumber} is out of range! Max levels: {_levelManager.MaxLevelNumber}");
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
                AwardLevelStars();
                _messageBus.Publish(new LevelCompletedMessage(CurrentLevelNumber));
                return;
            }

            WaveConfigDTO wave = _currentLevelConfigSo.WavesConfigs[_currentWaveNumber];
            _enemiesManager.SpawnEnemies(wave).Forget();
            _currentWaveNumber++;

            _messageBus.Publish(new WaveStartedMessage(_currentWaveNumber, WaveContainsBoss(wave)));
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

            _levelManager.RecordLevelResult(CurrentLevelNumber, stars);
        }

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
