using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    public enum TalentTypes
    {
        Health,
        MoveSpeed,
        FireRate,
        Damage,
        ProjectileSpeed
    }

    public abstract class TalentConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Talent Settings")]
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private List<TalentLevelDTO> _levels;

        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public IReadOnlyList<TalentLevelDTO> Levels => _levels;
        public int MaxLevel => _levels?.Count ?? 0;
        public string ObjectID => TalentType.ToString();

        public abstract TalentTypes TalentType { get; }

        public abstract void ApplyBonus(ShipStats stats, float totalBonusDelta);

        [Serializable]
        public struct TalentLevelDTO
        {
            [SerializeField] private int _cost;
            [SerializeField] private float _bonusDelta;

            public int Cost => _cost;
            public float BonusDelta => _bonusDelta;
        }
    }
}
