using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Expedition
{
    [AddressablePath("Screens/ExpeditionMapScreenView")]
    public class ExpeditionMapScreenView : View
    {
        [SerializeField] private ExpeditionMapUIComponent _map;
        [SerializeField] private Button _backButton;

        public event Action<int> OnNodeClicked;
        public event Action OnBackButtonClicked;

        public void Initialize(IReadOnlyList<ExpeditionNodeEntry> nodes, int currentNodeId)
        {
            _map.Build(nodes, currentNodeId);
        }

        /// <summary>After a move: the map keeps its cells and follows the player to the new depth.</summary>
        public void Refresh(IReadOnlyList<ExpeditionNodeEntry> nodes, int currentNodeId)
        {
            _map.Refresh(nodes, currentNodeId);
        }

        private void Awake()
        {
            _backButton.onClick.AddListener(() => OnBackButtonClicked?.Invoke());
            _map.OnNodeClicked += HandleNodeClicked;
        }

        private void OnDestroy()
        {
            _map.OnNodeClicked -= HandleNodeClicked;
        }

        private void HandleNodeClicked(int nodeId)
        {
            OnNodeClicked?.Invoke(nodeId);
        }
    }
}
