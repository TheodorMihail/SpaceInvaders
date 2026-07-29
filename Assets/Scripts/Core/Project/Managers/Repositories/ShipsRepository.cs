using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Project
{
    public interface IShipsRepository
    {
        bool TryGetPlayerConfig(PlayerTypes playerType, out PlayerSpaceshipConfigSO config);
        bool TryGetEnemyConfig(EnemyTypes enemyType, out EnemySpaceshipConfigSO config);
    }

    public class ShipsRepository : Repository, IShipsRepository
    {
        public ShipsRepository(PlayerDataConfigSO playerDataConfigSO, EnemyDataConfigSO enemyDataConfigSO)
        {
            AddObjects(playerDataConfigSO.PlayerConfigs);
            AddObjects(enemyDataConfigSO.EnemyConfigs);

            AddObject(playerDataConfigSO);
            AddObject(enemyDataConfigSO);
        }

        public bool TryGetPlayerConfig(PlayerTypes playerType, out PlayerSpaceshipConfigSO config)
        {
            return TryGet(playerType.ToString(), out config);
        }

        public bool TryGetEnemyConfig(EnemyTypes enemyType, out EnemySpaceshipConfigSO config)
        {
            return TryGet(enemyType.ToString(), out config);
        }
    }
}
