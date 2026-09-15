using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Project
{
    public interface ITalentsRepository
    {
        bool TryGetTalentConfig(string talentId, out TalentConfigSO config);
        IReadOnlyList<TalentConfigSO> GetAllTalentConfigs();
    }

    public class TalentsRepository : Repository, ITalentsRepository
    {
        public TalentsRepository(TalentsDataConfigSO talentsDataConfigSO)
        {
            AddObjects(talentsDataConfigSO.TalentConfigs);
            AddObject(talentsDataConfigSO);
        }

        public bool TryGetTalentConfig(string talentId, out TalentConfigSO config)
        {
            return TryGet(talentId, out config);
        }

        public IReadOnlyList<TalentConfigSO> GetAllTalentConfigs()
        {
            return GetAll<TalentConfigSO>().ToArray();
        }
    }
}
