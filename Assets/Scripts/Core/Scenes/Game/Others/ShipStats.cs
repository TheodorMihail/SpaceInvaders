using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Ship stats that permanent progression (talents, equipped items) can modify.
    /// </summary>
    public enum ShipUpgradableStatTypes
    {
        Health,
        MoveSpeed,
        FireRate,
        Damage,
        ProjectileSpeed
    }

    public enum ShipStatValueTypes
    {
        Percentage,
        Flat
    }

    [Serializable]
    public class ShipBaseStats
    {
        [SerializeField] private int _health = 100;
        [SerializeField] private float _moveSpeed = 50f;
        [SerializeField] private float _fireRate = 1f;
        [SerializeField] private int _projectileDamage = 10;
        [SerializeField] private float _projectileSpeed = 150f;

        public int BaseHealth => _health;
        public float BaseMoveSpeed => _moveSpeed;
        public float BaseFireRate => _fireRate;
        public int BaseProjectileDamage => _projectileDamage;
        public float BaseProjectileSpeed => _projectileSpeed;
    }

    /// <summary>
    /// A base value with stackable flat and percentage bonus modifiers. Both count from base
    /// independently (order doesn't matter): percentage bonuses scale the base (positive bonuses
    /// sum additively, negative gives diminishing returns, never reaching/crossing -100%), flat
    /// bonuses add directly to it. The combined result is floored at 10% of base either way, so
    /// no combination of maluses can zero out or invert a stat.
    /// </summary>
    public class StatValue
    {
        private const float MinValueFraction = 0.1f;

        private readonly float _baseValue;
        private readonly List<float> _percentageBonuses = new();
        private readonly List<float> _flatBonuses = new();

        public float BaseValue => _baseValue;

        public float CurrentValue
        {
            get
            {
                float flatAdjustedBase = Mathf.Max(_baseValue + CombineFlatBonuses(), _baseValue * MinValueFraction);
                float percentageContribution = _baseValue * (CombinePercentageBonuses() - 1f);

                return Mathf.Max(flatAdjustedBase + percentageContribution, _baseValue * MinValueFraction);
            }
        }

        public StatValue(float baseValue)
        {
            _baseValue = baseValue;
        }

        public void AddBonus(float bonus, ShipStatValueTypes valueType)
        {
            GetBonusList(valueType).Add(bonus);
        }

        public void RemoveBonus(float bonus, ShipStatValueTypes valueType)
        {
            GetBonusList(valueType).Remove(bonus);
        }

        private List<float> GetBonusList(ShipStatValueTypes valueType)
        {
            return valueType == ShipStatValueTypes.Flat ? _flatBonuses : _percentageBonuses;
        }

        private float CombineFlatBonuses()
        {
            float sum = 0f;
            foreach (float bonus in _flatBonuses)
            {
                sum += bonus;
            }

            return sum;
        }

        private float CombinePercentageBonuses()
        {
            float positiveSum = 0f;
            float negativeFactor = 1f;

            foreach (float bonus in _percentageBonuses)
            {
                if (bonus >= 0f)
                {
                    positiveSum += bonus;
                }
                else
                {
                    negativeFactor *= 1f + bonus;
                }
            }

            return Mathf.Max((1f + positiveSum) * negativeFactor, 0f);
        }
    }

    public class ShipStats
    {
        public event Action<int, int> HealthChanged; // currentHealth, baseHealth

        private readonly StatValue _healthStat;
        private readonly StatValue _damageStat;
        private readonly StatValue _moveSpeedStat;
        private readonly StatValue _fireRateStat;
        private readonly StatValue _projectileSpeedStat;

        public StatValue HealthStat => _healthStat;
        public StatValue DamageStat => _damageStat;
        public StatValue MoveSpeedStat => _moveSpeedStat;
        public StatValue FireRateStat => _fireRateStat;
        public StatValue ProjectileSpeedStat => _projectileSpeedStat;

        public int BaseHealth => Mathf.RoundToInt(_healthStat.BaseValue);
        public float BaseMoveSpeed => _moveSpeedStat.BaseValue;
        public float BaseFireRate => _fireRateStat.BaseValue;
        public int BaseProjectileDamage => Mathf.RoundToInt(_damageStat.BaseValue);
        public float BaseProjectileSpeed => _projectileSpeedStat.BaseValue;

        public int CurrentHealth { get; private set; }
        public int CumulativeDamageTaken { get; private set; }
        public bool IsInvincible { get; private set; }
        public int ExtraShotCount { get; private set; }
        public float SpreadAngleDegrees { get; private set; }

        public int CurrentMaxHealth => Mathf.RoundToInt(_healthStat.CurrentValue);
        public int CurrentProjectileDamage => Mathf.RoundToInt(_damageStat.CurrentValue);
        public float CurrentMoveSpeed => _moveSpeedStat.CurrentValue;
        public float CurrentFireRate => _fireRateStat.CurrentValue;
        public float CurrentProjectileSpeed => _projectileSpeedStat.CurrentValue;

        public ShipStats(ShipBaseStats baseStats)
        {
            _healthStat = new StatValue(baseStats.BaseHealth);
            _damageStat = new StatValue(baseStats.BaseProjectileDamage);
            _moveSpeedStat = new StatValue(baseStats.BaseMoveSpeed);
            _fireRateStat = new StatValue(baseStats.BaseFireRate);
            _projectileSpeedStat = new StatValue(baseStats.BaseProjectileSpeed);

            CurrentHealth = CurrentMaxHealth;
        }

        public void ApplyDamage(int amount)
        {
            CumulativeDamageTaken += amount;
            CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
            HealthChanged?.Invoke(CurrentHealth, CurrentMaxHealth);
        }

        public void Heal(int amount)
        {
            CurrentHealth = Mathf.Min(CurrentHealth + amount, CurrentMaxHealth);
            HealthChanged?.Invoke(CurrentHealth, CurrentMaxHealth);
        }

        public void RefillHealth()
        {
            CurrentHealth = CurrentMaxHealth;
            HealthChanged?.Invoke(CurrentHealth, CurrentMaxHealth);
        }

        /// <summary>
        /// Adds a permanent bonus to the given stat. FireRate is a cooldown, so a positive
        /// bonus is inverted here to make the ship shoot faster.
        /// </summary>
        public void ApplyStatBonus(ShipUpgradableStatTypes statType, float bonus, ShipStatValueTypes valueType)
        {
            switch (statType)
            {
                case ShipUpgradableStatTypes.Health:
                {
                    _healthStat.AddBonus(bonus, valueType);
                    break;
                }
                case ShipUpgradableStatTypes.MoveSpeed:
                {
                    _moveSpeedStat.AddBonus(bonus, valueType);
                    break;
                }
                case ShipUpgradableStatTypes.FireRate:
                {
                    _fireRateStat.AddBonus(-bonus, valueType);
                    break;
                }
                case ShipUpgradableStatTypes.Damage:
                {
                    _damageStat.AddBonus(bonus, valueType);
                    break;
                }
                case ShipUpgradableStatTypes.ProjectileSpeed:
                {
                    _projectileSpeedStat.AddBonus(bonus, valueType);
                    break;
                }
            }
        }

        public void SetInvincible(bool value)
        {
            IsInvincible = value;
        }

        public void UpdateShotSpread(int deltaCount, float angleDegrees)
        {
            ExtraShotCount = Mathf.Max(0, ExtraShotCount + deltaCount);
            SpreadAngleDegrees = ExtraShotCount > 0 ? angleDegrees : 0f;
        }

        public static string StatDisplayName(ShipUpgradableStatTypes statType)
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

        public static string AffixFormat(ShipUpgradableStatTypes statType, float bonus, ShipStatValueTypes valueType)
        {
            string valueText = valueType == ShipStatValueTypes.Flat ? FormatStatDelta(statType, bonus) : FormatPercent(bonus);
            return $"{StatDisplayName(statType)} {valueText}";
        }

        public static string FormatStatValue(ShipUpgradableStatTypes statType, float value)
        {
            // Health/Damage are whole numbers on ShipStats (Mathf.RoundToInt); the rest are floats.
            bool isWholeNumberStat = statType == ShipUpgradableStatTypes.Health || statType == ShipUpgradableStatTypes.Damage;
            if (isWholeNumberStat)
            {
                return ((int)Math.Round(value, MidpointRounding.AwayFromZero)).ToString();
            }

            return value.ToString("0.##");
        }

        public static string FormatStatDelta(ShipUpgradableStatTypes statType, float delta)
        {
            string sign = delta >= 0f ? "+" : string.Empty; // negative values already print their own "-"
            return $"{sign}{FormatStatValue(statType, delta)}";
        }

        private static string FormatPercent(float bonus)
        {
            return $"{(bonus >= 0f ? "+" : string.Empty)}{bonus * 100f:0.#}%";
        }
    }
}
