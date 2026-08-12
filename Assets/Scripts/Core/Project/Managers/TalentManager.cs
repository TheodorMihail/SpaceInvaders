using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface ITalentManager : IInitializable
    {
        int GetTalentLevel(ShipUpgradableStatTypes type);
        int GetNextLevelCost(ShipUpgradableStatTypes type);
        bool IsMaxLevel(ShipUpgradableStatTypes type);
        bool CanAfford(ShipUpgradableStatTypes type);
        bool TryPurchaseLevel(ShipUpgradableStatTypes type);
        void ApplyTalentBonuses(ShipStats stats);
    }

    public partial class TalentManager : ITalentManager
    {
        [Inject] private readonly IPersistenceManager _persistenceManager;
        [Inject] private readonly ITalentsRepository _talentsRepository;
        [Inject] private readonly ICurrencyManager _currencyManager;

        private TalentsSaveData _data;

        public void Initialize()
        {
            _data = _persistenceManager.Load<TalentsSaveData>(TalentsSaveData.SaveKey);
        }

        public int GetTalentLevel(ShipUpgradableStatTypes type)
        {
            return GetTalent(type)?.Level ?? 0;
        }

        public bool IsMaxLevel(ShipUpgradableStatTypes type)
        {
            if (!_talentsRepository.TryGetTalentConfig(type, out TalentConfigSO config))
            {
                return true;
            }

            return GetTalentLevel(type) >= config.MaxLevel;
        }

        public int GetNextLevelCost(ShipUpgradableStatTypes type)
        {
            if (IsMaxLevel(type) || !_talentsRepository.TryGetTalentConfig(type, out TalentConfigSO config))
            {
                return -1;
            }

            return config.Levels[GetTalentLevel(type)].Cost;
        }

        public bool CanAfford(ShipUpgradableStatTypes type)
        {
            int cost = GetNextLevelCost(type);
            return cost >= 0 && _currencyManager.Currency >= cost;
        }

        public bool TryPurchaseLevel(ShipUpgradableStatTypes type)
        {
            int cost = GetNextLevelCost(type);
            if (cost < 0)
            {
                return false;
            }

            if (!_currencyManager.TrySpend(cost))
            {
                return false;
            }

            TalentSaveEntry entry = GetOrCreateTalentEntry(type);
            entry.Level++;
            SaveData();
            return true;
        }

        /// <summary>Applies the sum of the purchased level deltas for each talent, then refills
        /// health since max health may have changed.</summary>
        public void ApplyTalentBonuses(ShipStats stats)
        {
            foreach (TalentConfigSO config in _talentsRepository.GetAllTalentConfigs())
            {
                int ownedLevel = GetTalentLevel(config.TalentType);
                if (ownedLevel <= 0)
                {
                    continue;
                }

                float totalBonus = 0f;
                for (int i = 0; i < ownedLevel; i++)
                {
                    totalBonus += config.Levels[i].BonusDelta;
                }

                config.ApplyBonus(stats, totalBonus);
            }

            stats.RefillHealth();
            stats.RefillAmmo();
        }

        private TalentSaveEntry GetTalent(ShipUpgradableStatTypes type)
        {
            return _data.Talents.Find(t => t.TalentType == type.ToString());
        }

        private TalentSaveEntry GetOrCreateTalentEntry(ShipUpgradableStatTypes type)
        {
            TalentSaveEntry entry = GetTalent(type);
            if (entry == null)
            {
                entry = new TalentSaveEntry { TalentType = type.ToString() };
                _data.Talents.Add(entry);
            }

            return entry;
        }

        private void SaveData()
        {
            _persistenceManager.Save(TalentsSaveData.SaveKey, _data);
        }
    }
}
