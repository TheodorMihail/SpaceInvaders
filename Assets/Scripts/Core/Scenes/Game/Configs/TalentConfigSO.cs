using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
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

    /// <summary>What one step up the ladder costs and grants. Each level carries its own lines, so a
    /// ladder can change shape as it climbs rather than repeating one scaled number.</summary>
    [Serializable]
    public struct TalentLevelDTO
    {
        [SerializeField] private int _cost;
        [SerializeField] private List<TalentModifierDTO> _modifiers;

        public int Cost => _cost;
        public List<TalentModifierDTO> Modifiers => _modifiers ?? new List<TalentModifierDTO>();
    }

    /// <summary>
    /// One talent and every step of its ladder. The level count doubles as the cap on how many times
    /// it can be taken, so a one-level talent is a one-off and a five-level one is a full tree branch.
    /// </summary>
    [CreateAssetMenu(fileName = "TalentConfig", menuName = "SpaceInvaders/Talents/Talent Config")]
    public class TalentConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Talent Settings")]
        [Tooltip("Stored in the save, so renaming one drops the levels bought against it.")]
        [SerializeField] private string _talentId;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private List<TalentLevelDTO> _levels;

        public virtual string DisplayName => _displayName;
        public virtual Sprite Icon => _icon;
        public virtual IReadOnlyList<TalentLevelDTO> Levels => _levels;
        public virtual int MaxLevel => _levels?.Count ?? 0;

        public virtual string ObjectID => _talentId;

        /// <summary>Applied one level at a time, so what a level grants stays with that level.</summary>
        public virtual void ApplyLevel(ShipStats stats, int levelIndex)
        {
            if (_levels == null || levelIndex < 0 || levelIndex >= _levels.Count)
            {
                return;
            }

            foreach (TalentModifierDTO modifier in _levels[levelIndex].Modifiers)
            {
                stats.ApplyStatBonus(modifier.StatType, modifier.Value, modifier.ValueType);
            }
        }
    }
}
