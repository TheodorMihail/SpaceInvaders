using System.Collections.Generic;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// What fired a shot: the attacker's stats plus the firing attack's multipliers. The stats are a
    /// live reference so bonuses applied mid-flight still count; the multipliers are constants.
    /// </summary>
    public readonly struct AttackSourceDTO
    {
        public ShipStats AttackerStats { get; }
        public float DamageMultiplier { get; }
        public float ProjectileSpeedMultiplier { get; }

        public float ProjectileSpeed => AttackerStats != null
            ? AttackerStats.CurrentProjectileSpeed * ProjectileSpeedMultiplier
            : 0f;

        public AttackSourceDTO(ShipStats attackerStats, float damageMultiplier, float projectileSpeedMultiplier)
        {
            AttackerStats = attackerStats;
            DamageMultiplier = damageMultiplier;
            ProjectileSpeedMultiplier = projectileSpeedMultiplier;
        }

        /// <summary>Unscaled source, for damage that has no attack behind it.</summary>
        public static AttackSourceDTO FromStats(ShipStats attackerStats)
        {
            return new AttackSourceDTO(attackerStats, 1f, 1f);
        }

        public int RollDamage(out bool isCritical)
        {
            if (AttackerStats == null)
            {
                isCritical = false;
                return 0;
            }

            return AttackerStats.RollOutgoingDamage(DamageMultiplier, out isCritical);
        }
    }

    /// <summary>
    /// One attack a ship can fire: its own projectile, damage and cooldown multipliers. A ship holds
    /// one per attack. Holds no runtime state, so there is nothing for pooling to reset.
    /// </summary>
    public abstract class BaseShipAttackComponent : MonoBehaviour
    {
        private const float MinFireRateMultiplier = 0.01f;

        public ProjectileBehaviourComponent ProjectilePrefab => _projectilePrefab;
        public IReadOnlyList<Transform> Muzzles => _muzzles;
        public bool AllowsStatsShotSpread => _allowsStatsShotSpread;

        protected Vector3 BaseDirection => _baseDirection;

        [SerializeField] private ProjectileBehaviourComponent _projectilePrefab;

        [Tooltip("Barrels this attack fires from. Every muzzle fires the whole pattern at once, so two " +
                 "entries give a twin-barrel volley. Left empty, the attack's own position is the barrel.")]
        [SerializeField] private Transform[] _muzzles;

        [Tooltip("Where this attack points before its own pattern is applied. Forward for the player, back for enemies.")]
        [SerializeField] private Vector3 _baseDirection = Vector3.forward;

        [Header("Stat Multipliers")]
        [SerializeField] private float _damageMultiplier = 1f;
        [SerializeField] private float _fireRateMultiplier = 1f;
        [SerializeField] private float _projectileSpeedMultiplier = 1f;

        [Tooltip("Whether the SpreadShot powerup fans this attack out. Off for attacks that already fill the screen.")]
        [SerializeField] private bool _allowsStatsShotSpread = true;

        /// <summary>An attack with no barrels authored fires from its own transform.</summary>
        private void Awake()
        {
            if (_muzzles == null || _muzzles.Length == 0)
            {
                _muzzles = new[] { transform };
            }
        }

        /// <summary>The ship's cooldown after firing this attack. Fire rate is a cooldown, so a
        /// higher multiplier divides it, so a higher value means faster, as with a FireRate bonus.
        /// A heavy attack leaves the ship idle for longer than a light one.</summary>
        public float GetCooldown(ShipStats stats)
        {
            return stats.CurrentFireRate / Mathf.Max(_fireRateMultiplier, MinFireRateMultiplier);
        }

        /// <summary>The ship's live stats scaled by this attack's own multipliers.</summary>
        public AttackSourceDTO CreateAttackSource(ShipStats stats)
        {
            return new AttackSourceDTO(stats, _damageMultiplier, _projectileSpeedMultiplier);
        }

        /// <summary>The shape of one volley. Each direction becomes a projectile.</summary>
        public abstract IEnumerable<Vector3> GetShotDirections();
    }
}
