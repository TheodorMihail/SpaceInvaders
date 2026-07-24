using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
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
    /// A base value with stackable bonus modifiers. Positive (growth) bonuses sum additively,
    /// Negative gives diminishing returns, never reaching/crossing -100%)
    /// </summary>
    public class StatValue
    {
        private readonly float _baseValue;
        private readonly List<float> _bonuses = new();

        public float BaseValue => _baseValue;
        public float CurrentValue => _baseValue * CombineBonuses();

        public StatValue(float baseValue)
        {
            _baseValue = baseValue;
        }

        public void AddBonus(float bonus)
        {
            _bonuses.Add(bonus);
        }

        public void RemoveBonus(float bonus)
        {
            _bonuses.Remove(bonus);
        }

        private float CombineBonuses()
        {
            float positiveSum = 0f;
            float negativeFactor = 1f;

            foreach (float bonus in _bonuses)
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

        public void SetInvincible(bool value)
        {
            IsInvincible = value;
        }

        public void UpdateShotSpread(int deltaCount, float angleDegrees)
        {
            ExtraShotCount = Mathf.Max(0, ExtraShotCount + deltaCount);
            SpreadAngleDegrees = ExtraShotCount > 0 ? angleDegrees : 0f;
        }
    }
}
