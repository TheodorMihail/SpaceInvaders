using System;
using SpaceInvaders.Scenes.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class LevelButtonComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _levelNumberText;
        [SerializeField] private GameObject _levelLockedImage;
        [SerializeField] private Button _button;

        private int _levelNumber;

        public event Action<int> OnLevelButtonClicked;

        private void Awake()
        {
            _button.onClick.AddListener(() => OnLevelButtonClicked?.Invoke(_levelNumber));
        }

        public void Setup(LevelConfigSO level, bool isLocked)
        {
            _levelNumber = level.Index;
            _levelNumberText.text = level.Index.ToString();
            _levelLockedImage.SetActive(isLocked);
            _button.interactable = !isLocked;
        }
    }
}
