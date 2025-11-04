using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;
using Zenject;
using static SpaceInvaders.Scenes.Game.AnnouncerScreen;

namespace SpaceInvaders.Scenes.Game
{
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

    public interface ILevelManager : IInitializable, IDisposable, IGameStartedListener
    {
        public int CurrentLevelNumber { get; }
        public int MaxLevelNumber { get; }

        public int CurrentWaveNumber { get; }
        public int MaxWaveNumber { get; }
        

        event Action<int> OnLevelCompleted;
    }

    public class LevelManager : ILevelManager
    {
        [Inject] private IEnemiesManager _enemiesManager;
        [Inject] private IUIManager _uiManager;
        [Inject] private readonly List<LevelConfigSO> _levelsConfigsSO;
        
        public int CurrentLevelNumber { get; private set; }
        public int MaxLevelNumber { get; private set; }
        public int CurrentWaveNumber { get; private set; }
        public int MaxWaveNumber { get; private set; }

        private LevelConfigSO _currentLevelConfigSo;

        public event Action<int> OnLevelCompleted;

        public void Initialize()
        {
            CurrentLevelNumber = 0;
            MaxLevelNumber = _levelsConfigsSO.Count;
            _enemiesManager.OnAllEnemiesDestroyed += OnAllEnemiesDestroyedCallback;
        }

        public void Dispose()
        {
            _enemiesManager.OnAllEnemiesDestroyed -= OnAllEnemiesDestroyedCallback;
        }
        
        public void OnGameStarted()
        {
            StartLevel(_levelsConfigsSO[CurrentLevelNumber]);
        }

        private void OnAllEnemiesDestroyedCallback()
        {
            StartNextWave();
        }
        
        private async void StartLevel(LevelConfigSO levelConfig)
        {
            if (CurrentLevelNumber >= MaxLevelNumber)
            {
                this.LogError($"Level {CurrentLevelNumber} is out of range! Max levels: {MaxLevelNumber}");
                return;
            }
    
            _currentLevelConfigSo = levelConfig;
            CurrentWaveNumber = 0;
            MaxWaveNumber = _currentLevelConfigSo.WavesConfigs.Count;
            
            CurrentLevelNumber++;
            await _uiManager.ShowScreen<AnnouncerScreen, AnnouncerScreenParams>(new AnnouncerScreenParams() { DisplayText = _currentLevelConfigSo.LevelName });

            StartNextWave();
        }

        private void StartNextWave()
        {
            if (CurrentWaveNumber >= _currentLevelConfigSo.WavesConfigs.Count)
            {
                OnLevelCompleted?.Invoke(CurrentLevelNumber);
                return;
            }
           
            _enemiesManager.SpawnEnemies(_currentLevelConfigSo.WavesConfigs[CurrentWaveNumber]);
            CurrentWaveNumber++;

            _uiManager.ShowScreen<AnnouncerScreen, AnnouncerScreenParams>(new AnnouncerScreenParams() { DisplayText = $"Wave {CurrentWaveNumber}" });
            this.Log($"Wave {CurrentWaveNumber} started!");
        }
    }
}
