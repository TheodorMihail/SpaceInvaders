using System.Collections.Generic;
using SpaceInvaders.Scenes.Expedition;
using SpaceInvaders.Scenes.Game;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface IExpeditionTalentDrawService
    {
        /// <summary>The cards for one reward screen. Empty when nothing can be offered.</summary>
        IReadOnlyList<TalentConfigSO> DrawTalents(bool isBossDraw);
    }

    /// <summary>
    /// Picks one screen's cards: a rarity per card by weight, then a talent of that rarity. Cards are
    /// distinct within an offer, and a talent stays eligible until it is maxed.
    /// </summary>
    public class ExpeditionTalentDrawService : IExpeditionTalentDrawService
    {
        [Inject] private readonly ITalentsRepository _talentsRepository;
        [Inject] private readonly ITalentManager _talentManager;
        [Inject] private readonly IExpeditionRepository _expeditionRepository;

        public IReadOnlyList<TalentConfigSO> DrawTalents(bool isBossDraw)
        {
            ExpeditionRewardsDataConfigSO config = _expeditionRepository.GetRewardsDataConfig();
            List<TalentConfigSO> candidates = GetCandidates(config, isBossDraw);
            var drawn = new List<TalentConfigSO>();

            while (drawn.Count < config.TalentChoiceCount && candidates.Count > 0)
            {
                TalentConfigSO talent = DrawOne(candidates);

                if (talent == null)
                {
                    break;
                }

                drawn.Add(talent);
                candidates.Remove(talent);
            }

            return drawn;
        }

        /// <summary>A boss draw pays out above a floor, unless that leaves nothing to offer at all.</summary>
        private List<TalentConfigSO> GetCandidates(ExpeditionRewardsDataConfigSO config, bool isBossDraw)
        {
            IReadOnlyList<TalentConfigSO> pool = _talentsRepository.GetTalentPool(GameModeTypes.Expedition);
            var offerable = new List<TalentConfigSO>();
            var candidates = new List<TalentConfigSO>();

            foreach (TalentConfigSO talent in pool)
            {
                if (_talentManager.IsMaxLevel(talent.ObjectID))
                {
                    continue;
                }

                offerable.Add(talent);

                if (!isBossDraw || talent.Rarity >= config.BossMinTalentRarity)
                {
                    candidates.Add(talent);
                }
            }

            return candidates.Count > 0 ? candidates : offerable;
        }

        /// <summary>Only tiers still holding a candidate are weighed, so an exhausted one never wins.</summary>
        private TalentConfigSO DrawOne(List<TalentConfigSO> candidates)
        {
            var availableRarities = new List<TalentRarityConfigSO>();

            foreach (TalentRarityConfigSO rarityConfig in _talentsRepository.GetAllTalentRarityConfigs())
            {
                if (HasRarity(candidates, rarityConfig.Rarity))
                {
                    availableRarities.Add(rarityConfig);
                }
            }

            TalentRarityConfigSO rolled = GameUtils.RollWeighted(availableRarities,
                candidate => candidate.DrawWeight);

            // Unauthored or zero-weight tiers leave nothing to weigh, so the offer fills out flat.
            return rolled == null
                ? candidates[Random.Range(0, candidates.Count)]
                : GetRandomOfRarity(candidates, rolled.Rarity);
        }

        private static bool HasRarity(List<TalentConfigSO> candidates, TalentRarityTypes rarity)
        {
            foreach (TalentConfigSO candidate in candidates)
            {
                if (candidate.Rarity == rarity)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Counted first so the pick stays uniform without building a second list.</summary>
        private static TalentConfigSO GetRandomOfRarity(List<TalentConfigSO> candidates, TalentRarityTypes rarity)
        {
            int matches = 0;
            foreach (TalentConfigSO candidate in candidates)
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
            foreach (TalentConfigSO candidate in candidates)
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
