using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Ship stats that permanent progression (talents, equipped items) can modify.
    /// </summary>
    /// <summary>Values are pinned: configs serialize this enum by index.</summary>
    public enum ShipUpgradableStatTypes
    {
        Health = 0,
        MoveSpeed = 1,
        FireRate = 2,
        Damage = 3,
        ProjectileSpeed = 4,
        CritChance = 5,
        CritDamage = 6,
        MagazineSize = 7,
        ReloadSpeed = 8
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
        [SerializeField] private float _critChance = 0.1f;
        [SerializeField] private float _critDamage = 2f;

        [Tooltip("Shots before a reload is needed. 0 means unlimited ammo.")]
        [SerializeField] private int _magazineSize = 0;
        [SerializeField] private float _reloadSpeed = 1.5f;

        public int BaseHealth => _health;
        public float BaseMoveSpeed => _moveSpeed;
        public float BaseFireRate => _fireRate;
        public int BaseProjectileDamage => _projectileDamage;
        public float BaseProjectileSpeed => _projectileSpeed;
        public float BaseCritChance => _critChance;
        public float BaseCritDamage => _critDamage;
        public int BaseMagazineSize => _magazineSize;
        public float BaseReloadSpeed => _reloadSpeed;
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
        private readonly StatValue _healthStat;
        private readonly StatValue _damageStat;
        private readonly StatValue _moveSpeedStat;
        private readonly StatValue _fireRateStat;
        private readonly StatValue _projectileSpeedStat;
        private readonly StatValue _critChanceStat;
        private readonly StatValue _critDamageStat;
        private readonly StatValue _magazineSizeStat;
        private readonly StatValue _reloadSpeedStat;

        public StatValue HealthStat => _healthStat;
        public StatValue DamageStat => _damageStat;
        public StatValue MoveSpeedStat => _moveSpeedStat;
        public StatValue FireRateStat => _fireRateStat;
        public StatValue ProjectileSpeedStat => _projectileSpeedStat;
        public StatValue CritChanceStat => _critChanceStat;
        public StatValue CritDamageStat => _critDamageStat;
        public StatValue MagazineSizeStat => _magazineSizeStat;
        public StatValue ReloadSpeedStat => _reloadSpeedStat;

        public int BaseHealth => Mathf.RoundToInt(_healthStat.BaseValue);
        public float BaseMoveSpeed => _moveSpeedStat.BaseValue;
        public float BaseFireRate => _fireRateStat.BaseValue;
        public int BaseProjectileDamage => Mathf.RoundToInt(_damageStat.BaseValue);
        public float BaseProjectileSpeed => _projectileSpeedStat.BaseValue;
        public float BaseCritChance => _critChanceStat.BaseValue;
        public float BaseCritDamage => _critDamageStat.BaseValue;
        public int BaseMagazineSize => Mathf.RoundToInt(_magazineSizeStat.BaseValue);
        public float BaseReloadSpeed => _reloadSpeedStat.BaseValue;

        public int CurrentHealth { get; private set; }
        public int CurrentAmmo { get; private set; }
        public int CumulativeDamageTaken { get; private set; }
        public bool IsInvincible { get; private set; }
        public int ExtraShotCount { get; private set; }
        public float SpreadAngleDegrees { get; private set; }

        public int CurrentMaxHealth => Mathf.RoundToInt(_healthStat.CurrentValue);
        public int CurrentProjectileDamage => Mathf.RoundToInt(_damageStat.CurrentValue);
        public float CurrentMoveSpeed => _moveSpeedStat.CurrentValue;
        public float CurrentFireRate => _fireRateStat.CurrentValue;
        public float CurrentProjectileSpeed => _projectileSpeedStat.CurrentValue;

        /// <summary>Clamped to a valid probability, since bonuses have no upper bound.</summary>
        public float CurrentCritChance => Mathf.Clamp01(_critChanceStat.CurrentValue);

        /// <summary>Floored at 1, so a critical hit can never deal less than a regular one.</summary>
        public float CurrentCritDamage => Mathf.Max(1f, _critDamageStat.CurrentValue);

        public int CurrentMaxAmmo => Mathf.RoundToInt(_magazineSizeStat.CurrentValue);
        public float CurrentReloadDuration => _reloadSpeedStat.CurrentValue;

        /// <summary>A magazine of 0 opts the ship out of ammo entirely, which is how enemies shoot forever.</summary>
        public bool HasUnlimitedAmmo => CurrentMaxAmmo <= 0;

        public bool IsOutOfAmmo => !HasUnlimitedAmmo && CurrentAmmo <= 0;

        public event Action<int, int> HealthChanged; // currentHealth, baseHealth
        public event Action<int, int> AmmoChanged; // currentAmmo, maxAmmo

        public ShipStats(ShipBaseStats baseStats)
        {
            _healthStat = new StatValue(baseStats.BaseHealth);
            _damageStat = new StatValue(baseStats.BaseProjectileDamage);
            _moveSpeedStat = new StatValue(baseStats.BaseMoveSpeed);
            _fireRateStat = new StatValue(baseStats.BaseFireRate);
            _projectileSpeedStat = new StatValue(baseStats.BaseProjectileSpeed);
            _critChanceStat = new StatValue(baseStats.BaseCritChance);
            _critDamageStat = new StatValue(baseStats.BaseCritDamage);
            _magazineSizeStat = new StatValue(baseStats.BaseMagazineSize);
            _reloadSpeedStat = new StatValue(baseStats.BaseReloadSpeed);

            CurrentHealth = CurrentMaxHealth;
            CurrentAmmo = CurrentMaxAmmo;
        }

        /// <summary>Rolls this ship's outgoing damage against its own crit stats.</summary>
        public int RollOutgoingDamage(out bool isCritical)
        {
            return RollOutgoingDamage(1f, out isCritical);
        }

        /// <summary>The multiplier is the firing attack's own damage scaling, layered over this ship's
        /// stats. It scales the rounded projectile damage, so a multiplier of 1 rolls exactly as an
        /// unscaled shot does.</summary>
        public int RollOutgoingDamage(float damageMultiplier, out bool isCritical)
        {
            isCritical = Random.value < CurrentCritChance;

            float damage = CurrentProjectileDamage * damageMultiplier;

            if (isCritical)
            {
                damage *= CurrentCritDamage;
            }

            return Mathf.RoundToInt(damage);
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

        /// <summary>Spends one round for a whole volley, so extra shots never cost extra ammo.
        /// Always succeeds for ships with unlimited ammo.</summary>
        public bool TryConsumeAmmo()
        {
            if (HasUnlimitedAmmo)
            {
                return true;
            }

            if (CurrentAmmo <= 0)
            {
                return false;
            }

            CurrentAmmo--;
            AmmoChanged?.Invoke(CurrentAmmo, CurrentMaxAmmo);

            return true;
        }

        public void RefillAmmo()
        {
            CurrentAmmo = CurrentMaxAmmo;
            AmmoChanged?.Invoke(CurrentAmmo, CurrentMaxAmmo);
        }

        /// <summary>
        /// Adds a permanent bonus to the given stat. FireRate and ReloadSpeed are durations, so a
        /// positive bonus is inverted here to make the ship shoot and reload faster.
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
                case ShipUpgradableStatTypes.CritChance:
                {
                    _critChanceStat.AddBonus(bonus, valueType);
                    break;
                }
                case ShipUpgradableStatTypes.CritDamage:
                {
                    _critDamageStat.AddBonus(bonus, valueType);
                    break;
                }
                case ShipUpgradableStatTypes.MagazineSize:
                {
                    _magazineSizeStat.AddBonus(bonus, valueType);
                    break;
                }
                case ShipUpgradableStatTypes.ReloadSpeed:
                {
                    _reloadSpeedStat.AddBonus(-bonus, valueType);
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
                ShipUpgradableStatTypes.CritChance => "Crit Chance",
                ShipUpgradableStatTypes.CritDamage => "Crit Damage",
                ShipUpgradableStatTypes.MagazineSize => "Magazine Size",
                ShipUpgradableStatTypes.ReloadSpeed => "Reload Speed",
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
            // Health/Damage/MagazineSize are whole numbers on ShipStats (Mathf.RoundToInt); the rest are floats.
            bool isWholeNumberStat = statType == ShipUpgradableStatTypes.Health
                || statType == ShipUpgradableStatTypes.Damage
                || statType == ShipUpgradableStatTypes.MagazineSize;
            if (isWholeNumberStat)
            {
                return ((int)Math.Round(value, MidpointRounding.AwayFromZero)).ToString();
            }

            // Crit stats are stored as ratios but read as percentages.
            if (IsRatioStat(statType))
            {
                return $"{value * 100f:0.#}%";
            }

            return value.ToString("0.##");
        }

        public static string FormatStatDelta(ShipUpgradableStatTypes statType, float delta)
        {
            string sign = delta >= 0f ? "+" : string.Empty; // negative values already print their own "-"
            return $"{sign}{FormatStatValue(statType, delta)}";
        }

        private static bool IsRatioStat(ShipUpgradableStatTypes statType)
        {
            return statType == ShipUpgradableStatTypes.CritChance || statType == ShipUpgradableStatTypes.CritDamage;
        }

        private static string FormatPercent(float bonus)
        {
            return $"{(bonus >= 0f ? "+" : string.Empty)}{bonus * 100f:0.#}%";
        }
    }
}
