using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public enum EnemyTypes
    {
        Enemy1
    }
    
    public interface IEnemiesManager : IDisposable, IGameInitializeListener, IGameEndedListener
    {
        event Action<string> EnemyDestroyed;
        event Action OnAllEnemiesDestroyed;
        public void SpawnEnemies(WaveConfigDTO wave);
    }

    public class EnemiesManager : IEnemiesManager, ITickable
    {
        [Inject] private ISpawnService _spawnService;

        private List<EnemySpaceshipBehaviourComponent> _spawnedEnemies;

        public event Action<string> EnemyDestroyed;
        public event Action OnAllEnemiesDestroyed;


        public void OnGameInitialized()
        {
            _spawnedEnemies = new List<EnemySpaceshipBehaviourComponent>();
        }

        public void OnGameEnded()
        {
            ClearEnemies();
        }

        public void Dispose()
        {
            ClearEnemies();
        }
        
        public async void SpawnEnemies(WaveConfigDTO waveConfig)
        {
            await UniTask.Delay((int)(waveConfig.TimeBetweenSpawns * 1000));

            _spawnedEnemies = await _spawnService.SpawnEnemies<EnemySpaceshipBehaviourComponent>(waveConfig);
            foreach(var enemy in _spawnedEnemies)
            {
                enemy.OnDestroyed += OnEnemyDestroyedCallback;
                enemy.StartEntryAnimation(waveConfig.EntrySpeed);
            }
        }

        private void OnEnemyDestroyedCallback(EnemySpaceshipBehaviourComponent enemy)
        {
            this.Log($"Enemy destroyed, remaining: {_spawnedEnemies.Count}");
            EnemyDestroyed?.Invoke(enemy.SpaceshipID);
            DespawnEnemy(enemy);

            if (_spawnedEnemies.Count == 0)
            {
                OnAllEnemiesDestroyed?.Invoke();
            }
        }

        private void DespawnEnemy(EnemySpaceshipBehaviourComponent enemy)
        {
            enemy.OnDestroyed -= OnEnemyDestroyedCallback;
            _spawnedEnemies.Remove(enemy);
            _spawnService.Despawn(enemy);
        }

        private void ClearEnemies()
        {
            for (int i = _spawnedEnemies.Count - 1; i >= 0; i--)
            {
                DespawnEnemy(_spawnedEnemies[i]);
            }
        }

        #region Debugging
        
        public void Tick()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                this.LogWarning("Debug: Destroying all enemies");
                
                for (int i = _spawnedEnemies.Count - 1; i >= 0; i--)
                {
                    OnEnemyDestroyedCallback(_spawnedEnemies[i]);
                }
            }
        }

        #endregion
    }
}
