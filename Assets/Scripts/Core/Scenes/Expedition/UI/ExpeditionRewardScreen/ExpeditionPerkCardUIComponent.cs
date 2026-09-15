using System;
using System.Text;
using SpaceInvaders.Scenes.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Expedition
{
    /// <summary>One offered card. Owns its own visuals; the offer only hands it a perk.</summary>
    public class ExpeditionPerkCardUIComponent : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _rarityFrameImage;
        [SerializeField] private TextMeshProUGUI _displayNameText;
        [SerializeField] private TextMeshProUGUI _rarityText;
        [SerializeField] private TextMeshProUGUI _bonusesText;

        private string _perkId;

        public event Action<string> OnClicked;

        public void SetPerk(ExpeditionPerkConfigSO config, ExpeditionPerkRarityConfigSO rarityConfig)
        {
            _perkId = config.ObjectID;
            _displayNameText.text = config.DisplayName;
            _bonusesText.text = GetBonusesText(config);

            _rarityText.text = rarityConfig != null ? rarityConfig.DisplayName : config.Rarity.ToString();
            _rarityText.color = rarityConfig != null ? rarityConfig.DisplayColor : Color.white;
            _rarityFrameImage.color = rarityConfig != null ? rarityConfig.DisplayColor : Color.white;
        }

        private void Awake()
        {
            _button.onClick.AddListener(() => OnClicked?.Invoke(_perkId));
        }

        /// <summary>Routed through the shared stat formatting, so a flat bonus never reads as a
        /// percentage. The description carries whatever no stat line describes.</summary>
        private static string GetBonusesText(ExpeditionPerkConfigSO config)
        {
            var builder = new StringBuilder();

            foreach (ExpeditionPerkModifierDTO modifier in config.Modifiers)
            {
                builder.AppendLine(ShipStats.AffixFormat(modifier.StatType, modifier.Value, modifier.ValueType));
            }

            if (!string.IsNullOrEmpty(config.Description))
            {
                builder.AppendLine(config.Description);
            }

            return builder.ToString();
        }
    }
}
