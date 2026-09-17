using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using UnityEngine;

namespace SpaceInvaders.Scenes.Expedition
{
    /// <summary>What a tier costs and how often it is stocked. Kept apart from the drop weights so the
    /// shelf can be richer than what a kill pays out.</summary>
    [Serializable]
    public class ExpeditionShopRarityDTO
    {
        [SerializeField] private ItemRarityTypes _rarity;
        [SerializeField] private int _price;

        [Tooltip("Weighed against the other tiers when a shelf slot is filled.")]
        [SerializeField] private int _stockWeight;

        public ItemRarityTypes Rarity => _rarity;
        public int Price => _price;
        public int StockWeight => _stockWeight;

        public ExpeditionShopRarityDTO()
        {
        }

        public ExpeditionShopRarityDTO(ItemRarityTypes rarity, int price, int stockWeight)
        {
            _rarity = rarity;
            _price = price;
            _stockWeight = stockWeight;
        }
    }

    /// <summary>What a shop node stocks and charges.</summary>
    [CreateAssetMenu(fileName = "ExpeditionShopDataConfig", menuName = "SpaceInvaders/Expedition/Expedition Shop Data Config")]
    public class ExpeditionShopDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Stock")]
        [Tooltip("Items rolled once per shop node, dealt across however many shelves the screen has.")]
        [SerializeField] private int _offerCount = 6;

        [SerializeField] private List<ExpeditionShopRarityDTO> _rarities = new();

        [Header("Repair")]
        [Tooltip("Scrap charged per percent of health restored, so a full mend costs the same whatever the hull.")]
        [SerializeField] private float _scrapPerHealthPercent = 3f;

        public virtual int OfferCount => _offerCount;
        public virtual IReadOnlyList<ExpeditionShopRarityDTO> Rarities => _rarities;
        public virtual float ScrapPerHealthPercent => _scrapPerHealthPercent;

        public virtual string ObjectID => nameof(ExpeditionShopDataConfigSO);
    }
}
