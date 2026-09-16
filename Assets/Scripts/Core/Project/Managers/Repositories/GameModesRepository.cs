using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Project
{
    public interface IGameModesRepository
    {
        GameModeDataConfigSO GetDataConfig(GameModeTypes mode);
    }

    public class GameModesRepository : Repository, IGameModesRepository
    {
        public GameModesRepository(CampaignDataConfigSO campaignDataConfigSO,
            ExpeditionDataConfigSO expeditionDataConfigSO)
        {
            // Added as the base type: buckets key on the generic argument, so the concrete types
            // would otherwise land in buckets nothing reads.
            AddObject<GameModeDataConfigSO>(campaignDataConfigSO);
            AddObject<GameModeDataConfigSO>(expeditionDataConfigSO);
        }

        public GameModeDataConfigSO GetDataConfig(GameModeTypes mode)
        {
            TryGet(mode.ToString(), out GameModeDataConfigSO config);
            return config;
        }
    }
}
