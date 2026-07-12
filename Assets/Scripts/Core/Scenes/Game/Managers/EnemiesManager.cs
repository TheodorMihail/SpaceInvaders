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
        Enemy1,
        Enemy2,
        Boss1
    }

    public enum EnemyCategory
    {
        Normal,
        Boss
    }

    public interface IEnemiesManager : IDisposable, IGameInitializeListener, IGameEndListener
    {
        event Action<string> EnemyDestroyed;
        event Action OnAllEnemiesDestroyed;
        event Action<IEnemySpaceship> OnBossSpawned;
        event Action<int, int> OnBossHealthChanged;
        int EnemiesAlive { get; }
        public UniTask SpawnEnemies(WaveConfigDTO wave);
    }

    public class EnemiesManager : IEnemiesManager, ITickable
    {
        [Inject] private ISpawnService _spawnService;

        private List<IEnemySpaceship> _spawnedEnemies;

        public int EnemiesAlive => _spawnedEnemies.Count;
        public event Action<string> EnemyDestroyed;
        public event Action OnAllEnemiesDestroyed;
        public event Action<IEnemySpaceship> OnBossSpawned;
        public event Action<int, int> OnBossHealthChanged;

        public UniTask GameInitialize()
        {
            _spawnedEnemies = new List<IEnemySpaceship>();
            return UniTask.CompletedTask;
        }

        public UniTask GameEnd()
        {
            ClearEnemies();
            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            ClearEnemies();
        }
        
        public async UniTask SpawnEnemies(WaveConfigDTO waveConfig)
        {
            await UniTask.Delay((int)(waveConfig.TimeBetweenSpawns * 1000));

            var newEnemies = await _spawnService.SpawnEnemies(waveConfig);
            foreach (var enemy in newEnemies)
            {
                _spawnedEnemies.Add(enemy);
                enemy.OnDestroyed += OnEnemyDestroyedCallback;
                enemy.StartEntryAnimation(waveConfig.EntrySpeed);

                if (enemy.Category == EnemyCategory.Boss)
                {
                    enemy.OnHealthChanged += OnBossHealthChangedCallback;
                    OnBossSpawned?.Invoke(enemy);
                    OnBossHealthChanged?.Invoke(enemy.CurrentHealth, enemy.CurrentHealth);
                }
            }
        }

        private void OnBossHealthChangedCallback(int currentHealth, int maxHealth)
        {
            OnBossHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void OnEnemyDestroyedCallback(IEnemySpaceship enemy)
        {
            EnemyDestroyed?.Invoke(enemy.SpaceshipID);
            DespawnEnemy(enemy);

            this.Log($"Enemy destroyed, remaining: {_spawnedEnemies.Count}");
            if (_spawnedEnemies.Count == 0)
            {
                OnAllEnemiesDestroyed?.Invoke();
            }
        }

        private void DespawnEnemy(IEnemySpaceship enemy)
        {
            enemy.OnDestroyed -= OnEnemyDestroyedCallback;
            enemy.OnHealthChanged -= OnBossHealthChangedCallback;
            _spawnedEnemies.Remove(enemy);
            _spawnService.Despawn(enemy as EnemySpaceshipBehaviourComponent);
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Input.GetKeyDown(KeyCode.F1))
            {
                this.LogWarning("Debug: Destroying all enemies");

                for (int i = _spawnedEnemies.Count - 1; i >= 0; i--)
                {
                    OnEnemyDestroyedCallback(_spawnedEnemies[i]);
                }
            }
#endif
        }

        #endregion
    }
}
