using System.Collections.Generic;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// What fired a shot: the attacker's live stats plus the multipliers of the attack that produced
    /// it. The stats stay a live reference so a buff landing mid-flight still counts, while the
    /// multipliers are per-attack constants and are safe to carry by value.
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
    /// One way a ship can shoot: its own projectile, its own damage, and its own share of the ship's
    /// firing cadence. A ship carries as many of these as it has attacks, and its weapon fires
    /// whichever one it is handed. Holds no runtime state, so pooling has nothing to reset here.
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

        /// <summary>An attack authored without barrels fires from wherever it sits, which is the
        /// single-barrel case and needs no wiring.</summary>
        private void Awake()
        {
            if (_muzzles == null || _muzzles.Length == 0)
            {
                _muzzles = new[] { transform };
            }
        }

        /// <summary>How long the ship rests after firing this attack. Fire rate is a cooldown, so a
        /// higher multiplier divides it: more reads as faster, same as a positive FireRate bonus.
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
