using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Expedition
{
    /// <summary>One shelf's worth of offers. Owns its cells; the screen only deals it a slice of the
    /// stock and handles the clicks.</summary>
    public class ExpeditionShopShelfUIComponent : MonoBehaviour
    {
        [Inject] private readonly ICustomFactory _factory;

        [SerializeField] private RectTransform _container;
        [SerializeField] private ExpeditionShopOfferUIComponent _cellPrefab;

        private readonly Dictionary<string, ExpeditionShopOfferUIComponent> _cells = new();

        public event Action<ItemSlotUIComponent, InventoryItemEntry> OnSlotClicked;
        public event Action<string> OnBuyClicked;

        private void OnDestroy()
        {
            Clear();
        }

        /// <summary>Cells are created outright rather than pooled, since object pooling is only bound
        /// in the game scene.</summary>
        public void Build(IEnumerable<ExpeditionShopOfferDTO> offers)
        {
            Clear();

            foreach (ExpeditionShopOfferDTO offer in offers)
            {
                ExpeditionShopOfferUIComponent cell = _factory.CreateFromPrefab(_cellPrefab, _container);

                cell.SetOffer(offer);
                cell.OnSlotClicked += HandleSlotClicked;
                cell.OnBuyClicked += HandleBuyClicked;

                _cells[offer.Item.InstanceId] = cell;
            }
        }

        /// <summary>Every cell is re-read after a purchase, since the balance gating them all has just
        /// changed. A shelf holding none of them simply does nothing.</summary>
        public void RefreshAffordability(Func<string, bool> isAffordable)
        {
            foreach (KeyValuePair<string, ExpeditionShopOfferUIComponent> cell in _cells)
            {
                cell.Value.SetAffordable(isAffordable(cell.Key));
            }
        }

        public void SetSold(string instanceId)
        {
            if (_cells.TryGetValue(instanceId, out ExpeditionShopOfferUIComponent cell))
            {
                cell.SetSold(true);
            }
        }

        private void Clear()
        {
            foreach (ExpeditionShopOfferUIComponent cell in _cells.Values)
            {
                cell.OnSlotClicked -= HandleSlotClicked;
                cell.OnBuyClicked -= HandleBuyClicked;
                Destroy(cell.gameObject);
            }

            _cells.Clear();
        }

        private void HandleSlotClicked(ItemSlotUIComponent slot, InventoryItemEntry item)
        {
            OnSlotClicked?.Invoke(slot, item);
        }

        private void HandleBuyClicked(string instanceId)
        {
            OnBuyClicked?.Invoke(instanceId);
        }
    }
}
