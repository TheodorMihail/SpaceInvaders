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

        [SerializeField] private TalentButtonComponent _talentButtonPrefab;
        [SerializeField] private Transform _talentButtonsContainer;
        [SerializeField] private CurrencyUIComponent _currency;
        [SerializeField] private Button _backButton;

        private readonly Dictionary<ShipUpgradableStatTypes, TalentButtonComponent> _talentButtons = new();
        private readonly Dictionary<ShipUpgradableStatTypes, TalentConfigSO> _talentConfigs = new();

        public event Action<ShipUpgradableStatTypes> OnTalentPurchaseClicked;
        public event Action OnBackClicked;

        private void Awake()
        {
            _backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
        }

        public void SetupTalents(IReadOnlyList<TalentConfigSO> talents)
        {
            foreach (Transform child in _talentButtonsContainer)
            {
                Destroy(child.gameObject);
            }

            _talentButtons.Clear();
            _talentConfigs.Clear();

            foreach (var talent in talents)
            {
                var button = _factory.CreateFromPrefab(_talentButtonPrefab, _talentButtonsContainer);
                button.OnTalentButtonClicked += OnTalentPurchaseClicked;

                _talentButtons[talent.TalentType] = button;
                _talentConfigs[talent.TalentType] = talent;

                RefreshButtonDisplay(talent.TalentType);
            }

            _currency.Initialize(_model.Currency);
        }

        public void RefreshAllTalentButtons()
        {
            foreach (var type in _talentButtons.Keys)
            {
                RefreshButtonDisplay(type);
            }

            _currency.UpdateCurrency(_model.Currency);
        }

        private void RefreshButtonDisplay(ShipUpgradableStatTypes type)
        {
            if (!_talentButtons.TryGetValue(type, out var button) || !_talentConfigs.TryGetValue(type, out var config))
            {
                return;
            }

            button.Setup(
                config,
                _model.GetTalentLevel(type),
                _model.GetNextLevelCost(type),
                _model.IsMaxLevel(type),
                _model.CanAfford(type));
        }

    }
}
