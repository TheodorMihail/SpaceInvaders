using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface ICurrencyManager : IInitializable
    {
        int Currency { get; }
        void AddCurrency(int amount);
        bool TrySpend(int amount);
    }

    public partial class CurrencyManager : ICurrencyManager
    {
        [Inject] private readonly IPersistenceManager _persistenceManager;

        private CurrencySaveData _data;

        public int Currency => _data.Amount;

        public void Initialize()
        {
            _data = _persistenceManager.Load<CurrencySaveData>(CurrencySaveData.SaveKey);
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
