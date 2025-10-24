using System;
using BaseArchitecture.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Game
{
    [AddressablePath("Screens/GameOverScreenView")]
    public class GameOverView : View
    {
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _mainMenuButton;

        public event Action OnRestartButtonClicked;
        public event Action OnMainMenuButtonClicked;

        private void Awake()
        {
            _restartButton.onClick.AddListener(() => OnRestartButtonClicked?.Invoke());
            _mainMenuButton.onClick.AddListener(() => OnMainMenuButtonClicked?.Invoke());
        }
    }
}
