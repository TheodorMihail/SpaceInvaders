using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
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
        [SerializeField] private ItemRarities _rarity;
        [SerializeField] private int _dropWeight = 1;
        [SerializeField] private int _affixCount = 1;

        [Header("Presentation")]
        [SerializeField] private string _displayName;
        [SerializeField] private Color _displayColor = Color.white;
        [SerializeField] private ItemPickupBehaviourComponent _pickupPrefab;

        public virtual ItemRarities Rarity => _rarity;
        public virtual int DropWeight => _dropWeight;
        public virtual int AffixCount => _affixCount;
        public virtual string DisplayName => _displayName;
        public virtual Color DisplayColor => _displayColor;
        public virtual ItemPickupBehaviourComponent PickupPrefab => _pickupPrefab;

        public virtual string ObjectID => _rarity.ToString();
    }
}
