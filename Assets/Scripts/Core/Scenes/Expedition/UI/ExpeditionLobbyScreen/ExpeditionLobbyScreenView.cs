using System;
using BaseArchitecture.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Expedition
{
    [AddressablePath("Screens/ExpeditionLobbyScreenView")]
    public class ExpeditionLobbyScreenView : View
    {
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _newRunButton;
        [SerializeField] private Button _backButton;

        [Header("Strings")]
        [SerializeField] private string _newRunString = "NEW EXPEDITION";
        [SerializeField] private string _abandonAndStartString = "ABANDON EXPEDITION";
        [SerializeField] private TextMeshProUGUI _newRunButtonText;

        public event Action OnContinueButtonClicked;
        public event Action OnNewRunButtonClicked;
        public event Action OnBackButtonClicked;

        /// <summary>Continuing only exists while a run does, and starting a new one then replaces it.</summary>
        public void Initialize(bool hasActiveRun)
        {
            _continueButton.gameObject.SetActive(hasActiveRun);

            if (_newRunButtonText != null)
            {
                _newRunButtonText.text = hasActiveRun ? _abandonAndStartString : _newRunString;
            }
        }

        private void Awake()
        {
            _continueButton.onClick.AddListener(() => OnContinueButtonClicked?.Invoke());
            _newRunButton.onClick.AddListener(() => OnNewRunButtonClicked?.Invoke());
            _backButton.onClick.AddListener(() => OnBackButtonClicked?.Invoke());
        }
    }
}
