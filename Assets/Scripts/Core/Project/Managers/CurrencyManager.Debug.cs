#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Project
{
    public partial class CurrencyManager : IDebugCommandProvider
    {
        // Only the cheats need the config, so the dependency lives here rather than on the manager.
        [Inject] private readonly IProjectRepository _projectRepository;

        public IReadOnlyList<DebugCommand> GetDebugCommands()
        {
            return new[]
            {
                new DebugCommand(DebugKeys.AddCurrency, "Add currency", DebugAddCurrency),
                new DebugCommand(DebugKeys.ClearCurrency, "Clear currency", DebugClearCurrency)
            };
        }

        private void DebugAddCurrency()
        {
            int amount = _projectRepository.GetProjectDataConfig().DebugAddCurrencyAmount;
            AddCurrency(amount);
            this.LogWarning($"Debug: Added {amount} currency. New balance: {Currency}");
        }

        private void DebugClearCurrency()
        {
            _data.Amount = 0;
            SaveData();
            this.LogWarning("Debug: Currency cleared.");
        }
    }
}
#endif
