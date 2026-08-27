using System;
using System.Collections.Generic;
using System.Threading;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Holds a ship's attacks, spends the shared magazine, spawns the projectiles and runs the
    /// reload. The ship decides which attack fires.
    /// </summary>
    public class ShipWeaponComponent : MonoBehaviour
    {
        [Inject] private readonly ISpawnManager _spawnManager;

        public IReadOnlyList<BaseShipAttackComponent> Attacks => _attacks;
        public BaseShipAttackComponent PrimaryAttack => _attacks.Length > 0 ? _attacks[0] : null;

        [Tooltip("Left empty, every attack under this ship is picked up automatically.")]
        [SerializeField] private BaseShipAttackComponent[] _attacks;

        private ShipStats _stats;
        private string _shooterTag;

        /// <summary>One cooldown for the whole ship. Per-attack clocks would let a rotation fire its
        /// whole set in consecutive frames.</summary>
        private float _nextShotTime;

        private CancellationTokenSource _reloadCancellationTokenSource;

        /// <summary>Kept only to unsubscribe from projectile events on despawn.</summary>
        private readonly List<ProjectileBehaviourComponent> _activeProjectiles = new();

        public event Action ShotFired;
        public event Action<float> ReloadStarted;

        public void Initialize(ShipStats stats, string shooterTag)
        {
            CancelReload();
            UnsubscribeFromStats();

            _stats = stats;
            _stats.UnlimitedAmmoChanged += OnUnlimitedAmmoChanged;
            _shooterTag = shooterTag;
            _nextShotTime = 0f;
        }

        public void Dispose()
        {
            CancelReload();
            UnsubscribeFromStats();

            foreach (ProjectileBehaviourComponent projectile in _activeProjectiles)
            {
                if (projectile != null)
                {
                    projectile.OnProjectileDestroyed -= OnProjectileDestroyed;
                }
            }

            _activeProjectiles.Clear();
            _stats = null;
        }

        /// <summary>Attacks are discovered rather than wired, so one added to a prefab cannot be left
        /// out of the pooling lifecycle.</summary>
        private void Awake()
        {
            if (_attacks == null || _attacks.Length == 0)
            {
                _attacks = GetComponentsInChildren<BaseShipAttackComponent>(true);
            }
        }

        /// <summary>One round is spent per volley however many projectiles it produces, so extra shots
        /// never cost extra ammo.</summary>
        public bool TryFire(BaseShipAttackComponent attack)
        {
            if (attack == null || Time.time < _nextShotTime)
            {
                return false;
            }

            if (!_stats.TryConsumeAmmo())
            {
                return false;
            }

            // The attack that just fired decides how long the ship waits before the next one.
            _nextShotTime = Time.time + attack.GetCooldown(_stats);

            foreach (Vector3 direction in ApplyStatsShotSpread(attack, attack.GetShotDirections()))
            {
                foreach (Transform muzzle in attack.Muzzles)
                {
                    FireProjectile(attack, muzzle.position, direction);
                }
            }

            ShotFired?.Invoke();

            if (_stats.IsOutOfAmmo)
            {
                StartReload();
            }

            return true;
        }

        public void Reload()
        {
            if (_stats == null || _stats.HasUnlimitedAmmo || _reloadCancellationTokenSource != null)
            {
                return;
            }

            if (_stats.CurrentAmmo >= _stats.CurrentMaxAmmo)
            {
                return;
            }

            StartReload();
        }

        public void DelayNextShot(float delay)
        {
            _nextShotTime = Time.time + delay;
        }

        private void UnsubscribeFromStats()
        {
            if (_stats == null)
            {
                return;
            }

            _stats.UnlimitedAmmoChanged -= OnUnlimitedAmmoChanged;
        }

        /// <summary>Finishes any running reload and refills, on both edges of the toggle.</summary>
        private void OnUnlimitedAmmoChanged(bool hasUnlimitedAmmo)
        {
            CancelReload();
            _stats.RefillAmmo();
        }

        private void CancelReload()
        {
            _reloadCancellationTokenSource?.CancelAndDispose();
            _reloadCancellationTokenSource = null;
        }

        private IEnumerable<Vector3> ApplyStatsShotSpread(BaseShipAttackComponent attack, IEnumerable<Vector3> baseDirections)
        {
            if (!attack.AllowsStatsShotSpread || _stats.ExtraShotCount <= 0)
            {
                foreach (Vector3 direction in baseDirections)
                {
                    yield return direction;
                }
                yield break;
            }

            int totalShots = _stats.ExtraShotCount + 1;
            float startAngle = -(_stats.SpreadAngleDegrees * (totalShots - 1)) / 2f;

            foreach (Vector3 baseDirection in baseDirections)
            {
                for (int i = 0; i < totalShots; i++)
                {
                    float angle = startAngle + i * _stats.SpreadAngleDegrees;
                    yield return Quaternion.Euler(0f, angle, 0f) * baseDirection;
                }
            }
        }

        private void FireProjectile(BaseShipAttackComponent attack, Vector3 muzzleWorldPosition, Vector3 direction)
        {
            if (attack.ProjectilePrefab == null)
            {
                this.LogError($"No projectile prefab assigned on attack '{attack.name}'!");
                return;
            }

            var projectile = _spawnManager.SpawnProjectile(
                attack.ProjectilePrefab,
                muzzleWorldPosition,
                direction,
                attack.CreateAttackSource(_stats),
                _shooterTag
            );

            if (projectile != null)
            {
                _activeProjectiles.Add(projectile);
                projectile.OnProjectileDestroyed += OnProjectileDestroyed;
            }
        }

        private void OnProjectileDestroyed(ProjectileBehaviourComponent projectile)
        {
            projectile.OnProjectileDestroyed -= OnProjectileDestroyed;
            _activeProjectiles.Remove(projectile);
        }

        /// <summary>The delay is time scaled, so a reload halts with the rest of the game while paused.</summary>
        private void StartReload()
        {
            CancelReload();

            _reloadCancellationTokenSource = new CancellationTokenSource();
            ReloadStarted?.Invoke(_stats.CurrentReloadDuration);
            RunReload(_stats.CurrentReloadDuration, _reloadCancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid RunReload(float duration, CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);

            if (token.IsCancellationRequested)
            {
                return;
            }

            CancelReload();
            _stats.RefillAmmo();
        }
    }
}
