using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SpaceInvaders.Scenes.MainMenu
{
    [AddressablePath("Screens/TalentTreeScreenView")]
    public class TalentTreeView : View
    {
        [SerializeField] private TalentButtonComponent _talentButtonPrefab;
        [SerializeField] private Transform _talentButtonsContainer;
        [SerializeField] private TextMeshProUGUI _currencyText;
        [SerializeField] private Button _backButton;

        [Inject] private readonly ITalentManager _talentManager;
        [Inject] private readonly ICurrencyManager _currencyManager;

        private readonly Dictionary<TalentTypes, TalentButtonComponent> _talentButtons = new();
        private readonly Dictionary<TalentTypes, TalentConfigSO> _talentConfigs = new();

        public event Action<TalentTypes> OnTalentPurchaseClicked;
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
                var button = Instantiate(_talentButtonPrefab, _talentButtonsContainer);
                button.OnTalentButtonClicked += OnTalentPurchaseClicked;

                _talentButtons[talent.TalentType] = button;
                _talentConfigs[talent.TalentType] = talent;

                RefreshButtonDisplay(talent.TalentType);
            }

            UpdateCurrencyDisplay();
        }

        public void RefreshAllTalentButtons()
        {
            foreach (var type in _talentButtons.Keys)
            {
                RefreshButtonDisplay(type);
            }

            UpdateCurrencyDisplay();
        }

        private void RefreshButtonDisplay(TalentTypes type)
        {
            if (!_talentButtons.TryGetValue(type, out var button) || !_talentConfigs.TryGetValue(type, out var config))
            {
                return;
            }

            button.Setup(
                config,
                _talentManager.GetTalentLevel(type),
                _talentManager.GetNextLevelCost(type),
                _talentManager.IsMaxLevel(type),
                _talentManager.CanAfford(type));
        }

        private void UpdateCurrencyDisplay()
        {
            _currencyText.text = _currencyManager.Currency.ToString();
        }
    }
}
