using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Expedition;

namespace SpaceInvaders.Project
{
    public interface IExpeditionRepository
    {
        ExpeditionDataConfigSO GetExpeditionDataConfig();
    }

    public class ExpeditionRepository : Repository, IExpeditionRepository
    {
        public ExpeditionRepository(ExpeditionDataConfigSO expeditionDataConfigSO)
        {
            AddObject(expeditionDataConfigSO);
        }

        public ExpeditionDataConfigSO GetExpeditionDataConfig()
        {
            TryGet(nameof(ExpeditionDataConfigSO), out ExpeditionDataConfigSO config);
            return config;
        }
    }
}
