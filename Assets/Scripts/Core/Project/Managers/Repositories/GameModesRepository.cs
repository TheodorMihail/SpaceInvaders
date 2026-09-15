using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Project
{
    public interface IGameModesRepository
    {
        GameModeRunDataConfigSO GetRunDataConfig(GameModeTypes mode);
    }

    public class GameModesRepository : Repository, IGameModesRepository
    {
        public GameModesRepository(CampaignRunDataConfigSO campaignRunDataConfigSO,
            ExpeditionRunDataConfigSO expeditionRunDataConfigSO)
        {
            // Added as the base type: buckets key on the generic argument, so the concrete types
            // would otherwise land in buckets nothing reads.
            AddObject<GameModeRunDataConfigSO>(campaignRunDataConfigSO);
            AddObject<GameModeRunDataConfigSO>(expeditionRunDataConfigSO);
        }

        public GameModeRunDataConfigSO GetRunDataConfig(GameModeTypes mode)
        {
            TryGet(mode.ToString(), out GameModeRunDataConfigSO config);
            return config;
        }
    }
}
