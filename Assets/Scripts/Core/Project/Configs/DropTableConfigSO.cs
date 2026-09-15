using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceInvaders.Project
{
    public enum DropCategoryTypes
    {
        None,
        Powerup,
        Item
    }

    /// <summary>Weight for one category in the enemy-kill drop roll, relative to the others. "None"
    /// is an entry like any other and controls how often a kill drops nothing.</summary>
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

    /// <summary>One authority for "what, if anything, drops on an enemy kill" - exactly one category
    /// wins the roll, so a kill can never drop both a powerup and an item. A mode points at the table
    /// it rolls against, so what drops is authored rather than branched on.</summary>
    [CreateAssetMenu(fileName = "DropTableConfig", menuName = "SpaceInvaders/Drops/Drop Table Config")]
    public class DropTableConfigSO : ScriptableObject
    {
        [SerializeField] private List<DropCategoryWeightDTO> _categoryWeights;

        public virtual IReadOnlyList<DropCategoryWeightDTO> CategoryWeights => _categoryWeights;
    }
}
