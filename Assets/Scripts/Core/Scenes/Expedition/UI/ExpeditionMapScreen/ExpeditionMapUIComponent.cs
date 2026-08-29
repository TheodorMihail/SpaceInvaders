using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Expedition
{
    /// <summary>One node type's icon, authored rather than resolved, since nothing else needs them.</summary>
    [Serializable]
    public struct ExpeditionNodeIconDTO
    {
        [SerializeField] private ExpeditionNodeTypes _nodeType;
        [SerializeField] private Sprite _icon;

        public ExpeditionNodeTypes NodeType => _nodeType;
        public Sprite Icon => _icon;
    }

    /// <summary>
    /// Lays the map out inside a scroll rect's content: one column per depth, branches spread across
    /// the height. Depth 0 sits at the left, so the map reads rightward toward the mega boss.
    /// </summary>
    public class ExpeditionMapUIComponent : MonoBehaviour
    {
        [Inject] private readonly ICustomFactory _factory;

        [SerializeField] private RectTransform _content;
        [SerializeField] private ExpeditionNodeUIComponent _nodePrefab;
        [SerializeField] private ExpeditionPathUIComponent _pathPrefab;

        [Header("Layout")]
        [Tooltip("Distance between one depth and the next, left to right.")]
        [SerializeField] private float _depthSpacing = 260f;

        [Tooltip("Distance between the branches sharing a depth.")]
        [SerializeField] private float _branchSpacing = 220f;

        [Tooltip("Space kept before the first depth and after the last.")]
        [SerializeField] private float _horizontalPadding = 160f;

        [Header("Icons")]
        [SerializeField] private List<ExpeditionNodeIconDTO> _nodeIcons = new();

        private readonly Dictionary<int, ExpeditionNodeUIComponent> _nodeComponents = new();
        private readonly Dictionary<int, Vector2> _nodePositions = new();

        public event Action<int> OnNodeClicked;

        public void Build(IReadOnlyList<ExpeditionNodeEntry> nodes)
        {
            Clear();

            if (nodes == null || nodes.Count == 0)
            {
                return;
            }

            ResizeContent(nodes);
            BuildPositions(nodes);

            // Paths first, so a node is never drawn underneath a link.
            BuildPaths(nodes);
            BuildNodes(nodes);
        }

        /// <summary>Updates the existing cells in place. Rebuilding would drop the scroll position, and
        /// walking the map changes nothing about its shape.</summary>
        public void Refresh(IReadOnlyList<ExpeditionNodeEntry> nodes)
        {
            foreach (ExpeditionNodeEntry node in nodes)
            {
                if (_nodeComponents.TryGetValue(node.Id, out ExpeditionNodeUIComponent component))
                {
                    component.RefreshState(node);
                }
            }
        }

        private void OnDestroy()
        {
            Clear();
        }

        private void Clear()
        {
            foreach (ExpeditionNodeUIComponent node in _nodeComponents.Values)
            {
                node.OnClicked -= HandleNodeClicked;
            }

            _nodeComponents.Clear();
            _nodePositions.Clear();

            for (int i = _content.childCount - 1; i >= 0; i--)
            {
                Destroy(_content.GetChild(i).gameObject);
            }
        }

        private void ResizeContent(IReadOnlyList<ExpeditionNodeEntry> nodes)
        {
            int depth = 0;
            foreach (ExpeditionNodeEntry node in nodes)
            {
                depth = Mathf.Max(depth, node.Depth);
            }

            _content.sizeDelta = new Vector2(depth * _depthSpacing + _horizontalPadding * 2f, _content.sizeDelta.y);
        }

        private void BuildPositions(IReadOnlyList<ExpeditionNodeEntry> nodes)
        {
            Dictionary<int, int> branchCounts = GetBranchCounts(nodes);

            foreach (ExpeditionNodeEntry node in nodes)
            {
                int branchCount = branchCounts[node.Depth];
                float offset = (branchCount - 1) * 0.5f;
                float x = _horizontalPadding + node.Depth * _depthSpacing;
                float y = (node.Column - offset) * _branchSpacing;

                _nodePositions[node.Id] = new Vector2(x, y);
            }
        }

        private static Dictionary<int, int> GetBranchCounts(IReadOnlyList<ExpeditionNodeEntry> nodes)
        {
            var counts = new Dictionary<int, int>();

            foreach (ExpeditionNodeEntry node in nodes)
            {
                counts.TryGetValue(node.Depth, out int count);
                counts[node.Depth] = Mathf.Max(count, node.Column + 1);
            }

            return counts;
        }

        private void BuildPaths(IReadOnlyList<ExpeditionNodeEntry> nodes)
        {
            foreach (ExpeditionNodeEntry node in nodes)
            {
                foreach (int nextNodeId in node.NextNodeIds)
                {
                    if (!_nodePositions.TryGetValue(nextNodeId, out Vector2 toPosition))
                    {
                        continue;
                    }

                    ExpeditionPathUIComponent path = _factory.CreateFromPrefab(_pathPrefab, _content);
                    path.Initialize(_nodePositions[node.Id], toPosition);
                }
            }
        }

        private void BuildNodes(IReadOnlyList<ExpeditionNodeEntry> nodes)
        {
            foreach (ExpeditionNodeEntry node in nodes)
            {
                ExpeditionNodeUIComponent component = _factory.CreateFromPrefab(_nodePrefab, _content);
                ((RectTransform)component.transform).anchoredPosition = _nodePositions[node.Id];

                component.Initialize(node, GetIcon(node.NodeType));
                component.OnClicked += HandleNodeClicked;

                _nodeComponents[node.Id] = component;
            }
        }

        private Sprite GetIcon(string nodeType)
        {
            foreach (ExpeditionNodeIconDTO icon in _nodeIcons)
            {
                if (icon.NodeType.ToString() == nodeType)
                {
                    return icon.Icon;
                }
            }

            return null;
        }

        private void HandleNodeClicked(int nodeId)
        {
            OnNodeClicked?.Invoke(nodeId);
        }
    }
}
