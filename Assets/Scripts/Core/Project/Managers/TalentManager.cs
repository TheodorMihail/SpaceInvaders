using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface ITalentManager : IGameModeScopedManager
    {
        int GetTalentLevel(string talentId);
        int GetNextLevelCost(string talentId);
        bool IsMaxLevel(string talentId);
        bool CanAfford(string talentId);
        bool TryPurchaseLevel(string talentId);
        void ApplyTalentBonuses(ShipStats stats);
    }

    public partial class TalentManager : ITalentManager
    {
        [Inject] private readonly ISaveProfileManager _saveProfileManager;

        private IPersistenceManager _persistenceManager;
        [Inject] private readonly ITalentsRepository _talentsRepository;
        [Inject] private readonly ICurrencyManager _currencyManager;

        private TalentsSaveData _data;

        public void LoadForMode(GameModeTypes mode)
        {
            _persistenceManager = _saveProfileManager.GetProfile(mode);
            _data = _persistenceManager.LoadVersioned<TalentsSaveData>(TalentsSaveData.SaveKey, TalentsSaveData.CurrentVersion);
        }

        public void ClearLoadedData()
        {
            _data.Talents.Clear();
            SaveData();
        }

        public int GetTalentLevel(string talentId)
        {
            return GetTalent(talentId)?.Level ?? 0;
        }

        public bool IsMaxLevel(string talentId)
        {
            if (!_talentsRepository.TryGetTalentConfig(talentId, out TalentConfigSO config))
            {
                return true;
            }

            return GetTalentLevel(talentId) >= config.MaxLevel;
        }

        public int GetNextLevelCost(string talentId)
        {
            if (IsMaxLevel(talentId) || !_talentsRepository.TryGetTalentConfig(talentId, out TalentConfigSO config))
            {
                return -1;
            }

            return config.Levels[GetTalentLevel(talentId)].Cost;
        }

        public bool CanAfford(string talentId)
        {
            int cost = GetNextLevelCost(talentId);
            return cost >= 0 && _currencyManager.Currency >= cost;
        }

        public bool TryPurchaseLevel(string talentId)
        {
            int cost = GetNextLevelCost(talentId);
            if (cost < 0)
            {
                return false;
            }

            if (!_currencyManager.TrySpend(cost))
            {
                return false;
            }

            TalentSaveEntry entry = GetOrCreateTalentEntry(talentId);
            entry.Level++;
            SaveData();
            return true;
        }

        /// <summary>Applies every level bought, one at a time, then refills health since max health
        /// may have changed.</summary>
        public void ApplyTalentBonuses(ShipStats stats)
        {
            foreach (TalentConfigSO config in _talentsRepository.GetAllTalentConfigs())
            {
                int ownedLevel = GetTalentLevel(config.ObjectID);

                for (int i = 0; i < ownedLevel && i < config.MaxLevel; i++)
                {
                    config.ApplyLevel(stats, i);
                }
            }

            stats.RefillHealth();
            stats.RefillAmmo();
        }

        private TalentSaveEntry GetTalent(string talentId)
        {
            return _data.Talents.Find(talent => talent.TalentId == talentId);
        }

        private TalentSaveEntry GetOrCreateTalentEntry(string talentId)
        {
            TalentSaveEntry entry = GetTalent(talentId);
            if (entry == null)
            {
                entry = new TalentSaveEntry { TalentId = talentId };
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
