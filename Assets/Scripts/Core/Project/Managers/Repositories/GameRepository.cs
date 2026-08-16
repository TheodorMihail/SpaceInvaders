using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Project
{
    public interface IGameRepository
    {
        GameDataConfigSO GetGameDataConfig();
    }

    public class GameRepository : Repository, IGameRepository
    {
        public GameRepository(GameDataConfigSO gameDataConfigSO)
        {
            AddObject(gameDataConfigSO);
        }

        public GameDataConfigSO GetGameDataConfig()
        {
            TryGet(nameof(GameDataConfigSO), out GameDataConfigSO config);
            return config;
        }
    }
}
