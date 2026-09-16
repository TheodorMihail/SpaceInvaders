using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using UnityEngine;

namespace SpaceInvaders.Scenes.Expedition
{
    /// <summary>How a cleared level pays out in talents. What it can offer is the mode's talent pool.</summary>
    [CreateAssetMenu(fileName = "ExpeditionRewardsDataConfig", menuName = "SpaceInvaders/Expedition/Expedition Rewards Data Config")]
    public class ExpeditionRewardsDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Draw")]
        [Tooltip("Cards offered on one draw.")]
        [SerializeField] private int _talentChoiceCount = 3;

        [Tooltip("Draws awarded for clearing a level, each offered on its own screen.")]
        [SerializeField] private int _talentRewardCount = 1;

        [SerializeField] private int _bossTalentRewardCount = 2;

        [Tooltip("Lowest rarity a boss draw may offer, so a boss never pays out in commons.")]
        [SerializeField] private TalentRarityTypes _bossMinTalentRarity = TalentRarityTypes.Rare;

        public virtual int TalentChoiceCount => _talentChoiceCount;
        public virtual int TalentRewardCount => _talentRewardCount;
        public virtual int BossTalentRewardCount => _bossTalentRewardCount;
        public virtual TalentRarityTypes BossMinTalentRarity => _bossMinTalentRarity;

        public virtual string ObjectID => nameof(ExpeditionRewardsDataConfigSO);
    }
}
