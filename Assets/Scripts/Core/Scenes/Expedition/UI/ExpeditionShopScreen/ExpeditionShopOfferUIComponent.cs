using System;
using SpaceInvaders.Project;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Expedition
{
    /// <summary>One thing on the shelf and everything a cell needs to present it.</summary>
    public readonly struct ExpeditionShopOfferDTO
    {
        public InventoryItemEntry Item { get; }
        public ItemConfigSO Config { get; }
        public ItemRarityConfigSO Rarity { get; }
        public int Price { get; }
        public bool IsSold { get; }

        public ExpeditionShopOfferDTO(InventoryItemEntry item, ItemConfigSO config, ItemRarityConfigSO rarity,
            int price, bool isSold)
        {
            Item = item;
            Config = config;
            Rarity = rarity;
            Price = price;
            IsSold = isSold;
        }
    }

    /// <summary>One offered item. The slot is clicked to inspect and the button to buy, so reading an
    /// item never risks spending on it.</summary>
    public class ExpeditionShopOfferUIComponent : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private ItemSlotUIComponent _slot;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private TextMeshProUGUI _buyButtonText;

        [Header("Strings")]
        [SerializeField] private string _priceString = "{0}";
        [SerializeField] private string _buyString = "BUY";
        [SerializeField] private string _soldString = "SOLD";

        [Tooltip("Faded rather than hidden, so the shelf keeps its shape once things start selling.")]
        [SerializeField] private float _soldAlpha = 0.4f;

        private InventoryItemEntry _item;
        private bool _isSold;

        /// <summary>Carries the slot, since the tooltip opens against it and marks it selected.</summary>
        public event Action<ItemSlotUIComponent, InventoryItemEntry> OnSlotClicked;
        public event Action<string> OnBuyClicked;

        private void Awake()
        {
            _slot.OnClicked += HandleSlotClicked;
            _buyButton.onClick.AddListener(HandleBuyClicked);
        }

        private void OnDestroy()
        {
            _slot.OnClicked -= HandleSlotClicked;
        }

        public void SetOffer(ExpeditionShopOfferDTO offer)
        {
            _item = offer.Item;
            _priceText.text = string.Format(_priceString, offer.Price);
            _slot.SetItem(offer.Config, offer.Rarity);

            SetSold(offer.IsSold);
        }

        public void SetSold(bool isSold)
        {
            _isSold = isSold;
            _canvasGroup.alpha = isSold ? _soldAlpha : 1f;
            _buyButton.interactable = !isSold;
            _buyButtonText.text = isSold ? _soldString : _buyString;
        }

        /// <summary>An item out of reach keeps its price, so the balance reads as the reason rather
        /// than the cell looking spent.</summary>
        public void SetAffordable(bool isAffordable)
        {
            _buyButton.interactable = !_isSold && isAffordable;
        }

        private void HandleSlotClicked()
        {
            OnSlotClicked?.Invoke(_slot, _item);
        }

        private void HandleBuyClicked()
        {
            OnBuyClicked?.Invoke(_item?.InstanceId);
        }
    }
}
