#if UNITY_EDITOR || DEVELOPMENT_BUILD
using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Project
{
    /// <summary>The cheat actions. Which scene exposes them, and on which key, is the scene's own
    /// debug provider's decision.</summary>
    public partial class CurrencyManager
    {
        // Only the cheats need the config, so the dependency lives here rather than on the manager.
        [Inject] private readonly IProjectRepository _projectRepository;

        public void DebugAddCurrency()
        {
            int amount = _projectRepository.GetProjectDataConfig().DebugAddCurrencyAmount;
            AddCurrency(amount);
            this.LogWarning($"Debug: Added {amount} currency. New balance: {Currency}");
        }

        public void DebugClearCurrency()
        {
            _data.Amount = 0;
            SaveData();
            this.LogWarning("Debug: Currency cleared.");
        }
    }
}
#endif
