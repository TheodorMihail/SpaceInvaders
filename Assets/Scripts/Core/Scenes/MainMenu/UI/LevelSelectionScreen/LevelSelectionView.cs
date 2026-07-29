using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SpaceInvaders.Scenes.MainMenu
{
    [AddressablePath("Screens/LevelSelectionScreenView")]
    public class LevelSelectionView : View<LevelSelectionModel>
    {
        [Inject] private readonly ICustomFactory _factory;

        [SerializeField] private LevelButtonComponent _levelButtonPrefab;
        [SerializeField] private Transform _levelButtonsContainer;
        [SerializeField] private Button _backButton;

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
                var button = _factory.CreateFromPrefab(_levelButtonPrefab, _levelButtonsContainer);
                button.Setup(level, !_model.IsLevelUnlocked(level.Index), _model.GetLevelStars(level.Index));
                button.OnLevelButtonClicked += OnLevelSelectedClicked;
            }
        }
    }
}
