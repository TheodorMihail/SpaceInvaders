using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    public enum TalentRarityTypes
    {
        Common,
        Rare,
        Epic
    }

    /// <summary>Per-rarity tuning: how often the tier is drawn, and how it presents. Only a mode that
    /// offers talents rather than selling them reads the weight.</summary>
    [CreateAssetMenu(fileName = "TalentRarityConfig", menuName = "SpaceInvaders/Talents/Talent Rarity Config")]
    public class TalentRarityConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Rarity Settings")]
        [SerializeField] private TalentRarityTypes _rarity;

        [Tooltip("Weighed against the other tiers that still have a talent left to offer.")]
        [SerializeField] private int _drawWeight = 1;

        [Header("Presentation")]
        [SerializeField] private string _displayName;
        [SerializeField] private Color _displayColor = Color.white;

        public virtual TalentRarityTypes Rarity => _rarity;
        public virtual int DrawWeight => _drawWeight;
        public virtual string DisplayName => _displayName;
        public virtual Color DisplayColor => _displayColor;

        public virtual string ObjectID => _rarity.ToString();
    }
}
