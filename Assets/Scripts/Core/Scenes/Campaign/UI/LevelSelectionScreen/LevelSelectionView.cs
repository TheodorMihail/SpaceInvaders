using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SpaceInvaders.Scenes.Campaign
{
    [AddressablePath("Screens/LevelSelectionScreenView")]
    public class LevelSelectionView : View<LevelSelectionModel>
    {
        [Inject] private readonly ICustomFactory _factory;

        [SerializeField] private LevelButtonComponent _levelButtonPrefab;
        [SerializeField] private Transform _levelButtonsContainer;
        [SerializeField] private Button _backButton;
        [SerializeField] private List<LevelTypeIconDTO> _levelTypeIcons;

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
                button.Setup(level, GetLevelTypeIcon(level.LevelType), !_model.IsLevelUnlocked(level.Index), _model.GetLevelStars(level.Index));
                button.OnLevelButtonClicked += OnLevelSelectedClicked;
            }
        }

        private Sprite GetLevelTypeIcon(LevelTypes levelType)
        {
            foreach (LevelTypeIconDTO entry in _levelTypeIcons)
            {
                if (entry.LevelType == levelType)
                {
                    return entry.Icon;
                }
            }

            return null;
        }

        /// <summary>Icon shown on a level button, picked by the level's type.</summary>
        [Serializable]
        public struct LevelTypeIconDTO
        {
            [SerializeField] private LevelTypes _levelType;
            [SerializeField] private Sprite _icon;

            public LevelTypes LevelType => _levelType;
            public Sprite Icon => _icon;
        }
    }
}
