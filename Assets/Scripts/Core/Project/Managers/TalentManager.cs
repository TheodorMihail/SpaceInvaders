using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface ITalentManager : IInitializable
    {
        int GetTalentLevel(TalentTypes type);
        int GetNextLevelCost(TalentTypes type);
        bool IsMaxLevel(TalentTypes type);
        bool CanAfford(TalentTypes type);
        bool TryPurchaseLevel(TalentTypes type);
        void ApplyTalentBonuses(ShipStats stats);
    }

    public class TalentManager : ITalentManager
    {
        private const string SaveKey = "TalentProgress";

        [Inject] private readonly IPersistenceManager _persistenceManager;
        [Inject] private readonly IRepositoryManager _repositoryManager;
        [Inject] private readonly ICurrencyManager _currencyManager;

        private TalentSaveData _data;

        public void Initialize()
        {
            _data = _persistenceManager.Load<TalentSaveData>(SaveKey);
        }

        public int GetTalentLevel(TalentTypes type)
        {
            return GetTalentEntry(type)?.Level ?? 0;
        }

        public bool IsMaxLevel(TalentTypes type)
        {
            TalentConfigSO config = _repositoryManager.GetTalentConfig(type);
            return GetTalentLevel(type) >= config.MaxLevel;
        }

        public int GetNextLevelCost(TalentTypes type)
        {
            if (IsMaxLevel(type))
            {
                return -1;
            }

            TalentConfigSO config = _repositoryManager.GetTalentConfig(type);
            return config.Levels[GetTalentLevel(type)].Cost;
        }

        public bool CanAfford(TalentTypes type)
        {
            int cost = GetNextLevelCost(type);
            return cost >= 0 && _currencyManager.Currency >= cost;
        }

        public bool TryPurchaseLevel(TalentTypes type)
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

        public void ApplyTalentBonuses(ShipStats stats)
        {
            foreach (TalentConfigSO config in _repositoryManager.GetAllTalentConfigs())
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
        }

        private TalentSaveEntry GetTalentEntry(TalentTypes type)
        {
            return _data.Talents.Find(t => t.TalentType == type.ToString());
        }

        private TalentSaveEntry GetOrCreateTalentEntry(TalentTypes type)
        {
            TalentSaveEntry entry = GetTalentEntry(type);
            if (entry == null)
            {
                entry = new TalentSaveEntry { TalentType = type.ToString() };
                _data.Talents.Add(entry);
            }

            return entry;
        }

        private void SaveData()
        {
            _persistenceManager.Save(SaveKey, _data);
        }
    }
}
