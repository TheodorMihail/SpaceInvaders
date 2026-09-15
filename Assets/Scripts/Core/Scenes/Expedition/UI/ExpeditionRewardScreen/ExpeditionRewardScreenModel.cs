using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Expedition
{
    public class ExpeditionRewardScreenModel : Model
    {
        [Inject] private readonly IExpeditionRunManager _expeditionRunManager;
        [Inject] private readonly IExpeditionRepository _expeditionRepository;

        public IReadOnlyList<ExpeditionPerkConfigSO> Choices { get; set; } = Array.Empty<ExpeditionPerkConfigSO>();

        /// <summary>Held once drawn, so rebuilding the view never rerolls what is on offer.</summary>
        public void DrawChoices()
        {
            Choices = _expeditionRunManager.DrawPerkChoices();
        }

        /// <summary>Paired with their tier, so a card presents a rarity without looking it up itself.</summary>
        public IEnumerable<(ExpeditionPerkConfigSO perk, ExpeditionPerkRarityConfigSO rarity)> GetChoices()
        {
            foreach (ExpeditionPerkConfigSO perk in Choices)
            {
                _expeditionRepository.TryGetPerkRarityConfig(perk.Rarity, out ExpeditionPerkRarityConfigSO rarity);
                yield return (perk, rarity);
            }
        }
    }
}
