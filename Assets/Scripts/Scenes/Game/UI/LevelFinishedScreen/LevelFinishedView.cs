using System;
using BaseArchitecture.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Game
{
    [AddressablePath("Screens/LevelFinishedScreenView")]
    public class LevelFinishedView : View
    {
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _mainMenuButton;

        public event Action OnNextLevelButtonClicked;
        public event Action OnMainMenuButtonClicked;

        private void Awake()
        {
            _nextLevelButton.onClick.AddListener(() => OnNextLevelButtonClicked?.Invoke());
            _mainMenuButton.onClick.AddListener(() => OnMainMenuButtonClicked?.Invoke());
        }

        public void Initialize(bool allLevelsComplete)
        {
            _nextLevelButton.gameObject.SetActive(!allLevelsComplete);
        }
    }
}
