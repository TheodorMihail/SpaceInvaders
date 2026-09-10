using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface ICurrencyManager : IModeScopedManager
    {
        int Currency { get; }
        void AddCurrency(int amount);
        bool TrySpend(int amount);
    }

    public partial class CurrencyManager : ICurrencyManager
    {
        [Inject] private readonly ISaveProfileManager _saveProfileManager;

        private IPersistenceManager _persistenceManager;
        private CurrencySaveData _data;

        public int Currency => _data.Amount;

        public void LoadForMode(GameModeTypes mode)
        {
            _persistenceManager = _saveProfileManager.GetProfile(mode);
            _data = _persistenceManager.LoadVersioned<CurrencySaveData>(CurrencySaveData.SaveKey, CurrencySaveData.CurrentVersion);
        }

        public void ClearLoadedData()
        {
            _data.Amount = 0;
            SaveData();
        }

        public void AddCurrency(int amount)
        {
            _data.Amount += amount;
            SaveData();
        }

        public bool TrySpend(int amount)
        {
            if (amount > _data.Amount)
            {
                return false;
            }

            _data.Amount -= amount;
            SaveData();
            return true;
        }

        private void SaveData()
        {
            _persistenceManager.Save(CurrencySaveData.SaveKey, _data);
        }
    }
}
