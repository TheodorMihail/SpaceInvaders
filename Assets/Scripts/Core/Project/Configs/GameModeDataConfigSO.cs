using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using UnityEngine;

namespace SpaceInvaders.Project
{
    public enum DropCategoryTypes
    {
        None,
        Powerup,
        Item
    }

    /// <summary>One category's weight against the others. "None" is an entry like any other, and is
    /// how often a kill drops nothing.</summary>
    [Serializable]
    public class DropCategoryWeightDTO
    {
        [SerializeField] private DropCategoryTypes _category;
        [SerializeField] private int _weight;

        public DropCategoryTypes Category => _category;
        public int Weight => _weight;

        public DropCategoryWeightDTO()
        {
        }

        public DropCategoryWeightDTO(DropCategoryTypes category, int weight)
        {
            _category = category;
            _weight = weight;
        }
    }

    /// <summary>What one mode drops, offers and pays. Held per mode, so tuning one never reaches into
    /// another's numbers.</summary>
    public abstract class GameModeDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Drops")]
        [Tooltip("Rolled on every enemy kill. Exactly one category wins, and an empty list drops nothing.")]
        [SerializeField] private List<DropCategoryWeightDTO> _dropWeights = new();

        [Header("Talents")]
        [Tooltip("What this mode offers. A talent in no pool exists for nobody.")]
        [SerializeField] private TalentsDataConfigSO _talentPool;

        [Header("Currency")]
        [Tooltip("Banked per point of score. The mode decides what its currency is called.")]
        [SerializeField] private float _currencyPerScore = 1f;

        [Tooltip("Flat amount on top of the level's own, for clearing a boss.")]
        [SerializeField] private int _bossCurrencyBonus;

        public virtual IReadOnlyList<DropCategoryWeightDTO> DropWeights => _dropWeights;
        public virtual TalentsDataConfigSO TalentPool => _talentPool;
        public virtual float CurrencyPerScore => _currencyPerScore;
        public virtual int BossCurrencyBonus => _bossCurrencyBonus;

        public abstract GameModeTypes Mode { get; }

        public virtual string ObjectID => Mode.ToString();
    }
}
