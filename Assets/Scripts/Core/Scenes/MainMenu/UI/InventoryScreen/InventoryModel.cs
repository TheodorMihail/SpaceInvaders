using System;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class InventoryModel : Model
    {
        public string EmptyInventoryText { get; } = "No items collected yet.";
        public string EquipActionLabel { get; } = "Equip";
        public string UnequipActionLabel { get; } = "Unequip";

        /// <summary>Instance id of the inventory item whose tooltip is open, or null.</summary>
        public string OpenItemInstanceId { get; set; }

        /// <summary>Equipment slot whose tooltip is open, or null. Mutually exclusive with OpenItemInstanceId.</summary>
        public EquipmentSlots? OpenSlot { get; set; }

        public void CloseTooltip()
        {
            OpenItemInstanceId = null;
            OpenSlot = null;
        }

        public string AffixFormat(ShipUpgradableStatTypes statType, float bonus)
        {
            return $"{StatDisplayName(statType)} {FormatPercent(bonus)}";
        }

        public string StatDisplayName(ShipUpgradableStatTypes statType)
        {
            return statType switch
            {
                ShipUpgradableStatTypes.Health => "Health",
                ShipUpgradableStatTypes.MoveSpeed => "Move Speed",
                ShipUpgradableStatTypes.FireRate => "Fire Rate",
                ShipUpgradableStatTypes.Damage => "Damage",
                ShipUpgradableStatTypes.ProjectileSpeed => "Projectile Speed",
                _ => statType.ToString()
            };
        }

        public string FormatPercent(float bonus)
        {
            return $"{(bonus >= 0f ? "+" : string.Empty)}{bonus * 100f:0.#}%";
        }

        /// <summary>
        /// "Health: 100 +20" with the base (pre-equipment) value in white and the equipped items'
        /// contribution in green, so the panel visually separates "what you have" from "what your
        /// gear adds."
        /// </summary>
        public string StatRowText(ShipUpgradableStatTypes statType, float baseValue, float withEquipmentValue)
        {
            float delta = withEquipmentValue - baseValue;
            string baseText = FormatStatValue(statType, baseValue);
            string deltaText = delta == 0 ? "" : FormatStatDelta(statType, delta);
            return $"{StatDisplayName(statType)}: <color=white>{baseText}</color> <color=green>{deltaText}</color>";
        }

        private string FormatStatDelta(ShipUpgradableStatTypes statType, float delta)
        {
            string sign = delta >= 0f ? "+" : string.Empty; // negative values already print their own "-"
            return $"{sign}{FormatStatValue(statType, delta)}";
        }

        private string FormatStatValue(ShipUpgradableStatTypes statType, float value)
        {
            // Health/Damage are whole numbers on ShipStats (Mathf.RoundToInt); the rest are floats.
            bool isWholeNumberStat = statType == ShipUpgradableStatTypes.Health || statType == ShipUpgradableStatTypes.Damage;
            if (isWholeNumberStat)
            {
                return ((int)Math.Round(value, MidpointRounding.AwayFromZero)).ToString();
            }

            return value.ToString("0.#");
        }
    }
}
