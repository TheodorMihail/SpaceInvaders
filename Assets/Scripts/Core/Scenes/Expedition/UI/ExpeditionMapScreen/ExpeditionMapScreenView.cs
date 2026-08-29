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

        public void Initialize(IReadOnlyList<ExpeditionNodeEntry> nodes)
        {
            _map.Build(nodes);
        }

        /// <summary>After a move: the map keeps its cells and its scroll position.</summary>
        public void Refresh(IReadOnlyList<ExpeditionNodeEntry> nodes)
        {
            _map.Refresh(nodes);
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
