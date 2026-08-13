using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Project
{
    /// <summary>
    /// Per-rarity tuning: how often the tier drops, how many affixes its items roll,
    /// and how it is presented.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemRarityConfig", menuName = "SpaceInvaders/Items/Item Rarity Config")]
    public class ItemRarityConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Rarity Settings")]
        [SerializeField] private ItemRarityTypes _rarity;
        [SerializeField] private int _dropWeight = 1;
        [SerializeField] private int _affixCount = 1;

        [Header("Presentation")]
        [SerializeField] private string _displayName;
        [SerializeField] private Color _displayColor = Color.white;

        [Tooltip("Applied to the shared pickup prefab, so each tier still drops with its own look.")]
        [SerializeField] private Sprite _icon;

        public virtual ItemRarityTypes Rarity => _rarity;
        public virtual int DropWeight => _dropWeight;
        public virtual int AffixCount => _affixCount;
        public virtual string DisplayName => _displayName;
        public virtual Color DisplayColor => _displayColor;
        public virtual Sprite Icon => _icon;

        public virtual string ObjectID => _rarity.ToString();
    }
}
