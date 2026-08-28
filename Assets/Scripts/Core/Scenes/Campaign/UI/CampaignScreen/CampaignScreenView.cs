using System;
using BaseArchitecture.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Campaign
{
    [AddressablePath("Screens/CampaignScreenView")]
    public class CampaignScreenView : View
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _talentsButton;
        [SerializeField] private Button _inventoryButton;
        [SerializeField] private Button _backButton;

        public event Action OnPlayButtonClicked;
        public event Action OnTalentsButtonClicked;
        public event Action OnInventoryButtonClicked;
        public event Action OnBackButtonClicked;

        private void Awake()
        {
            _playButton.onClick.AddListener(() => OnPlayButtonClicked?.Invoke());
            _talentsButton.onClick.AddListener(() => OnTalentsButtonClicked?.Invoke());
            _inventoryButton.onClick.AddListener(() => OnInventoryButtonClicked?.Invoke());
            _backButton.onClick.AddListener(() => OnBackButtonClicked?.Invoke());
        }
    }
}
