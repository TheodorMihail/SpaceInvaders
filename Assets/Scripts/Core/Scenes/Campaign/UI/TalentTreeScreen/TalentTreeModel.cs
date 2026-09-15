using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Campaign
{
    public class TalentTreeModel : Model
    {
        [Inject] private readonly ITalentManager _talentManager;
        [Inject] private readonly ICurrencyManager _currencyManager;

        public int Currency => _currencyManager.Currency;

        public int GetTalentLevel(string talentId)
        {
            return _talentManager.GetTalentLevel(talentId);
        }

        public int GetNextLevelCost(string talentId)
        {
            return _talentManager.GetNextLevelCost(talentId);
        }

        public bool IsMaxLevel(string talentId)
        {
            return _talentManager.IsMaxLevel(talentId);
        }

        public bool CanAfford(string talentId)
        {
            return _talentManager.CanAfford(talentId);
        }
    }
}
