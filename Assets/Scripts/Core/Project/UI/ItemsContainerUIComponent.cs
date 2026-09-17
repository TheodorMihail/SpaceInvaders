using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using TMPro;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Project
{
    /// <summary>
    /// Holds a grid of item cells and the empty-state message, so every screen listing items builds
    /// them the same way. Owns the cells and their item mapping; the screen only supplies the items
    /// and handles clicks.
    /// </summary>
    public class ItemsContainerUIComponent : MonoBehaviour
    {
        [Inject] private readonly ICustomFactory _factory;

        [SerializeField] private ItemSlotUIComponent _itemCellPrefab;
        [SerializeField] private Transform _itemsContainer;

        [Tooltip("Shown only while nothing is in the container.")]
        [SerializeField] private TextMeshProUGUI _emptyText;

        private readonly Dictionary<string, ItemSlotUIComponent> _cells = new();

        public int ItemCount => _cells.Count;

        /// <summary>Carries the cell's rect as well, since a tooltip has to open against it, and the
        /// item itself, since a tooltip is handed what to show rather than looking it up.</summary>
        public event Action<RectTransform, InventoryItemEntry> OnItemClicked;

        public void Clear()
        {
            foreach (ItemSlotUIComponent cell in _cells.Values)
            {
                Destroy(cell.gameObject);
            }

            _cells.Clear();
            RefreshEmptyText();
        }

        /// <summary>Cells are created outright rather than pooled, since object pooling is only bound
        /// in the game scene.</summary>
        public void AddItem(InventoryItemEntry entry, ItemConfigSO config, ItemRarityConfigSO rarity)
        {
            if (entry == null || config == null || _cells.ContainsKey(entry.InstanceId))
            {
                return;
            }

            ItemSlotUIComponent cell = _factory.CreateFromPrefab(_itemCellPrefab, _itemsContainer);
            cell.SetItem(config, rarity);

            cell.OnClicked += () => OnItemClicked?.Invoke(cell.RectTransform, entry);

            _cells[entry.InstanceId] = cell;
            RefreshEmptyText();
        }

        public void RemoveItem(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId) || !_cells.TryGetValue(instanceId, out ItemSlotUIComponent cell))
            {
                return;
            }

            _cells.Remove(instanceId);
            Destroy(cell.gameObject);

            RefreshEmptyText();
        }

        public void SetEquipped(string instanceId, bool isEquipped)
        {
            if (TryGetCell(instanceId, out ItemSlotUIComponent cell))
            {
                cell.SetEquipped(isEquipped);
            }
        }

        public void SetSelected(string instanceId, bool isSelected)
        {
            if (TryGetCell(instanceId, out ItemSlotUIComponent cell))
            {
                cell.SetSelected(isSelected);
            }
        }

        private bool TryGetCell(string instanceId, out ItemSlotUIComponent cell)
        {
            cell = null;
            return !string.IsNullOrEmpty(instanceId) && _cells.TryGetValue(instanceId, out cell);
        }

        private void RefreshEmptyText()
        {
            if (_emptyText == null)
            {
                return;
            }

            _emptyText.gameObject.SetActive(_cells.Count == 0);
        }
    }
}
