using System;
using BaseArchitecture.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.MainMenu
{
    [AddressablePath("Screens/MenuScreenView")]
    public class MenuView : View
    {
        [SerializeField] private Button _campaignButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitGameButton;

        public event Action OnCampaignButtonClicked;
        public event Action OnSettingsButtonClicked;
        public event Action OnQuitGameButtonClicked;

        private void Awake()
        {
            _campaignButton.onClick.AddListener(() => OnCampaignButtonClicked?.Invoke());
            _settingsButton.onClick.AddListener(() => OnSettingsButtonClicked?.Invoke());
            _quitGameButton.onClick.AddListener(() => OnQuitGameButtonClicked?.Invoke());
        }
    }
}
