using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SpaceInvaders.Scenes.MainMenu
{
    [AddressablePath("Screens/LevelSelectionScreenView")]
    public class LevelSelectionView : View
    {
        [SerializeField] private LevelButtonComponent _levelButtonPrefab;
        [SerializeField] private Transform _levelButtonsContainer;
        [SerializeField] private Button _backButton;

        [Inject] private readonly ILevelManager _levelManager;

        public event Action<int> OnLevelSelectedClicked;
        public event Action OnBackClicked;

        private void Awake()
        {
            _backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
        }

        public void SetupLevels(IReadOnlyList<LevelConfigSO> levels)
        {
            foreach(var level in levels)
            {
                var button = Instantiate(_levelButtonPrefab, _levelButtonsContainer);
                button.Setup(level, !_levelManager.IsLevelUnlocked(level.Index), _levelManager.GetLevelStars(level.Index));
                button.OnLevelButtonClicked += OnLevelSelectedClicked;
            }
        }
    }
}
