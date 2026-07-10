using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using UnityEngine;

namespace SpaceInvaders.Scenes.MainMenu
{
    [AddressablePath("Screens/LevelSelectionScreenView")]
    public class LevelSelectionView : View
    {
        [SerializeField] private LevelButtonComponent _levelButtonPrefab;
        [SerializeField] private Transform _levelButtonsContainer;

        public event Action<int> OnLevelSelectedClicked;

        public void SetupLevels(IReadOnlyList<LevelConfigSO> levels)
        {
            foreach(var level in levels)
            {
                var button = Instantiate(_levelButtonPrefab, _levelButtonsContainer);
                button.Setup(level, false);
                button.OnLevelButtonClicked += OnLevelSelectedClicked;
            }
        }
    }
}
