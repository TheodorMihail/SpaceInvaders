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

    public enum EnemyCategoryTypes
    {
        Normal,
        Boss
    }

    public interface IEnemiesManager : IDisposable, IGameInitializeListener, IGameEndListener
    {
        int EnemiesAlive { get; }
        public UniTask SpawnEnemies(WaveConfigDTO wave);
    }

    /// <summary>Owns the enemies of the current wave and republishes their events as bus messages.</summary>
    public partial class EnemiesManager : IEnemiesManager
    {
        [Inject] private readonly ISpawnService _spawnService;
        [Inject] private readonly IMessageBus _messageBus;

        private List<IEnemySpaceship> _spawnedEnemies;

        public int EnemiesAlive => _spawnedEnemies.Count;

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

            (float frontZ, float backZ) = GetFormationDepthRange(newEnemies);
            float longestDistance = 0f;

            foreach (var enemy in newEnemies)
            {
                _spawnedEnemies.Add(enemy);
                enemy.OnDestroyed += OnEnemyDestroyedCallback;
                enemy.OnShotFired += OnEnemyShotFiredCallback;
                enemy.OnDamaged += OnEnemyDamagedCallback;

                float distance = enemy.PrepareEntry(GetDepthRatio(enemy, frontZ, backZ));
                longestDistance = Mathf.Max(longestDistance, distance);

                if (enemy.Category == EnemyCategoryTypes.Boss)
                {
                    enemy.OnHealthChanged += OnBossHealthChangedCallback;
                    _messageBus.Publish(new BossSpawnedMessage(enemy.EnemyType, enemy.Stats.CurrentHealth));
                }
            }

            // The furthest ship sets the pace, so entry speed still means what it always did.
            float entryDuration = waveConfig.EntrySpeed > 0f ? longestDistance / waveConfig.EntrySpeed : 0f;

            foreach (var enemy in newEnemies)
            {
                enemy.StartEntryAnimation(entryDuration);
            }
        }

        /// <summary>Spawn depth spans the formation: the lowest value leads the wave in.</summary>
        private static (float frontZ, float backZ) GetFormationDepthRange(List<IEnemySpaceship> enemies)
        {
            float front = float.MaxValue;
            float back = float.MinValue;

            foreach (var enemy in enemies)
            {
                float z = enemy.LocalPosition.z;
                front = Mathf.Min(front, z);
                back = Mathf.Max(back, z);
            }

            return (front, back);
        }

        /// <summary>The ship leading the wave gets 1 so it lands deepest, the rearmost gets 0.</summary>
        private static float GetDepthRatio(IEnemySpaceship enemy, float frontZ, float backZ)
        {
            float span = backZ - frontZ;
            if (span <= Mathf.Epsilon)
            {
                return 0f;
            }

            return (backZ - enemy.LocalPosition.z) / span;
        }

        private void OnBossHealthChangedCallback(int currentHealth, int maxHealth)
        {
            _messageBus.Publish(new BossHealthChangedMessage(currentHealth, maxHealth));
        }

        private void OnEnemyDestroyedCallback(IEnemySpaceship enemy)
        {
            _messageBus.Publish(new EnemyDestroyedMessage(enemy.EnemyType, enemy.Category, enemy.LocalPosition));
            DespawnEnemy(enemy);

            // Triggers advancing the level
            if (_spawnedEnemies.Count == 0)
            {
                _messageBus.Publish(new AllEnemiesDestroyedMessage());
            }
        }

        private void OnEnemyShotFiredCallback(ISpaceship spaceship)
        {
            _messageBus.Publish(new ShipShotFiredMessage(spaceship.LocalPosition));
        }

        private void OnEnemyDamagedCallback(ISpaceship spaceship, int damage, bool isCritical)
        {
            _messageBus.Publish(new ShipDamagedMessage(spaceship.Stats.CurrentHealth, damage, isCritical, spaceship.WorldPosition));
        }

        private void DespawnEnemy(IEnemySpaceship enemy)
        {
            enemy.OnDestroyed -= OnEnemyDestroyedCallback;
            enemy.OnHealthChanged -= OnBossHealthChangedCallback;
            enemy.OnShotFired -= OnEnemyShotFiredCallback;
            enemy.OnDamaged -= OnEnemyDamagedCallback;
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
    }
}
