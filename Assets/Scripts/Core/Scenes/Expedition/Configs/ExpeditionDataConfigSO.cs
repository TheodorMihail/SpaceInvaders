using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using UnityEngine;

namespace SpaceInvaders.Scenes.Expedition
{
    /// <summary>One node type's candidate levels over a stretch of the map.</summary>
    [Serializable]
    public struct ExpeditionLevelPoolDTO
    {
        [SerializeField] private ExpeditionNodeTypes _nodeType;
        [SerializeField] private int _minDepth;
        [SerializeField] private int _maxDepth;
        [SerializeField] private List<LevelConfigSO> _levels;

        public ExpeditionNodeTypes NodeType => _nodeType;
        public int MinDepth => _minDepth;
        public int MaxDepth => _maxDepth;
        public List<LevelConfigSO> Levels => _levels ?? new List<LevelConfigSO>();
    }

    /// <summary>How likely a node type is on a row that is not already fixed.</summary>
    [Serializable]
    public struct ExpeditionNodeWeightDTO
    {
        [SerializeField] private ExpeditionNodeTypes _nodeType;
        [SerializeField] private float _weight;

        public ExpeditionNodeTypes NodeType => _nodeType;
        public float Weight => _weight;
    }

    [CreateAssetMenu(fileName = "ExpeditionDataConfig", menuName = "SpaceInvaders/Expedition/Expedition Data Config")]
    public class ExpeditionDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Map Shape")]
        [Tooltip("Rows including the start and the mega boss.")]
        [SerializeField] private int _depth = 10;
        [SerializeField] private int _minBranchWidth = 2;
        [SerializeField] private int _maxBranchWidth = 3;

        [Tooltip("Depths that are always a boss. The last row is always the mega boss.")]
        [SerializeField] private List<int> _bossDepths = new();

        [Tooltip("Chance a node also links to a neighbour of its nearest next-depth node, which is what " +
                 "gives every branch a real choice rather than one forced path.")]
        [Range(0f, 1f)]
        [SerializeField] private float _extraLinkChance = 0.5f;

        [Header("Placement Rules")]
        [Tooltip("Earliest depth a boss may appear at, however it was rolled or authored.")]
        [SerializeField] private int _minBossDepth = 3;

        [Tooltip("Earliest depth a shop may appear at, so the run never opens on one.")]
        [SerializeField] private int _minShopDepth = 2;

        [Tooltip("Drawn for any row that is not the start, a boss, or the mega boss.")]
        [SerializeField] private List<ExpeditionNodeWeightDTO> _nodeTypeWeights = new();

        [Header("Levels")]
        [SerializeField] private List<ExpeditionLevelPoolDTO> _levelPools = new();

        public virtual int Depth => _depth;
        public virtual int MinBranchWidth => _minBranchWidth;
        public virtual int MaxBranchWidth => _maxBranchWidth;
        public virtual IReadOnlyList<int> BossDepths => _bossDepths;
        public virtual float ExtraLinkChance => _extraLinkChance;
        public virtual int MinBossDepth => _minBossDepth;
        public virtual int MinShopDepth => _minShopDepth;
        public virtual IReadOnlyList<ExpeditionNodeWeightDTO> NodeTypeWeights => _nodeTypeWeights;
        public virtual IReadOnlyList<ExpeditionLevelPoolDTO> LevelPools => _levelPools;

        public virtual string ObjectID => nameof(ExpeditionDataConfigSO);
    }
}
