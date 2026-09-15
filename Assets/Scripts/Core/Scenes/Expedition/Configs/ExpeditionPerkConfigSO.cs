using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using UnityEngine;

namespace SpaceInvaders.Scenes.Expedition
{
    public enum ExpeditionPerkRarityTypes
    {
        Common,
        Rare,
        Epic
    }

    /// <summary>Only what ship stats already carry, so a perk needs no gameplay change of its own.</summary>
    public enum ExpeditionPerkEffectTypes
    {
        None,
        ExtraProjectile,
        UnlimitedAmmo
    }

    /// <summary>One stat line on a card. Several on one perk is what makes a trade-off authorable.</summary>
    [Serializable]
    public struct ExpeditionPerkModifierDTO
    {
        [SerializeField] private ShipUpgradableStatTypes _statType;
        [SerializeField] private ShipStatValueTypes _valueType;
        [SerializeField] private float _value;

        public ShipUpgradableStatTypes StatType => _statType;
        public ShipStatValueTypes ValueType => _valueType;
        public float Value => _value;

        public ExpeditionPerkModifierDTO(ShipUpgradableStatTypes statType, ShipStatValueTypes valueType, float value)
        {
            _statType = statType;
            _valueType = valueType;
            _value = value;
        }
    }

    /// <summary>
    /// One offered card. Magnitudes are authored rather than rolled, so the same perk always reads the
    /// same way and rarity carries the variance instead.
    /// </summary>
    [CreateAssetMenu(fileName = "ExpeditionPerkConfig", menuName = "SpaceInvaders/Expedition/Expedition Perk Config")]
    public class ExpeditionPerkConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Perk Settings")]
        [Tooltip("Stored in the save, so renaming one drops it from expeditions already under way.")]
        [SerializeField] private string _perkId;
        [SerializeField] private string _displayName;

        [Tooltip("Shown under the stat lines. Needed for a perk whose effect no stat line describes.")]
        [SerializeField] [TextArea] private string _description;
        [SerializeField] private ExpeditionPerkRarityTypes _rarity;

        [Tooltip("Off for a perk a second copy would do nothing for, which is then never offered twice.")]
        [SerializeField] private bool _isStackable = true;

        [Header("Bonuses")]
        [Tooltip("Applied at every spawn for the rest of the expedition.")]
        [SerializeField] private List<ExpeditionPerkModifierDTO> _modifiers = new();

        [Header("Effect")]
        [Tooltip("Applied on top of the modifiers. None leaves the perk a pure stat card.")]
        [SerializeField] private ExpeditionPerkEffectTypes _effect;

        [Tooltip("Extra projectiles and the angle they spread over. Read only by the matching effect.")]
        [SerializeField] private int _extraProjectileCount = 1;
        [SerializeField] private float _spreadAngleDegrees = 15f;

        public virtual string DisplayName => _displayName;
        public virtual string Description => _description;
        public virtual ExpeditionPerkRarityTypes Rarity => _rarity;
        public virtual bool IsStackable => _isStackable;
        public virtual IReadOnlyList<ExpeditionPerkModifierDTO> Modifiers => _modifiers;
        public virtual ExpeditionPerkEffectTypes Effect => _effect;
        public virtual int ExtraProjectileCount => _extraProjectileCount;
        public virtual float SpreadAngleDegrees => _spreadAngleDegrees;

        public virtual string ObjectID => _perkId;

        /// <summary>Applied to a ship that was just built, so none of this is ever reversed.</summary>
        public virtual void ApplyTo(ShipStats stats)
        {
            foreach (ExpeditionPerkModifierDTO modifier in Modifiers)
            {
                stats.ApplyStatBonus(modifier.StatType, modifier.Value, modifier.ValueType);
            }

            switch (Effect)
            {
                case ExpeditionPerkEffectTypes.ExtraProjectile:
                {
                    stats.UpdateShotSpread(ExtraProjectileCount, SpreadAngleDegrees);
                    break;
                }
                case ExpeditionPerkEffectTypes.UnlimitedAmmo:
                {
                    stats.SetUnlimitedAmmo(true);
                    break;
                }
            }
        }
    }
}
