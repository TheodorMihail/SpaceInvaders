using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;
using Zenject;

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
            public string EnemyType;
        }
    }

    public interface ILevelManager : IInitializable, IDisposable, IGameStartedListener
    {
        event Action<int> OnLevelCompleted;
    }

    public class LevelManager : ILevelManager
    {
        [Inject] private LevelConfigSO _levelConfig;
        [Inject] private IEnemiesManager _enemiesManager;
        private int _currentWaveIndex = 0;

        public event Action<int> OnLevelCompleted;

        public void Initialize()
        {
            _currentWaveIndex = 0;
            _enemiesManager.OnAllEnemiesDestroyed += OnAllEnemiesDestroyedCallback;
        }

        public void Dispose()
        {
            _enemiesManager.OnAllEnemiesDestroyed -= OnAllEnemiesDestroyedCallback;
        }
        
        public void OnGameStarted()
        {
            StartNextWave();
        }

        private void OnAllEnemiesDestroyedCallback()
        {
            StartNextWave();
        }

        private void StartNextWave()
        {
            if (_currentWaveIndex >= _levelConfig.WavesConfigs.Count)
            {
                OnLevelCompleted?.Invoke(_levelConfig.LevelNumber);
                return;
            }
            
            _enemiesManager.SpawnEnemies(_levelConfig.WavesConfigs[_currentWaveIndex]);
            _currentWaveIndex++;
            this.Log($"Wave {_currentWaveIndex} started!");
        }
    }
}
