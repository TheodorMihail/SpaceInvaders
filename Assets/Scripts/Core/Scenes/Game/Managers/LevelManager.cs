using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using static SpaceInvaders.Scenes.Game.AnnouncerScreen;

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

        public List<WaveFormationDTO> WavesFormation => _wavesFormation;
        public float TimeBetweenSpawns => _timeBetweenSpawns;
        public float EntrySpeed => _entrySpeed;

        [Serializable]
        public struct WaveFormationDTO
        {
            public Vector2Int Position;
            public EnemyTypes EnemyType;
        }
    }

    public interface ILevelManager : IInitializable, IDisposable, IGameStartListener
    {
        public int CurrentLevelNumber { get; }
        public int MaxLevelNumber { get; }

        public int CurrentWaveNumber { get; }
        public int MaxWaveNumber { get; }
        
        event Action<int> OnLevelCompleted;
    }

    public class LevelManager : ILevelManager
    {
        [Inject] private IRepositoryManager _repositoryManager;
        [Inject] private IEnemiesManager _enemiesManager;
        [Inject] private IUIManager _uiManager;

        public int CurrentLevelNumber { get; private set; }
        public int MaxLevelNumber { get; private set; }
        
        public int CurrentWaveNumber { get; private set; }
        public int MaxWaveNumber { get; private set; }

        private LevelConfigSO _currentLevelConfigSo;
        private string _normalWaveString(int waveNumber) => $"Wave {CurrentWaveNumber}";
        private string _bossWaveString() => "BOSS WARNING!";

        public event Action<int> OnLevelCompleted;

        public void Initialize()
        {
            MaxLevelNumber = _repositoryManager.GetLevelsCount();
            _enemiesManager.OnAllEnemiesDestroyed += OnAllEnemiesDestroyedCallback;
        }

        public void Dispose()
        {
            _enemiesManager.OnAllEnemiesDestroyed -= OnAllEnemiesDestroyedCallback;
        }

        public UniTask GameStart(int levelNumber)
        {
            CurrentLevelNumber = levelNumber;
            LevelConfigSO levelConfig = GetLevelConfig(levelNumber);
            return StartLevel(levelConfig);
        }

        private void OnAllEnemiesDestroyedCallback()
        {
            StartNextWave();
        }
        
        private async UniTask StartLevel(LevelConfigSO levelConfig)
        {
            if (CurrentLevelNumber > MaxLevelNumber)
            {
                this.LogError($"Level {CurrentLevelNumber} is out of range! Max levels: {MaxLevelNumber}");
                return;
            }
    
            _currentLevelConfigSo = levelConfig;
            CurrentWaveNumber = 0;
            MaxWaveNumber = _currentLevelConfigSo.WavesConfigs.Count;
            
            await _uiManager.ShowScreen<AnnouncerScreen, AnnouncerScreenParams>(
                new AnnouncerScreenParams() { DisplayText = _currentLevelConfigSo.LevelName });

            StartNextWave();
        }

        private void StartNextWave()
        {
            if (CurrentWaveNumber >= _currentLevelConfigSo.WavesConfigs.Count)
            {
                OnLevelCompleted?.Invoke(CurrentLevelNumber);
                return;
            }

            WaveConfigDTO wave = _currentLevelConfigSo.WavesConfigs[CurrentWaveNumber];
            _enemiesManager.SpawnEnemies(wave).Forget();
            CurrentWaveNumber++;

            ShowWaveAnnouncerScreen(wave, CurrentWaveNumber);
            this.Log($"Wave {CurrentWaveNumber} started!");
        }

        private void ShowWaveAnnouncerScreen(WaveConfigDTO wave, int waveNumber)
        {
            string announcementText = WaveContainsBoss(wave) ? _bossWaveString() : _normalWaveString(waveNumber);

            _uiManager.ShowScreen<AnnouncerScreen, AnnouncerScreenParams>(
                new AnnouncerScreenParams() { DisplayText = announcementText });
        }

        private bool WaveContainsBoss(WaveConfigDTO wave)
        {
            foreach (WaveConfigDTO.WaveFormationDTO formation in wave.WavesFormation)
            {
                if (_repositoryManager.GetEnemyConfig(formation.EnemyType).Category == EnemyCategory.Boss)
                {
                    return true;
                }
            }

            return false;
        }

        private LevelConfigSO GetLevelConfig(int levelNumber)
        {
            return _repositoryManager.GetLevelConfig(levelNumber);
        }
    }
}
