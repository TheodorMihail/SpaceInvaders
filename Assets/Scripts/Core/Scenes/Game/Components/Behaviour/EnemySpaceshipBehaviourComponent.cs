using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>Reinforcements a ship can call for, as authored on the ship itself.</summary>
    [Serializable]
    public struct EnemySpawnDTO
    {
        public EnemyTypes EnemyType;

        [Tooltip("Ships spawned per request. 0 opts out.")]
        public int Count;

        [Tooltip("Sideways gap between them, so they fan out instead of stacking up.")]
        public float Spread;
    }

    /// <summary>One call for reinforcements and where they belong. The owning manager does the
    /// spawning, so they end up tracked like any other enemy.</summary>
    public readonly struct EnemySpawnRequestDTO
    {
        public EnemySpawnDTO Spawn { get; }
        public Vector3 LocalPosition { get; }

        public EnemySpawnRequestDTO(EnemySpawnDTO spawn, Vector3 localPosition)
        {
            Spawn = spawn;
            LocalPosition = localPosition;
        }
    }

    public interface IEnemySpaceship : ISpaceship
    {
        new event Action<IEnemySpaceship> OnDestroyed;
        event Action<IEnemySpaceship, EnemySpawnRequestDTO> OnSpawnRequested;
        EnemyTypes EnemyType { get; }
        EnemyCategoryTypes Category { get; }

        /// <summary>Works out where this ship lands and returns how far it has to travel. Depth ratio
        /// places it within the formation: 0 lands at the top of the play area, 1 lands deepest.</summary>
        float PrepareEntry(float formationDepthRatio);

        /// <summary>Flies to the prepared spot. The whole wave shares one duration, so the formation
        /// arrives together instead of trickling in.</summary>
        void StartEntryAnimation(float duration);

        /// <summary>Starts fighting straight away, for ships that appear in place rather than flying in.</summary>
        void SkipEntry();
    }

    /// <summary>
    /// Enemy ship with two movement phases: a tweened entry towards the top of the screen, followed
    /// by bouncing movement within the top half bounds.
    /// </summary>
    public class EnemySpaceshipBehaviourComponent : BaseSpaceshipBehaviourComponent<EnemySpaceshipConfigSO>, IEnemySpaceship
    {
        private enum EnemyState { Entering, Bouncing }

        public EnemyTypes EnemyType => ShipConfig.EnemyType;
        public EnemyCategoryTypes Category => ShipConfig.Category;

        [Tooltip("How much of the play area a formation may occupy in depth. Lower keeps waves nearer the top.")]
        [SerializeField, Range(0f, 1f)] private float _formationDepthFactor = 0.6f;

        [Tooltip("Upper bound of the random wait before the first shot, so a wave does not fire in unison.")]
        [SerializeField] private float _maxFirstShotDelay = 3f;

        /// <summary>Blocks damage until the entry animation completes.</summary>
        protected virtual bool IsInvulnerableWhileEntering => false;

        private EnemyState _currentState = EnemyState.Entering;
        private Vector3 _entryTargetPosition;
        private Tween _entryTween;

        public new event Action<IEnemySpaceship> OnDestroyed;
        public event Action<IEnemySpaceship, EnemySpawnRequestDTO> OnSpawnRequested;

        protected override void Destroy()
        {
            SpawnDestroyVFX();
            OnDestroyed?.Invoke(this);
        }

        public override void OnDespawned()
        {
            base.OnDespawned();

            _entryTween?.Kill();
            _entryTween = null;

            // The movement's own Dispose, run by the base, is what clears the bounce direction.
            _currentState = EnemyState.Entering;
        }

        public float PrepareEntry(float formationDepthRatio)
        {
            Vector3 minBounds = _movement.MinBounds;
            Vector3 maxBounds = _movement.MaxBounds;

            // Landing straight onto the movement bounds keeps the formation inside the playable
            // area, including the margins reserved for UI.
            _entryTargetPosition = transform.position;
            _entryTargetPosition.x = Mathf.Clamp(_entryTargetPosition.x, minBounds.x, maxBounds.x);

            // Ships further back in the formation land deeper, so the shape survives the entry.
            float depth = Mathf.Clamp01(formationDepthRatio) * _formationDepthFactor;
            _entryTargetPosition.z = Mathf.Lerp(maxBounds.z, minBounds.z, depth);

            _currentState = EnemyState.Entering;

            if (IsInvulnerableWhileEntering)
            {
                Stats.SetInvincible(true);
            }

            return Vector3.Distance(transform.position, _entryTargetPosition);
        }

        /// <summary>Speed is whatever covers this ship's distance in the shared wave duration, so
        /// ships further out simply fly faster and the formation lands as one.</summary>
        public void StartEntryAnimation(float duration)
        {
            if (duration <= 0f)
            {
                transform.position = _entryTargetPosition;
                OnEntryComplete();
                return;
            }

            // The dive into formation is the only time an enemy actually accelerates.
            SetFlamesThrusting(true);

            _entryTween = transform.DOMove(_entryTargetPosition, duration)
                .SetEase(Ease.Linear)
                .OnComplete(OnEntryComplete);
        }

        /// <summary>Split and summoned ships are placed where they are needed, so there is no entry
        /// to fly and nothing to be invulnerable through.</summary>
        public void SkipEntry()
        {
            _entryTween?.Kill();
            _entryTween = null;

            OnEntryComplete();
        }

        protected void RaiseSpawnRequest(EnemySpawnRequestDTO request)
        {
            OnSpawnRequested?.Invoke(this, request);
        }

        private void OnEntryComplete()
        {
            SetFlamesThrusting(false);
            _currentState = EnemyState.Bouncing;

            if (IsInvulnerableWhileEntering)
            {
                Stats.SetInvincible(false);
            }

            _weapon.DelayNextShot(Random.Range(0f, _maxFirstShotDelay));
            _movement.StartMoving();
        }

        private void Update()
        {
            if (_currentState == EnemyState.Bouncing)
            {
                _movement.Tick();
                Shoot(); // Continuously attempt to shoot (fire rate cooldown handled in base)
            }
        }
    }
}
