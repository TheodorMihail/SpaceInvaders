using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using TMPro;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Project
{
    /// <summary>
    /// Holds a grid of item cells and the message shown while it is empty, so every screen listing
    /// items builds them the same way. Owns the cells it creates and the mapping back to their item,
    /// leaving the screen to say only what goes in and to answer clicks.
    /// </summary>
    public class ItemsContainerUIComponent : MonoBehaviour
    {
        [Inject] private readonly ICustomFactory _factory;

        [SerializeField] private ItemSlotComponent _itemCellPrefab;
        [SerializeField] private Transform _itemsContainer;

        [Tooltip("Shown only while nothing is in the container.")]
        [SerializeField] private TextMeshProUGUI _emptyText;

        private readonly Dictionary<string, ItemSlotComponent> _cells = new();

        public int ItemCount => _cells.Count;

        /// <summary>Carries the cell's rect as well, since a tooltip has to open against it.</summary>
        public event Action<RectTransform, string> OnItemClicked;

        public void Clear()
        {
            foreach (ItemSlotComponent cell in _cells.Values)
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

            ItemSlotComponent cell = _factory.CreateFromPrefab(_itemCellPrefab, _itemsContainer);
            cell.SetItem(config, rarity);

            string instanceId = entry.InstanceId;
            cell.OnClicked += () => OnItemClicked?.Invoke(cell.RectTransform, instanceId);

            _cells[instanceId] = cell;
            RefreshEmptyText();
        }

        public void RemoveItem(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId) || !_cells.TryGetValue(instanceId, out ItemSlotComponent cell))
            {
                return;
            }

            _cells.Remove(instanceId);
            Destroy(cell.gameObject);

            RefreshEmptyText();
        }

        public void SetEquipped(string instanceId, bool isEquipped)
        {
            if (TryGetCell(instanceId, out ItemSlotComponent cell))
            {
                cell.SetEquipped(isEquipped);
            }
        }

        public void SetSelected(string instanceId, bool isSelected)
        {
            if (TryGetCell(instanceId, out ItemSlotComponent cell))
            {
                cell.SetSelected(isSelected);
            }
        }

        private bool TryGetCell(string instanceId, out ItemSlotComponent cell)
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
