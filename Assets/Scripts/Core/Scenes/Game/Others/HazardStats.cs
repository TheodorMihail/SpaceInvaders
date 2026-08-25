using System;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Hazard tuning, authored inside the hazard config. None of it is upgradable, so these are plain
    /// values rather than StatValues.
    /// </summary>
    [Serializable]
    public class HazardBaseStats
    {
        [SerializeField] private int _health = 60;
        [SerializeField] private float _speed = 45f;

        [Tooltip("Dealt to the player on contact.")]
        [SerializeField] private int _contactDamage = 50;

        [Tooltip("Whether shots can break it. An indestructible hazard still stops fire, so it shields " +
                 "whatever sits behind it, and can only ever be avoided.")]
        [SerializeField] private bool _isDestructible = true;

        [Tooltip("Sideways travel per unit of forward travel. Higher cuts a shallower diagonal.")]
        [SerializeField] private float _minLateralDrift = 0.25f;
        [SerializeField] private float _maxLateralDrift = 0.8f;

        public int BaseHealth => _health;
        public float BaseSpeed => _speed;
        public int BaseContactDamage => _contactDamage;
        public bool IsDestructible => _isDestructible;
        public float MinLateralDrift => _minLateralDrift;
        public float MaxLateralDrift => _maxLateralDrift;
    }

    /// <summary>
    /// One hazard instance's live state. Created fresh per spawn, so pooled hazards start clean.
    /// </summary>
    public class HazardStats
    {
        public int MaxHealth { get; }
        public float Speed { get; }
        public int ContactDamage { get; }
        public bool IsDestructible { get; }

        public int CurrentHealth { get; private set; }

        /// <summary>An indestructible hazard is never destroyed, whatever damage it takes.</summary>
        public bool IsDestroyed => IsDestructible && CurrentHealth <= 0;

        public HazardStats(HazardBaseStats baseStats)
        {
            MaxHealth = baseStats.BaseHealth;
            Speed = baseStats.BaseSpeed;
            ContactDamage = baseStats.BaseContactDamage;
            IsDestructible = baseStats.IsDestructible;

            CurrentHealth = MaxHealth;
        }

        public void ApplyDamage(int amount)
        {
            CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        }
    }
}
