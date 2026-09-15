using System.Collections.Generic;
using SpaceInvaders.Scenes.Expedition;
using SpaceInvaders.Scenes.Game;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface IExpeditionPerkDrawService
    {
        /// <summary>The cards for one reward screen. Empty when nothing can be offered.</summary>
        IReadOnlyList<ExpeditionPerkConfigSO> DrawPerks(bool isBossDraw, ISet<string> ownedPerkIds);
    }

    /// <summary>
    /// Picks one screen's cards: a rarity per card by weight, then a perk of that rarity. Cards are
    /// distinct within an offer, so a draw never asks the player to choose between two of the same
    /// thing. A perk already owned is still eligible where picking it twice stacks.
    /// </summary>
    public class ExpeditionPerkDrawService : IExpeditionPerkDrawService
    {
        [Inject] private readonly IExpeditionRepository _expeditionRepository;

        public IReadOnlyList<ExpeditionPerkConfigSO> DrawPerks(bool isBossDraw, ISet<string> ownedPerkIds)
        {
            ExpeditionPerksDataConfigSO config = _expeditionRepository.GetPerksDataConfig();
            List<ExpeditionPerkConfigSO> candidates = GetCandidates(config, isBossDraw, ownedPerkIds);
            var drawn = new List<ExpeditionPerkConfigSO>();

            while (drawn.Count < config.PerkChoiceCount && candidates.Count > 0)
            {
                ExpeditionPerkConfigSO perk = DrawOne(candidates);

                if (perk == null)
                {
                    break;
                }

                drawn.Add(perk);
                candidates.Remove(perk);
            }

            return drawn;
        }

        /// <summary>A boss draw pays out above a floor, unless that leaves nothing: offering nothing
        /// would read worse than offering a common. An owned perk that cannot stack is dropped either
        /// way, since a second copy of it would change nothing.</summary>
        private List<ExpeditionPerkConfigSO> GetCandidates(ExpeditionPerksDataConfigSO config, bool isBossDraw,
            ISet<string> ownedPerkIds)
        {
            IReadOnlyList<ExpeditionPerkConfigSO> allPerks = _expeditionRepository.GetAllPerkConfigs();
            var offerable = new List<ExpeditionPerkConfigSO>();
            var candidates = new List<ExpeditionPerkConfigSO>();

            foreach (ExpeditionPerkConfigSO perk in allPerks)
            {
                if (!perk.IsStackable && ownedPerkIds != null && ownedPerkIds.Contains(perk.ObjectID))
                {
                    continue;
                }

                offerable.Add(perk);

                if (!isBossDraw || perk.Rarity >= config.BossMinPerkRarity)
                {
                    candidates.Add(perk);
                }
            }

            return candidates.Count > 0 ? candidates : offerable;
        }

        /// <summary>Only tiers still holding a candidate are weighed, so an exhausted one is skipped
        /// rather than wasting the card.</summary>
        private ExpeditionPerkConfigSO DrawOne(List<ExpeditionPerkConfigSO> candidates)
        {
            var availableRarities = new List<ExpeditionPerkRarityConfigSO>();

            foreach (ExpeditionPerkRarityConfigSO rarityConfig in _expeditionRepository.GetAllPerkRarityConfigs())
            {
                if (HasRarity(candidates, rarityConfig.Rarity))
                {
                    availableRarities.Add(rarityConfig);
                }
            }

            ExpeditionPerkRarityConfigSO rolled = GameUtils.RollWeighted(availableRarities,
                candidate => candidate.DrawWeight);

            // Unauthored or zero-weight tiers leave nothing to weigh, so the offer fills out flat.
            return rolled == null
                ? candidates[Random.Range(0, candidates.Count)]
                : GetRandomOfRarity(candidates, rolled.Rarity);
        }

        private static bool HasRarity(List<ExpeditionPerkConfigSO> candidates, ExpeditionPerkRarityTypes rarity)
        {
            foreach (ExpeditionPerkConfigSO candidate in candidates)
            {
                if (candidate.Rarity == rarity)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Counted first so the pick stays uniform without building a second list.</summary>
        private static ExpeditionPerkConfigSO GetRandomOfRarity(List<ExpeditionPerkConfigSO> candidates,
            ExpeditionPerkRarityTypes rarity)
        {
            int matches = 0;
            foreach (ExpeditionPerkConfigSO candidate in candidates)
            {
                if (candidate.Rarity == rarity)
                {
                    matches++;
                }
            }

            if (matches == 0)
            {
                return null;
            }

            int index = Random.Range(0, matches);
            foreach (ExpeditionPerkConfigSO candidate in candidates)
            {
                if (candidate.Rarity != rarity)
                {
                    continue;
                }

                if (index == 0)
                {
                    return candidate;
                }

                index--;
            }

            return null;
        }
    }
}
