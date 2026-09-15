using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Expedition
{
    /// <summary>The perk catalogue and the rules a reward screen draws it by.</summary>
    [CreateAssetMenu(fileName = "ExpeditionPerksDataConfig", menuName = "SpaceInvaders/Expedition/Expedition Perks Data Config")]
    public class ExpeditionPerksDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Catalogue")]
        [SerializeField] private List<ExpeditionPerkConfigSO> _perkConfigs = new();

        [Tooltip("Per-tier draw weight and presentation. A tier with no perks left to offer is skipped.")]
        [SerializeField] private List<ExpeditionPerkRarityConfigSO> _rarityConfigs = new();

        [Header("Draw")]
        [Tooltip("Cards offered on one draw.")]
        [SerializeField] private int _perkChoiceCount = 3;

        [Tooltip("Draws awarded for clearing a level, each offered on its own screen.")]
        [SerializeField] private int _perkRewardCount = 1;

        [SerializeField] private int _bossPerkRewardCount = 2;

        [Tooltip("Lowest rarity a boss draw may offer, so a boss never pays out in commons.")]
        [SerializeField] private ExpeditionPerkRarityTypes _bossMinPerkRarity = ExpeditionPerkRarityTypes.Rare;

        public virtual List<ExpeditionPerkConfigSO> PerkConfigs => _perkConfigs;
        public virtual List<ExpeditionPerkRarityConfigSO> RarityConfigs => _rarityConfigs;
        public virtual int PerkChoiceCount => _perkChoiceCount;
        public virtual int PerkRewardCount => _perkRewardCount;
        public virtual int BossPerkRewardCount => _bossPerkRewardCount;
        public virtual ExpeditionPerkRarityTypes BossMinPerkRarity => _bossMinPerkRarity;

        public virtual string ObjectID => nameof(ExpeditionPerksDataConfigSO);
    }
}
