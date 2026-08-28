using BaseArchitecture.Core;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Scenes.Campaign
{
    public class TalentTreeModel : Model
    {
        [Inject] private readonly ITalentManager _talentManager;
        [Inject] private readonly ICurrencyManager _currencyManager;

        public int Currency => _currencyManager.Currency;

        public int GetTalentLevel(ShipUpgradableStatTypes type)
        {
            return _talentManager.GetTalentLevel(type);
        }

        public int GetNextLevelCost(ShipUpgradableStatTypes type)
        {
            return _talentManager.GetNextLevelCost(type);
        }

        public bool IsMaxLevel(ShipUpgradableStatTypes type)
        {
            return _talentManager.IsMaxLevel(type);
        }

        public bool CanAfford(ShipUpgradableStatTypes type)
        {
            return _talentManager.CanAfford(type);
        }
    }
}
