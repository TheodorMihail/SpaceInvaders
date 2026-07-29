using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Project
{
    public enum DropCategoryTypes
    {
        None,
        Powerup,
        Item
    }

    /// <summary>Weight for one category in the enemy-kill drop roll. Higher weight relative to
    /// the others means more likely; "None" is a normal entry here too, so it controls how often
    /// a kill drops nothing at all.</summary>
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

    /// <summary>Single authority for "what, if anything, drops on an enemy kill" - exactly one
    /// category wins the roll, so a kill can never drop both a powerup and an item.</summary>
    [CreateAssetMenu(fileName = "DropTableConfig", menuName = "SpaceInvaders/Data Config/Drop Table Config")]
    public class DropTableConfigSO : ScriptableObject, IRepositoryObject
    {
        [SerializeField] private List<DropCategoryWeightDTO> _categoryWeights;

        public virtual IReadOnlyList<DropCategoryWeightDTO> CategoryWeights => _categoryWeights;

        public string ObjectID => nameof(DropTableConfigSO);
    }
}
