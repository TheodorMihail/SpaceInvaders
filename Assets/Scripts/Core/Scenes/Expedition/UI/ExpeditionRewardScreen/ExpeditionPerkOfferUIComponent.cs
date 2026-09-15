using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Expedition
{
    /// <summary>The row of cards on offer. Built once per screen, since a draw is never rerolled.</summary>
    public class ExpeditionPerkOfferUIComponent : MonoBehaviour
    {
        [Inject] private readonly ICustomFactory _factory;

        [SerializeField] private RectTransform _container;
        [SerializeField] private ExpeditionPerkCardComponent _cardPrefab;

        private readonly List<ExpeditionPerkCardComponent> _cards = new();

        public event Action<string> OnPerkClicked;

        public void Build(IEnumerable<(ExpeditionPerkConfigSO perk, ExpeditionPerkRarityConfigSO rarity)> choices)
        {
            Clear();

            foreach ((ExpeditionPerkConfigSO perk, ExpeditionPerkRarityConfigSO rarity) choice in choices)
            {
                ExpeditionPerkCardComponent card = _factory.CreateFromPrefab(_cardPrefab, _container);

                card.SetPerk(choice.perk, choice.rarity);
                card.OnClicked += HandlePerkClicked;

                _cards.Add(card);
            }
        }

        private void OnDestroy()
        {
            Clear();
        }

        private void Clear()
        {
            foreach (ExpeditionPerkCardComponent card in _cards)
            {
                card.OnClicked -= HandlePerkClicked;
                Destroy(card.gameObject);
            }

            _cards.Clear();
        }

        private void HandlePerkClicked(string perkId)
        {
            OnPerkClicked?.Invoke(perkId);
        }
    }
}
