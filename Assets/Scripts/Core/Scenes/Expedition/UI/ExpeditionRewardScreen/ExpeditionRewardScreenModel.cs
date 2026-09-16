using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Scenes.Expedition
{
    public class ExpeditionRewardScreenModel : Model
    {
        [Inject] private readonly IExpeditionRunManager _expeditionRunManager;
        [Inject] private readonly ITalentsRepository _talentsRepository;
        [Inject] private readonly ITalentManager _talentManager;

        public IReadOnlyList<TalentConfigSO> Choices { get; set; } = Array.Empty<TalentConfigSO>();

        /// <summary>Held once drawn, so rebuilding the view never rerolls what is on offer.</summary>
        public void DrawChoices()
        {
            Choices = _expeditionRunManager.DrawTalentChoices();
        }

        /// <summary>Paired with their tier and the level already held, so a card presents itself
        /// without looking anything up.</summary>
        public IEnumerable<ExpeditionTalentOfferDTO> GetOffers()
        {
            foreach (TalentConfigSO talent in Choices)
            {
                _talentsRepository.TryGetTalentRarityConfig(talent.Rarity, out TalentRarityConfigSO rarity);

                yield return new ExpeditionTalentOfferDTO(talent, rarity, _talentManager.GetTalentLevel(talent.ObjectID));
            }
        }
    }
}
