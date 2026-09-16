using System;
using System.Text;
using SpaceInvaders.Scenes.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Expedition
{
    /// <summary>One offered card. Owns its own visuals; the offer only hands it a talent.</summary>
    public class ExpeditionTalentCardUIComponent : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _rarityFrameImage;
        [SerializeField] private TextMeshProUGUI _displayNameText;
        [SerializeField] private TextMeshProUGUI _rarityText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _bonusesText;

        [Header("Strings")]
        [SerializeField] private string _levelFormat = "LV {0}/{1}";

        private string _talentId;

        public event Action<string> OnTalentCardClicked;

        /// <summary>The card shows the level it would grant, not the one already owned.</summary>
        public void SetTalent(TalentConfigSO config, TalentRarityConfigSO rarityConfig, int ownedLevel)
        {
            _talentId = config.ObjectID;
            _displayNameText.text = config.DisplayName;
            _levelText.text = string.Format(_levelFormat, ownedLevel + 1, config.MaxLevel);
            _bonusesText.text = GetBonusesText(config, ownedLevel);

            _rarityText.text = rarityConfig != null ? rarityConfig.DisplayName : config.Rarity.ToString();
            _rarityText.color = rarityConfig != null ? rarityConfig.DisplayColor : Color.white;
            _rarityFrameImage.color = rarityConfig != null ? rarityConfig.DisplayColor : Color.white;
        }

        private void Awake()
        {
            _button.onClick.AddListener(() => OnTalentCardClicked?.Invoke(_talentId));
        }

        /// <summary>Routed through the shared stat formatting, so a flat bonus never reads as a
        /// percentage. The description carries whatever no stat line says.</summary>
        private static string GetBonusesText(TalentConfigSO config, int levelIndex)
        {
            var builder = new StringBuilder();

            if (levelIndex >= 0 && levelIndex < config.Levels.Count)
            {
                foreach (TalentModifierDTO modifier in config.Levels[levelIndex].Modifiers)
                {
                    builder.AppendLine(ShipStats.AffixFormat(modifier.StatType, modifier.Value, modifier.ValueType));
                }
            }

            if (!string.IsNullOrEmpty(config.Description))
            {
                builder.AppendLine(config.Description);
            }

            return builder.ToString();
        }
    }
}
