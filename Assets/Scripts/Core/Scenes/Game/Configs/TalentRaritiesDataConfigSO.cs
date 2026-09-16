using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>The rarity tiers, shared by every mode. Which talents a mode offers is on the mode.</summary>
    [CreateAssetMenu(fileName = "TalentRaritiesDataConfig", menuName = "SpaceInvaders/Data Config/Talent Rarities Data Config")]
    public class TalentRaritiesDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Rarities")]
        [SerializeField] private List<TalentRarityConfigSO> _rarityConfigs = new();

        public virtual List<TalentRarityConfigSO> RarityConfigs => _rarityConfigs;

        public string ObjectID => nameof(TalentRaritiesDataConfigSO);
    }
}
