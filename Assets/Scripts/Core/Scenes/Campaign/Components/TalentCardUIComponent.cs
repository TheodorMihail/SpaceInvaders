using System;
using System.Collections.Generic;
using System.Text;
using SpaceInvaders.Scenes.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Campaign
{
    public class TalentCardUIComponent : MonoBehaviour
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
        [SerializeField] private string _bonusSeparator = ", ";

        private string _talentId;
        public event Action<string> OnTalentCardClicked;

        private void Awake()
        {
            _buyButton.onClick.AddListener(() => OnTalentCardClicked?.Invoke(_talentId));
        }

        public void Setup(TalentConfigSO config, int currentLevel, int nextCost, bool isMaxed, bool canAfford)
        {
            _talentId = config.ObjectID;
            _iconImage.sprite = config.Icon;
            _displayNameText.text = config.DisplayName;

            _currentMultiplierText.text = string.Format(_currentMultiplierFormat, FormatOwnedBonuses(config, currentLevel));

            if (isMaxed)
            {
                _nextMultiplierText.text = string.Format(_nextMultiplierFormat, _maxLevelValue);
                _costText.text = string.Format(_costFormat, _maxLevelValue);
            }
            else
            {
                _nextMultiplierText.text =
                    string.Format(_nextMultiplierFormat, FormatOwnedBonuses(config, currentLevel + 1));
                _costText.text = string.Format(_costFormat, nextCost);
            }

            _buyButton.interactable = !isMaxed && canAfford;
        }

        /// <summary>
        /// Every line the whole ladder can touch, totalled over the levels bought so far. Lines the
        /// talent has but has not reached yet still render at zero, so an unbought talent reads in its
        /// own units rather than blank.
        /// </summary>
        private string FormatOwnedBonuses(TalentConfigSO config, int levelCount)
        {
            var builder = new StringBuilder();

            foreach (TalentModifierDTO line in GetDistinctLines(config))
            {
                float total = SumLine(config, line, levelCount);

                if (builder.Length > 0)
                {
                    builder.Append(_bonusSeparator);
                }

                builder.Append(FormatBonus(line.StatType, line.ValueType, total));
            }

            return builder.ToString();
        }

        /// <summary>One entry per stat the ladder touches, in the order the levels introduce them.</summary>
        private static List<TalentModifierDTO> GetDistinctLines(TalentConfigSO config)
        {
            var lines = new List<TalentModifierDTO>();

            foreach (TalentLevelDTO level in config.Levels)
            {
                foreach (TalentModifierDTO modifier in level.Modifiers)
                {
                    if (!ContainsLine(lines, modifier))
                    {
                        lines.Add(modifier);
                    }
                }
            }

            return lines;
        }

        private static bool ContainsLine(List<TalentModifierDTO> lines, TalentModifierDTO modifier)
        {
            foreach (TalentModifierDTO line in lines)
            {
                if (line.StatType == modifier.StatType && line.ValueType == modifier.ValueType)
                {
                    return true;
                }
            }

            return false;
        }

        private static float SumLine(TalentConfigSO config, TalentModifierDTO line, int levelCount)
        {
            float total = 0f;

            for (int i = 0; i < levelCount && i < config.Levels.Count; i++)
            {
                foreach (TalentModifierDTO modifier in config.Levels[i].Modifiers)
                {
                    if (modifier.StatType == line.StatType && modifier.ValueType == line.ValueType)
                    {
                        total += modifier.Value;
                    }
                }
            }

            return total;
        }

        /// <summary>Routed through the shared stat formatting so flat talents don't render as percentages.</summary>
        private static string FormatBonus(ShipUpgradableStatTypes statType, ShipStatValueTypes valueType, float bonus)
        {
            if (valueType == ShipStatValueTypes.Flat)
            {
                return ShipStats.FormatStatDelta(statType, bonus);
            }

            return $"{bonus * 100f:0.#}%";
        }
    }
}
