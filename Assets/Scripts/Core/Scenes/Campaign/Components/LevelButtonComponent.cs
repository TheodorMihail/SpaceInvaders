using System;
using SpaceInvaders.Scenes.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Campaign
{
    public class LevelButtonComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _levelNumberText;
        [SerializeField] private GameObject _levelLockedImage;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject[] _starIcons;
        [SerializeField] private Image _levelTypeIcon;

        private int _levelNumber;
        public event Action<int> OnLevelButtonClicked;

        private void Awake()
        {
            _button.onClick.AddListener(() => OnLevelButtonClicked?.Invoke(_levelNumber));
        }

        public void Setup(LevelConfigSO level, Sprite levelTypeIcon, bool isLocked, int starsEarned)
        {
            _levelNumber = level.Index;
            _levelNumberText.text = level.LevelType == LevelTypes.Boss ? BossLevelName() : NormalLevelName(level);
            _levelLockedImage.SetActive(isLocked);
            _button.interactable = !isLocked;

            SetLevelTypeIcon(levelTypeIcon);

            for (int i = 0; i < _starIcons.Length; i++)
            {
                _starIcons[i].SetActive(i < starsEarned);
            }
        }

        private string BossLevelName()
        {
            return "BOSS";
        }

        private string NormalLevelName(LevelConfigSO level)
        {
            return level.Index.ToString();
        }

        /// <summary>Hidden rather than left blank when the type has no icon authored.</summary>
        private void SetLevelTypeIcon(Sprite levelTypeIcon)
        {
            if (_levelTypeIcon == null)
            {
                return;
            }

            _levelTypeIcon.sprite = levelTypeIcon;
            _levelTypeIcon.enabled = levelTypeIcon != null;
        }
    }
}
