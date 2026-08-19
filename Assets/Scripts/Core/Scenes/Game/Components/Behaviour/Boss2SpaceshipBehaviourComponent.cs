using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Calls in reinforcements as it loses health, so the fight keeps changing shape instead of
    /// settling into one rhythm the player can park against.
    /// </summary>
    public class Boss2SpaceshipBehaviourComponent : BossSpaceshipBehaviourComponent
    {
        [Header("Reinforcements")]
        [Tooltip("Ships called in each time a health threshold is passed.")]
        [SerializeField] private EnemySpawnDTO _summonSpawn;

        [Tooltip("Remaining health fractions that each trigger one summon. Author these highest first.")]
        [SerializeField] private float[] _summonHealthThresholds = { 0.66f, 0.33f };

        private int _nextThresholdIndex;

        public override void OnSpawned()
        {
            base.OnSpawned();

            // Stats are rebuilt on every spawn, so the hook goes on the instance the base just made.
            _nextThresholdIndex = 0;
            Stats.HealthChanged += OnStatsHealthChangedForSummon;
        }

        public override void OnDespawned()
        {
            if (Stats != null)
            {
                Stats.HealthChanged -= OnStatsHealthChangedForSummon;
            }

            base.OnDespawned();
            _nextThresholdIndex = 0;
        }

        /// <summary>A single hit can pass more than one threshold, and each one it passes still pays,
        /// so chunking the boss down does not skip a phase.</summary>
        private void OnStatsHealthChangedForSummon(int currentHealth, int maxHealth)
        {
            if (_summonSpawn.Count <= 0 || currentHealth <= 0 || maxHealth <= 0)
            {
                return;
            }

            float remainingFraction = (float)currentHealth / maxHealth;

            while (_nextThresholdIndex < _summonHealthThresholds.Length
                && remainingFraction <= _summonHealthThresholds[_nextThresholdIndex])
            {
                _nextThresholdIndex++;
                RaiseSpawnRequest(new EnemySpawnRequestDTO(_summonSpawn, LocalPosition));
            }
        }
    }
}
