using System;
using BaseArchitecture.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.MainMenu
{
    [AddressablePath("Screens/MenuView")]
    public class MenuView : View
    {
        [SerializeField] private Button _playGameButton;
        [SerializeField] private Button _quitGameButton;

        public event Action OnPlayGameButtonClicked;
        public event Action OnQuitGameButtonClicked;

        private void Awake()
        {
            _playGameButton.onClick.AddListener(() => OnPlayGameButtonClicked?.Invoke());
            _quitGameButton.onClick.AddListener(() => OnQuitGameButtonClicked?.Invoke());
        }
    }
}