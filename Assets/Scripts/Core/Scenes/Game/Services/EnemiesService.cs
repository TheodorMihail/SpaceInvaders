using System;
using System.Collections.Generic;
using System.Threading;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public enum EnemyTypes
    {
        Enemy1 = 0,
        Enemy2 = 1,
        Enemy3 = 2,
        Enemy4 = 3,
        Boss1 = 50,
        Boss2 = 51
    }

    public enum EnemyCategoryTypes
    {
        Normal,
        Boss
    }

    public interface IEnemiesService
    {
        int EnemiesAlive { get; }

        void GameInitialize();
        void GameEnd();
        UniTask SpawnEnemies(WaveConfigDTO wave);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        void DebugDestroyAllEnemies();
#endif
    }

    /// <summary>Owns the enemies of the current wave and republishes their events as bus messages.</summary>
    public partial class EnemiesService : IEnemiesService
    {
        [Inject] private readonly ISpawnManager _spawnManager;
        [Inject] private readonly IMessageBus _messageBus;

        private List<IEnemySpaceship> _spawnedEnemies;
        private CancellationTokenSource _spawnCancellationTokenSource;

        /// <summary>Reinforcements asked for but not yet loaded. A wave is only clear once these land,
        /// otherwise a splitting enemy dying last would advance the level before its children exist.</summary>
        private int _pendingSpawnRequests;

        public int EnemiesAlive => _spawnedEnemies.Count;

        public void GameInitialize()
        {
            // The next level re-initializes without disposing, so a previous run's source can still be here.
            CancelSpawning();

            _spawnedEnemies = new List<IEnemySpaceship>();
            _spawnCancellationTokenSource = new CancellationTokenSource();
            _pendingSpawnRequests = 0;
        }

        public void GameEnd()
        {
            CancelSpawning();
            ClearEnemies();
            _pendingSpawnRequests = 0;
        }

        public async UniTask SpawnEnemies(WaveConfigDTO waveConfig)
        {
            if (_spawnCancellationTokenSource == null)
            {
                return;
            }

            // Held locally: the source is disposed on cancellation, while the token stays readable.
            CancellationToken token = _spawnCancellationTokenSource.Token;

            await UniTask.Delay((int)(waveConfig.TimeBetweenSpawns * 1000), cancellationToken: token);

            if (token.IsCancellationRequested)
            {
                return;
            }

            var newEnemies = await _spawnManager.SpawnEnemies(waveConfig);

            // A wave that lands after the run ended belongs to nothing, so it goes straight back.
            if (token.IsCancellationRequested)
            {
                DespawnEnemies(newEnemies);
                return;
            }

            (float frontZ, float backZ) = GetFormationDepthRange(newEnemies);
            float longestDistance = 0f;

            foreach (var enemy in newEnemies)
            {
                RegisterEnemy(enemy);

                float distance = enemy.PrepareEntry(GetDepthRatio(enemy, frontZ, backZ));
                longestDistance = Mathf.Max(longestDistance, distance);
            }

            // The furthest ship sets the pace, so entry speed still means what it always did.
            float entryDuration = waveConfig.EntrySpeed > 0f ? longestDistance / waveConfig.EntrySpeed : 0f;

            foreach (var enemy in newEnemies)
            {
                enemy.StartEntryAnimation(entryDuration);

                // Announced as it starts its run in rather than when it was built, so the arrival and
                // whatever plays off it line up with the boss actually moving into view.
                if (enemy.Category == EnemyCategoryTypes.Boss)
                {
                    _messageBus.Publish(new BossSpawnedMessage(enemy.EnemyType, enemy.Stats.CurrentHealth));
                }
            }
        }

        private void RegisterEnemy(IEnemySpaceship enemy)
        {
            _spawnedEnemies.Add(enemy);
            enemy.OnDestroyed += OnEnemyDestroyedCallback;
            enemy.OnShotFired += OnEnemyShotFiredCallback;
            enemy.OnDamaged += OnEnemyDamagedCallback;
            enemy.OnSpawnRequested += OnEnemySpawnRequestedCallback;
            enemy.OnEnteredView += OnEnemyEnteredViewCallback;

            if (enemy.Category == EnemyCategoryTypes.Boss)
            {
                enemy.OnHealthChanged += OnBossHealthChangedCallback;
            }
        }

        /// <summary>Only a boss is announced on entering view; regular enemies arrive as a wave.</summary>
        private void OnEnemyEnteredViewCallback(IEnemySpaceship enemy)
        {
            if (enemy.Category != EnemyCategoryTypes.Boss)
            {
                return;
            }

            _messageBus.Publish(new BossEnteredMessage(enemy.EnemyType));
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

            NotifyIfWaveCleared();
        }

        /// <summary>A ship asking for reinforcements. Counted before the load starts, so the wave
        /// cannot be reported clear in the frames the new ships are still being built.</summary>
        private void OnEnemySpawnRequestedCallback(IEnemySpaceship requester, EnemySpawnRequestDTO request)
        {
            if (request.Spawn.Count <= 0 || _spawnCancellationTokenSource == null)
            {
                return;
            }

            _pendingSpawnRequests++;
            SpawnRequestedEnemies(request, _spawnCancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid SpawnRequestedEnemies(EnemySpawnRequestDTO request, CancellationToken token)
        {
            var requestedEnemies = new List<IEnemySpaceship>();

            for (int i = 0; i < request.Spawn.Count; i++)
            {
                // Centred on the origin, so the ships fan out evenly either side of where they were asked for.
                float offsetX = (i - (request.Spawn.Count - 1) * 0.5f) * request.Spawn.Spread;
                var enemy = await _spawnManager.SpawnEnemy(request.Spawn.EnemyType, request.LocalPosition + new Vector3(offsetX, 0f, 0f));

                if (enemy != null)
                {
                    requestedEnemies.Add(enemy);
                }
            }

            _pendingSpawnRequests--;

            // Reinforcements that land after the run ended belong to nothing, so they go straight back.
            if (token.IsCancellationRequested)
            {
                foreach (var enemy in requestedEnemies)
                {
                    _spawnManager.Despawn(enemy as EnemySpaceshipBehaviourComponent);
                }

                return;
            }

            foreach (var enemy in requestedEnemies)
            {
                RegisterEnemy(enemy);
                enemy.SkipEntry();
            }

            NotifyIfWaveCleared();
        }

        /// <summary>Triggers advancing the level.</summary>
        private void NotifyIfWaveCleared()
        {
            if (_spawnedEnemies.Count == 0 && _pendingSpawnRequests == 0)
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
            _messageBus.Publish(new ShipDamagedMessage(spaceship.Stats.CurrentHealth, damage, isCritical, spaceship.WorldPosition, isPlayer: false));
        }

        private void DespawnEnemy(IEnemySpaceship enemy)
        {
            enemy.OnDestroyed -= OnEnemyDestroyedCallback;
            enemy.OnHealthChanged -= OnBossHealthChangedCallback;
            enemy.OnShotFired -= OnEnemyShotFiredCallback;
            enemy.OnDamaged -= OnEnemyDamagedCallback;
            enemy.OnSpawnRequested -= OnEnemySpawnRequestedCallback;
            enemy.OnEnteredView -= OnEnemyEnteredViewCallback;
            _spawnedEnemies.Remove(enemy);
            _spawnManager.Despawn(enemy as EnemySpaceshipBehaviourComponent);
        }

        private void DespawnEnemies(List<IEnemySpaceship> enemies)
        {
            foreach (var enemy in enemies)
            {
                DespawnEnemy(enemy);
            }
        }

        private void ClearEnemies()
        {
            for (int i = _spawnedEnemies.Count - 1; i >= 0; i--)
            {
                DespawnEnemy(_spawnedEnemies[i]);
            }
        }

        /// <summary>A wave in flight outlives the run that asked for it: the delay between waves is
        /// time scaled, so pausing freezes a pending spawn until long after the level ended.</summary>
        private void CancelSpawning()
        {
            _spawnCancellationTokenSource?.CancelAndDispose();
            _spawnCancellationTokenSource = null;
        }
    }
}
