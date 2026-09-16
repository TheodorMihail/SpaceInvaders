using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Expedition
{
    /// <summary>One offered talent and everything the card needs to present it.</summary>
    public readonly struct ExpeditionTalentOfferDTO
    {
        public TalentConfigSO Talent { get; }
        public TalentRarityConfigSO Rarity { get; }
        public int OwnedLevel { get; }

        public ExpeditionTalentOfferDTO(TalentConfigSO talent, TalentRarityConfigSO rarity, int ownedLevel)
        {
            Talent = talent;
            Rarity = rarity;
            OwnedLevel = ownedLevel;
        }
    }

    /// <summary>The row of cards on offer. Built once per screen, since a draw is never rerolled.</summary>
    public class ExpeditionTalentOfferUIComponent : MonoBehaviour
    {
        [Inject] private readonly ICustomFactory _factory;

        [SerializeField] private RectTransform _container;
        [SerializeField] private ExpeditionTalentCardUIComponent _cardPrefab;

        private readonly List<ExpeditionTalentCardUIComponent> _cards = new();

        public event Action<string> OnTalentClicked;

        public void Build(IEnumerable<ExpeditionTalentOfferDTO> offers)
        {
            Clear();

            foreach (ExpeditionTalentOfferDTO offer in offers)
            {
                ExpeditionTalentCardUIComponent card = _factory.CreateFromPrefab(_cardPrefab, _container);

                card.SetTalent(offer.Talent, offer.Rarity, offer.OwnedLevel);
                card.OnTalentCardClicked += HandleTalentClicked;

                _cards.Add(card);
            }
        }

        private void OnDestroy()
        {
            Clear();
        }

        private void Clear()
        {
            foreach (ExpeditionTalentCardUIComponent card in _cards)
            {
                card.OnTalentCardClicked -= HandleTalentClicked;
                Destroy(card.gameObject);
            }

            _cards.Clear();
        }

        private void HandleTalentClicked(string talentId)
        {
            OnTalentClicked?.Invoke(talentId);
        }
    }
}
