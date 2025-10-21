using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface IEnemiesManager : IInitializable, IDisposable
    {
        event Action OnAllEnemiesDestroyed;
        public void SpawnEnemies(WaveConfigDTO wave);
    }

    public class EnemiesManager : IEnemiesManager
    {
        [Inject] private ISpawnService _spawnService;

        private List<EnemySpaceshipBehaviourComponent> _spawnedEnemies;

        public event Action OnAllEnemiesDestroyed;


        public void Initialize()
        {
            _spawnedEnemies = new List<EnemySpaceshipBehaviourComponent>();
        }

        public void Dispose()
        {
            for(int i = _spawnedEnemies.Count - 1; i >= 0; i--)
            {
                OnEnemyDestroyedCallback(_spawnedEnemies[i]);
            }
        }
        
        public async void SpawnEnemies(WaveConfigDTO waveConfig)
        {
            await UniTask.Delay((int)(waveConfig.TimeBetweenSpawns * 1000));

            _spawnedEnemies = await _spawnService.Spawn<EnemySpaceshipBehaviourComponent>(waveConfig);
            foreach(var enemy in _spawnedEnemies)
            {
                enemy.OnDestroyed += OnEnemyDestroyedCallback;
                enemy.StartEntryAnimation(waveConfig.EntrySpeed);
            }
        }

        private void OnEnemyDestroyedCallback(SpaceshipBehaviourComponent enemy)
        {
            enemy.OnDestroyed -= OnEnemyDestroyedCallback;
            _spawnedEnemies.Remove((EnemySpaceshipBehaviourComponent)enemy);
            _spawnService.Despawn(enemy);

            if(_spawnedEnemies.Count == 0)
            {
                OnAllEnemiesDestroyed?.Invoke();
            }
        }
    }
}
