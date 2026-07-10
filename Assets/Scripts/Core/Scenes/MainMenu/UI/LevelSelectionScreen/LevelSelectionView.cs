using System;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.MainMenu
{
    [AddressablePath("Screens/LevelSelectionScreenView")]
    public class LevelSelectionView : View
    {
        [SerializeField] private LevelButtonComponent _levelButtonPrefab;
        [SerializeField] private Transform _levelButtonsContainer;

        public event Action<int> OnLevelSelectedClicked;

    }
}
