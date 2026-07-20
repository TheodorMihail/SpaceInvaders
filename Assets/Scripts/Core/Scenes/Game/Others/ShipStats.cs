using System;
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
    
    public class ShipStats
    {
        public event Action<int, int> HealthChanged; // currentHealth, baseHealth

        private readonly ShipBaseStats _baseStats;

        public int BaseHealth => _baseStats.BaseHealth;
        public float BaseMoveSpeed => _baseStats.BaseMoveSpeed;
        public float BaseFireRate => _baseStats.BaseFireRate;
        public int BaseProjectileDamage => _baseStats.BaseProjectileDamage;
        public float BaseProjectileSpeed => _baseStats.BaseProjectileSpeed;

        public int CurrentHealth { get; private set; }
        public int CumulativeDamageTaken { get; private set; }
        public bool IsInvincible { get; private set; }
        public int ExtraShotCount { get; private set; }
        public float SpreadAngleDegrees { get; private set; }

        private float _damageMultiplierBonus;
        private float _fireRateMultiplierBonus;
        private float _moveSpeedMultiplierBonus;
        private float _projectileSpeedMultiplierBonus;

        public float CurrentMoveSpeed => BaseMoveSpeed * (1f + _moveSpeedMultiplierBonus);
        public float CurrentFireRate => BaseFireRate * (1f + _fireRateMultiplierBonus);
        public int CurrentProjectileDamage => Mathf.RoundToInt(BaseProjectileDamage * (1f + _damageMultiplierBonus));
        public float CurrentProjectileSpeed => BaseProjectileSpeed * (1f + _projectileSpeedMultiplierBonus);

        public ShipStats(ShipBaseStats baseStats)
        {
            _baseStats = baseStats;
            CurrentHealth = baseStats.BaseHealth;
        }

        public void ApplyDamage(int amount)
        {
            CumulativeDamageTaken += amount;
            CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
            HealthChanged?.Invoke(CurrentHealth, BaseHealth);
        }

        public void Heal(int amount)
        {
            CurrentHealth = Mathf.Min(CurrentHealth + amount, BaseHealth);
            HealthChanged?.Invoke(CurrentHealth, BaseHealth);
        }

        public void SetInvincible(bool value) => IsInvincible = value;
        public void UpdateDamageMultiplier(float delta) => _damageMultiplierBonus += delta;
        public void UpdateFireRateMultiplier(float delta) => _fireRateMultiplierBonus += delta;
        public void UpdateMoveSpeedMultiplier(float delta) => _moveSpeedMultiplierBonus += delta;
        public void UpdateProjectileSpeedMultiplier(float delta) => _projectileSpeedMultiplierBonus += delta;

        public void UpdateShotSpread(int deltaCount, float angleDegrees)
        {
            ExtraShotCount = Mathf.Max(0, ExtraShotCount + deltaCount);
            SpreadAngleDegrees = ExtraShotCount > 0 ? angleDegrees : 0f;
        }
    }
}
