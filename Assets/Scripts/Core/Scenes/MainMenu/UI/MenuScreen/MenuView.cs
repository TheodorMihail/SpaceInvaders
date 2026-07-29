using System;
using BaseArchitecture.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.MainMenu
{
    [AddressablePath("Screens/MenuScreenView")]
    public class MenuView : View
    {
        [SerializeField] private Button _playGameButton;
        [SerializeField] private Button _talentsButton;
        [SerializeField] private Button _inventoryButton;
        [SerializeField] private Button _quitGameButton;

        public event Action OnPlayGameButtonClicked;
        public event Action OnQuitGameButtonClicked;
        public event Action OnTalentsButtonClicked;
        public event Action OnInventoryButtonClicked;

        private void Awake()
        {
            _playGameButton.onClick.AddListener(() => OnPlayGameButtonClicked?.Invoke());
            _talentsButton.onClick.AddListener(() => OnTalentsButtonClicked?.Invoke());
            _inventoryButton.onClick.AddListener(() => OnInventoryButtonClicked?.Invoke());
            _quitGameButton.onClick.AddListener(() => OnQuitGameButtonClicked?.Invoke());
        }
    }
}