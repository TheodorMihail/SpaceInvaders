using System;
using SpaceInvaders.Project;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Expedition
{
    /// <summary>One node on the map. Owns its own visuals; the map only hands it an entry.</summary>
    public class ExpeditionNodeUIComponent : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _iconImage;

        [Header("State Colors")]
        [SerializeField] private Color _lockedColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] private Color _availableColor = Color.white;
        [SerializeField] private Color _visitedColor = new Color(0.6f, 0.9f, 0.6f, 1f);

        private int _nodeId;

        public event Action<int> OnClicked;

        public void Initialize(ExpeditionNodeEntry entry, Sprite icon)
        {
            _nodeId = entry.Id;
            _iconImage.sprite = icon;

            RefreshState(entry);
        }

        /// <summary>Only what walking the map changes, so a move never rebuilds the cell.</summary>
        public void RefreshState(ExpeditionNodeEntry entry)
        {
            bool isAvailable = entry.State == ExpeditionNodeStateTypes.Available.ToString();
            bool isVisited = entry.State == ExpeditionNodeStateTypes.Visited.ToString();

            _iconImage.color = isVisited ? _visitedColor : isAvailable ? _availableColor : _lockedColor;
            _button.interactable = isAvailable;
        }

        private void Awake()
        {
            _button.onClick.AddListener(() => OnClicked?.Invoke(_nodeId));
        }
    }
}
