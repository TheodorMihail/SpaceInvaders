using BaseArchitecture.Core;
using UnityEngine.InputSystem;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface ICurrencyManager : IInitializable
    {
        int Currency { get; }
        void AddCurrency(int amount);
        bool TrySpend(int amount);
    }

    public class CurrencyManager : ICurrencyManager, ITickable
    {
        [Inject] private readonly IPersistenceManager _persistenceManager;
        [Inject] private readonly IProjectRepository _projectRepository;

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

        #region Debugging

        public void Tick()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.f3Key.wasPressedThisFrame)
            {
                int amount = _projectRepository.GetProjectDataConfig().DebugAddCurrencyAmount;
                AddCurrency(amount);
                this.LogWarning($"Debug: Added {amount} currency. New balance: {Currency}");
            }

            if (Keyboard.current.f9Key.wasPressedThisFrame)
            {
                _data.Amount = 0;
                SaveData();
                this.LogWarning("Debug: Currency cleared.");
            }
#endif
        }

        #endregion
    }
}
