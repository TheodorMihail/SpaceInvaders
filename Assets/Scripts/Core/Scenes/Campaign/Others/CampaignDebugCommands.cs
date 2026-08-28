#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Campaign
{
    /// <summary>
    /// The Campaign progression cheats. The managers own the actions, so they keep reaching their own
    /// private state; this only decides which of them the Campaign scene exposes and on which key.
    /// It is bound in the Campaign scene rather than the project context, which is what stops the
    /// cheats appearing in Expedition, since a scene's debug manager collects providers from its own
    /// container upward.
    /// </summary>
    public class CampaignDebugCommands : IDebugCommandProvider
    {
        [Inject] private readonly CurrencyManager _currencyManager;
        [Inject] private readonly InventoryManager _inventoryManager;
        [Inject] private readonly EquipmentManager _equipmentManager;
        [Inject] private readonly TalentManager _talentManager;
        [Inject] private readonly LevelProgressManager _levelProgressManager;

        public IReadOnlyList<DebugCommandDTO> GetDebugCommands()
        {
            return new[]
            {
                new DebugCommandDTO(DebugKeys.Campaign.AddCurrency, "Add currency", _currencyManager.DebugAddCurrency),
                new DebugCommandDTO(DebugKeys.Campaign.ClearCurrency, "Clear currency", _currencyManager.DebugClearCurrency),
                new DebugCommandDTO(DebugKeys.Campaign.AddRandomItem, "Add random item", _inventoryManager.DebugAddRandomItem),
                new DebugCommandDTO(DebugKeys.Campaign.ClearInventory, "Clear inventory", _inventoryManager.DebugClearInventory),
                new DebugCommandDTO(DebugKeys.Campaign.ClearEquipment, "Clear equipment", _equipmentManager.DebugClearEquipment),
                new DebugCommandDTO(DebugKeys.Campaign.ClearTalents, "Clear talents", _talentManager.DebugClearTalents),
                new DebugCommandDTO(DebugKeys.Campaign.ClearLevelProgress, "Clear level progress", _levelProgressManager.DebugClearLevelProgress)
            };
        }
    }
}
#endif
