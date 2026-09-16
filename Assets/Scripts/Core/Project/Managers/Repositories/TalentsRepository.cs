using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Project
{
    public interface ITalentsRepository
    {
        bool TryGetTalentConfig(string talentId, out TalentConfigSO config);

        /// <summary>What one mode offers, which is not every talent that exists.</summary>
        IReadOnlyList<TalentConfigSO> GetTalentPool(GameModeTypes mode);

        IReadOnlyList<TalentConfigSO> GetAllTalentConfigs();
        bool TryGetTalentRarityConfig(TalentRarityTypes rarity, out TalentRarityConfigSO config);
        IReadOnlyList<TalentRarityConfigSO> GetAllTalentRarityConfigs();
    }

    /// <summary>
    /// Every talent any mode offers, plus the pools saying which mode offers what. Ids are unique
    /// across pools, so a talent in two of them is one talent rather than a copy.
    /// </summary>
    public class TalentsRepository : Repository, ITalentsRepository
    {
        private readonly Dictionary<GameModeTypes, List<TalentConfigSO>> _pools = new();

        public TalentsRepository(CampaignDataConfigSO campaignDataConfigSO,
            ExpeditionDataConfigSO expeditionDataConfigSO, TalentRaritiesDataConfigSO talentRaritiesDataConfigSO)
        {
            AddPool(campaignDataConfigSO);
            AddPool(expeditionDataConfigSO);

            AddObjects(talentRaritiesDataConfigSO.RarityConfigs);
        }

        public bool TryGetTalentConfig(string talentId, out TalentConfigSO config)
        {
            return TryGet(talentId, out config);
        }

        public IReadOnlyList<TalentConfigSO> GetTalentPool(GameModeTypes mode)
        {
            return _pools.TryGetValue(mode, out List<TalentConfigSO> pool) ? pool : new List<TalentConfigSO>();
        }

        public IReadOnlyList<TalentConfigSO> GetAllTalentConfigs()
        {
            return GetAll<TalentConfigSO>().ToArray();
        }

        public bool TryGetTalentRarityConfig(TalentRarityTypes rarity, out TalentRarityConfigSO config)
        {
            return TryGet(rarity.ToString(), out config);
        }

        public IReadOnlyList<TalentRarityConfigSO> GetAllTalentRarityConfigs()
        {
            return GetAll<TalentRarityConfigSO>().ToArray();
        }

        /// <summary>A mode with no pool authored offers nothing rather than throwing.</summary>
        private void AddPool(GameModeDataConfigSO runDataConfig)
        {
            var pool = new List<TalentConfigSO>();

            if (runDataConfig.TalentPool == null)
            {
                _pools[runDataConfig.Mode] = pool;
                return;
            }

            foreach (TalentConfigSO talent in runDataConfig.TalentPool.Talents)
            {
                pool.Add(talent);

                if (!ContainsTalent(talent.ObjectID))
                {
                    AddObject(talent);
                }
            }

            _pools[runDataConfig.Mode] = pool;
        }

        private bool ContainsTalent(string talentId)
        {
            foreach (TalentConfigSO talent in GetAll<TalentConfigSO>())
            {
                if (talent.ObjectID == talentId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
