using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.MainMenu
{
    [AddressablePath("Screens/LevelSelectionScreenView")]
    public class LevelSelectionView : View
    {
        [SerializeField] private LevelButtonComponent _levelButtonPrefab;
        [SerializeField] private Transform _levelButtonsContainer;

        [Inject] private readonly IProgressManager _progressManager;

        public event Action<int> OnLevelSelectedClicked;

        public void SetupLevels(IReadOnlyList<LevelConfigSO> levels)
        {
            foreach(var level in levels)
            {
                var button = Instantiate(_levelButtonPrefab, _levelButtonsContainer);
                button.Setup(level, !_progressManager.IsLevelUnlocked(level.Index), _progressManager.GetStars(level.Index));
                button.OnLevelButtonClicked += OnLevelSelectedClicked;
            }
        }
    }
}
