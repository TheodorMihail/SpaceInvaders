using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Project
{
    public interface ITalentsRepository
    {
        bool TryGetTalentConfig(ShipUpgradableStatTypes talentType, out TalentConfigSO config);
        IReadOnlyList<TalentConfigSO> GetAllTalentConfigs();
    }

    public class TalentsRepository : Repository, ITalentsRepository
    {
        public TalentsRepository(TalentsDataConfigSO talentsDataConfigSO)
        {
            AddObjects(talentsDataConfigSO.TalentConfigs);
            AddObject(talentsDataConfigSO);
        }

        public bool TryGetTalentConfig(ShipUpgradableStatTypes talentType, out TalentConfigSO config)
        {
            return TryGet(talentType.ToString(), out config);
        }

        public IReadOnlyList<TalentConfigSO> GetAllTalentConfigs()
        {
            return GetAll<TalentConfigSO>().ToArray();
        }
    }
}
