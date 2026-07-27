using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "TalentConfig", menuName = "SpaceInvaders/Talents/Talent Config")]
    public class TalentConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Talent Settings")]
        [SerializeField] private ShipUpgradableStatTypes _talentType;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private List<TalentLevelDTO> _levels;

        public ShipUpgradableStatTypes TalentType => _talentType;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public IReadOnlyList<TalentLevelDTO> Levels => _levels;
        public int MaxLevel => _levels?.Count ?? 0;
        public string ObjectID => _talentType.ToString();

        public void ApplyBonus(ShipStats stats, float totalBonusDelta)
        {
            stats.ApplyStatBonus(TalentType, totalBonusDelta);
        }

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
