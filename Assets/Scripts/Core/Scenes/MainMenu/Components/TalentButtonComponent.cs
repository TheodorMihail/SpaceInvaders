using System;
using SpaceInvaders.Scenes.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class TalentButtonComponent : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _displayNameText;
        [SerializeField] private TextMeshProUGUI _currentMultiplierText;
        [SerializeField] private TextMeshProUGUI _nextMultiplierText;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private Button _buyButton;

        [Header("Text Formats")]
        [SerializeField] private string _currentMultiplierFormat = "Current Multiplier: {0}";
        [SerializeField] private string _nextMultiplierFormat = "Next Multiplier: {0}";
        [SerializeField] private string _costFormat = "Cost: {0}";
        [SerializeField] private string _maxLevelValue = "MAX";

        private TalentTypes _talentType;
        public event Action<TalentTypes> OnTalentButtonClicked;

        private void Awake()
        {
            _buyButton.onClick.AddListener(() => OnTalentButtonClicked?.Invoke(_talentType));
        }

        public void Setup(TalentConfigSO config, int currentLevel, int nextCost, bool isMaxed, bool canAfford)
        {
            _talentType = config.TalentType;
            _iconImage.sprite = config.Icon;
            _displayNameText.text = config.DisplayName;

            float currentBonus = SumBonusDelta(config, currentLevel);
            _currentMultiplierText.text = string.Format(_currentMultiplierFormat, FormatPercent(currentBonus));

            if (isMaxed)
            {
                _nextMultiplierText.text = string.Format(_nextMultiplierFormat, _maxLevelValue);
                _costText.text = string.Format(_costFormat, _maxLevelValue);
            }
            else
            {
                float nextBonus = SumBonusDelta(config, currentLevel + 1);
                _nextMultiplierText.text = string.Format(_nextMultiplierFormat, FormatPercent(nextBonus));
                _costText.text = string.Format(_costFormat, nextCost);
            }

            _buyButton.interactable = !isMaxed && canAfford;
        }

        private static float SumBonusDelta(TalentConfigSO config, int levelCount)
        {
            float total = 0f;
            for (int i = 0; i < levelCount; i++)
            {
                total += config.Levels[i].BonusDelta;
            }

            return total;
        }

        private static string FormatPercent(float bonus)
        {
            return $"{bonus * 100f:0.#}%";
        }
    }
}
