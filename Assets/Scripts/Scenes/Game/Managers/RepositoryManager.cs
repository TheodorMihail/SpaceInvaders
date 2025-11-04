using System.Collections.Generic;
using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface IRepositoryManager
    {
        LevelConfigSO GetLevelConfig(int levelNumber);
        PlayerSpaceshipConfigSO GetPlayerConfig(string playerID);
        EnemySpaceshipConfigSO GetEnemyConfig(string enemyID);
    }

    public class RepositoryManager : IRepositoryManager
    {
        [Inject] private readonly IRepository _repository;

        public RepositoryManager(
            List<IRepositoryObject> levelsConfigs,
            List<IRepositoryObject> playersConfigs,
            List<IRepositoryObject> enemiesConfigs)
        {
            _repository.AddObjects(levelsConfigs);
            _repository.AddObjects(playersConfigs);
            _repository.AddObjects(enemiesConfigs);
        }


        public LevelConfigSO GetLevelConfig(int levelNumber)
        {
            return _repository.GetObject($"Level {levelNumber}") as LevelConfigSO;
        }

        public PlayerSpaceshipConfigSO GetPlayerConfig(string playerID)
        {
            return _repository.GetObject(playerID) as PlayerSpaceshipConfigSO;
        }

        public EnemySpaceshipConfigSO GetEnemyConfig(string enemyID)
        {
            return _repository.GetObject(enemyID) as EnemySpaceshipConfigSO;
        }
    }
}
