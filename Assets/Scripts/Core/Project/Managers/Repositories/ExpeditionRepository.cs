using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Expedition;

namespace SpaceInvaders.Project
{
    public interface IExpeditionRepository
    {
        ExpeditionMapDataConfigSO GetMapDataConfig();
        ExpeditionRewardsDataConfigSO GetRewardsDataConfig();
    }

    public class ExpeditionRepository : Repository, IExpeditionRepository
    {
        public ExpeditionRepository(ExpeditionDataConfigSO expeditionDataConfigSO)
        {
            AddObject(expeditionDataConfigSO.MapData);
            AddObject(expeditionDataConfigSO.RewardsData);
        }

        public ExpeditionMapDataConfigSO GetMapDataConfig()
        {
            TryGet(nameof(ExpeditionMapDataConfigSO), out ExpeditionMapDataConfigSO config);
            return config;
        }

        public ExpeditionRewardsDataConfigSO GetRewardsDataConfig()
        {
            TryGet(nameof(ExpeditionRewardsDataConfigSO), out ExpeditionRewardsDataConfigSO config);
            return config;
        }
    }
}
