using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SpaceInvaders.Scenes.Campaign
{
    [AddressablePath("Screens/TalentTreeScreenView")]
    public class TalentTreeView : View<TalentTreeModel>
    {
        [Inject] private readonly ICustomFactory _factory;

        [SerializeField] private TalentCardUIComponent _talentCardPrefab;
        [SerializeField] private Transform _talentCardsContainer;
        [SerializeField] private CurrencyUIComponent _currency;
        [SerializeField] private Button _backButton;

        private readonly Dictionary<string, TalentCardUIComponent> _talentCards = new();
        private readonly Dictionary<string, TalentConfigSO> _talentConfigs = new();

        public event Action<string> OnTalentPurchaseClicked;
        public event Action OnBackClicked;

        private void Awake()
        {
            _backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
        }

        public void SetupTalents(IReadOnlyList<TalentConfigSO> talents)
        {
            foreach (Transform child in _talentCardsContainer)
            {
                Destroy(child.gameObject);
            }

            _talentCards.Clear();
            _talentConfigs.Clear();

            foreach (var talent in talents)
            {
                var card = _factory.CreateFromPrefab(_talentCardPrefab, _talentCardsContainer);
                card.OnTalentCardClicked += OnTalentPurchaseClicked;

                _talentCards[talent.ObjectID] = card;
                _talentConfigs[talent.ObjectID] = talent;

                RefreshCardDisplay(talent.ObjectID);
            }

            _currency.Initialize(_model.Currency);
        }

        public void RefreshAllTalentCards()
        {
            foreach (string talentId in _talentCards.Keys)
            {
                RefreshCardDisplay(talentId);
            }

            _currency.UpdateCurrency(_model.Currency);
        }

        private void RefreshCardDisplay(string talentId)
        {
            if (!_talentCards.TryGetValue(talentId, out var card)
                || !_talentConfigs.TryGetValue(talentId, out var config))
            {
                return;
            }

            card.Setup(
                config,
                _model.GetTalentLevel(talentId),
                _model.GetNextLevelCost(talentId),
                _model.IsMaxLevel(talentId),
                _model.CanAfford(talentId));
        }

    }
}
