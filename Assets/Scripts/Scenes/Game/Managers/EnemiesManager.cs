using System;
using System.Collections.Generic;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface IEnemiesManager : IInitializable
    {
        event Action OnAllEnemiesDestroyed;
        public void SpawnEnemies(WaveConfigDTO wave);
    }

    public class EnemiesManager : IEnemiesManager
    {
        [Inject] private ISpawnService _spawnService;

        private List<SpaceshipBehaviourComponent> _spawnedEnemies;

        public event Action OnAllEnemiesDestroyed;

        public void Initialize()
        {
            _spawnedEnemies = new List<SpaceshipBehaviourComponent>();
        }

        public async void SpawnEnemies(WaveConfigDTO waveConfig)
        {
            _spawnedEnemies = await _spawnService.Spawn<SpaceshipBehaviourComponent>(waveConfig);
            foreach(var enemy in _spawnedEnemies)
            {
                enemy.OnDestroyed += OnEnemyDestroyedCallback;
            }
        }

        private void OnEnemyDestroyedCallback(SpaceshipBehaviourComponent enemy)
        {
            _spawnedEnemies.Remove(enemy);

            if(_spawnedEnemies.Count == 0)
            {
                OnAllEnemiesDestroyed?.Invoke();
            }
        }
    }
}
