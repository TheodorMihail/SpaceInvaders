using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>Only what ship stats already carry, so a talent needs no gameplay change of its own.</summary>
    public enum TalentEffectTypes
    {
        None,
        ExtraProjectile,
        UnlimitedAmmo
    }

    /// <summary>One stat line granted by a talent level.</summary>
    [Serializable]
    public struct TalentModifierDTO
    {
        [SerializeField] private ShipUpgradableStatTypes _statType;
        [SerializeField] private ShipStatValueTypes _valueType;
        [SerializeField] private float _value;

        public ShipUpgradableStatTypes StatType => _statType;
        public ShipStatValueTypes ValueType => _valueType;
        public float Value => _value;

        public TalentModifierDTO(ShipUpgradableStatTypes statType, ShipStatValueTypes valueType, float value)
        {
            _statType = statType;
            _valueType = valueType;
            _value = value;
        }
    }

    /// <summary>What one step up the ladder costs and grants. Lines are per level, so a ladder can
    /// change shape as it climbs.</summary>
    [Serializable]
    public struct TalentLevelDTO
    {
        [Tooltip("Ignored by a mode that grants talents rather than selling them.")]
        [SerializeField] private int _cost;
        [SerializeField] private List<TalentModifierDTO> _modifiers;

        [Tooltip("Granted on top of the lines. None leaves the level a pure stat step.")]
        [SerializeField] private TalentEffectTypes _effect;

        [Tooltip("Extra projectiles and the angle they spread over. Read only by the matching effect.")]
        [SerializeField] private int _extraProjectileCount;
        [SerializeField] private float _spreadAngleDegrees;

        public int Cost => _cost;
        public List<TalentModifierDTO> Modifiers => _modifiers ?? new List<TalentModifierDTO>();
        public TalentEffectTypes Effect => _effect;
        public int ExtraProjectileCount => _extraProjectileCount;
        public float SpreadAngleDegrees => _spreadAngleDegrees;

        public TalentLevelDTO(int cost, List<TalentModifierDTO> modifiers,
            TalentEffectTypes effect = TalentEffectTypes.None, int extraProjectileCount = 0,
            float spreadAngleDegrees = 0f)
        {
            _cost = cost;
            _modifiers = modifiers;
            _effect = effect;
            _extraProjectileCount = extraProjectileCount;
            _spreadAngleDegrees = spreadAngleDegrees;
        }
    }

    /// <summary>One talent and every step of its ladder. The level count is also the cap on how many
    /// times it can be taken.</summary>
    [CreateAssetMenu(fileName = "TalentConfig", menuName = "SpaceInvaders/Talents/Talent Config")]
    public class TalentConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Talent Settings")]
        [Tooltip("Stored in the save, so renaming one drops the levels bought against it.")]
        [SerializeField] private string _talentId;
        [SerializeField] private string _displayName;

        [Tooltip("Shown under the stat lines. Needed for a level whose effect no stat line describes.")]
        [SerializeField] [TextArea] private string _description;

        [Tooltip("Read only where talents are drawn rather than picked from a tree.")]
        [SerializeField] private TalentRarityTypes _rarity;
        [SerializeField] private Sprite _icon;
        [SerializeField] private List<TalentLevelDTO> _levels;

        public virtual string DisplayName => _displayName;
        public virtual string Description => _description;
        public virtual TalentRarityTypes Rarity => _rarity;
        public virtual Sprite Icon => _icon;
        public virtual IReadOnlyList<TalentLevelDTO> Levels => _levels;
        public virtual int MaxLevel => _levels?.Count ?? 0;

        public virtual string ObjectID => _talentId;

        /// <summary>Applied to a ship that was just built, so none of it is ever reversed.</summary>
        public virtual void ApplyLevel(ShipStats stats, int levelIndex)
        {
            if (_levels == null || levelIndex < 0 || levelIndex >= _levels.Count)
            {
                return;
            }

            TalentLevelDTO level = _levels[levelIndex];

            foreach (TalentModifierDTO modifier in level.Modifiers)
            {
                stats.ApplyStatBonus(modifier.StatType, modifier.Value, modifier.ValueType);
            }

            switch (level.Effect)
            {
                case TalentEffectTypes.ExtraProjectile:
                {
                    stats.UpdateShotSpread(level.ExtraProjectileCount, level.SpreadAngleDegrees);
                    break;
                }
                case TalentEffectTypes.UnlimitedAmmo:
                {
                    stats.SetUnlimitedAmmo(true);
                    break;
                }
            }
        }
    }
}
