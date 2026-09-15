using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Expedition
{
    /// <summary>
    /// Per-rarity tuning: how often the tier is drawn, and how it is presented on a card.
    /// </summary>
    [CreateAssetMenu(fileName = "ExpeditionPerkRarityConfig", menuName = "SpaceInvaders/Expedition/Expedition Perk Rarity Config")]
    public class ExpeditionPerkRarityConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Rarity Settings")]
        [SerializeField] private ExpeditionPerkRarityTypes _rarity;

        [Tooltip("Weighed against the other tiers that still have a perk left to offer.")]
        [SerializeField] private int _drawWeight = 1;

        [Header("Presentation")]
        [SerializeField] private string _displayName;
        [SerializeField] private Color _displayColor = Color.white;

        public virtual ExpeditionPerkRarityTypes Rarity => _rarity;
        public virtual int DrawWeight => _drawWeight;
        public virtual string DisplayName => _displayName;
        public virtual Color DisplayColor => _displayColor;

        public virtual string ObjectID => _rarity.ToString();
    }
}
