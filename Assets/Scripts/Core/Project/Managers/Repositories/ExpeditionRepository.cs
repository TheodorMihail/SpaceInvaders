using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Expedition;

namespace SpaceInvaders.Project
{
    public interface IExpeditionRepository
    {
        ExpeditionMapDataConfigSO GetMapDataConfig();
        ExpeditionRewardsDataConfigSO GetRewardsDataConfig();
        ExpeditionShopDataConfigSO GetShopDataConfig();
    }

    public class ExpeditionRepository : Repository, IExpeditionRepository
    {
        public ExpeditionRepository(ExpeditionDataConfigSO expeditionDataConfigSO)
        {
            AddObject(expeditionDataConfigSO.MapData);
            AddObject(expeditionDataConfigSO.RewardsData);
            AddObject(expeditionDataConfigSO.ShopData);
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

        public ExpeditionShopDataConfigSO GetShopDataConfig()
        {
            TryGet(nameof(ExpeditionShopDataConfigSO), out ExpeditionShopDataConfigSO config);
            return config;
        }
    }
}
