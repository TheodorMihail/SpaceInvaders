using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Expedition;

namespace SpaceInvaders.Project
{
    public interface IExpeditionRepository
    {
        ExpeditionMapDataConfigSO GetMapDataConfig();
        ExpeditionPerksDataConfigSO GetPerksDataConfig();

        bool TryGetPerkConfig(string perkId, out ExpeditionPerkConfigSO config);

        /// <summary>Silent, unlike TryGet, so saved data can be validated without logging a miss.</summary>
        bool ContainsPerkConfig(string perkId);

        IReadOnlyList<ExpeditionPerkConfigSO> GetAllPerkConfigs();
        bool TryGetPerkRarityConfig(ExpeditionPerkRarityTypes rarity, out ExpeditionPerkRarityConfigSO config);
        IReadOnlyList<ExpeditionPerkRarityConfigSO> GetAllPerkRarityConfigs();
    }

    public class ExpeditionRepository : Repository, IExpeditionRepository
    {
        public ExpeditionRepository(ExpeditionMapDataConfigSO mapDataConfigSO,
            ExpeditionPerksDataConfigSO perksDataConfigSO)
        {
            AddObjects(perksDataConfigSO.PerkConfigs);
            AddObjects(perksDataConfigSO.RarityConfigs);

            AddObject(mapDataConfigSO);
            AddObject(perksDataConfigSO);
        }

        public ExpeditionMapDataConfigSO GetMapDataConfig()
        {
            TryGet(nameof(ExpeditionMapDataConfigSO), out ExpeditionMapDataConfigSO config);
            return config;
        }

        public ExpeditionPerksDataConfigSO GetPerksDataConfig()
        {
            TryGet(nameof(ExpeditionPerksDataConfigSO), out ExpeditionPerksDataConfigSO config);
            return config;
        }

        public bool TryGetPerkConfig(string perkId, out ExpeditionPerkConfigSO config)
        {
            return TryGet(perkId, out config);
        }

        public bool ContainsPerkConfig(string perkId)
        {
            foreach (ExpeditionPerkConfigSO config in GetAll<ExpeditionPerkConfigSO>())
            {
                if (config.ObjectID == perkId)
                {
                    return true;
                }
            }

            return false;
        }

        public IReadOnlyList<ExpeditionPerkConfigSO> GetAllPerkConfigs()
        {
            return GetAll<ExpeditionPerkConfigSO>().ToArray();
        }

        public bool TryGetPerkRarityConfig(ExpeditionPerkRarityTypes rarity, out ExpeditionPerkRarityConfigSO config)
        {
            return TryGet(rarity.ToString(), out config);
        }

        public IReadOnlyList<ExpeditionPerkRarityConfigSO> GetAllPerkRarityConfigs()
        {
            return GetAll<ExpeditionPerkRarityConfigSO>().ToArray();
        }
    }
}
