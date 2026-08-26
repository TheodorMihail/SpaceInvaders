using System.Collections.Generic;
using System.Threading;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Project;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace SpaceInvaders.Scenes.Game
{
    public interface IHazardsService
    {
        /// <summary>Replaces whatever the previous wave was sending in with this wave's own hazards.</summary>
        void StartWaveHazards(WaveConfigDTO waveConfig);
        void StopHazards();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        void DebugSpawnFirstHazard();
#endif
    }

    /// <summary>
    /// Runs a wave's hazard spawn timing. Owns only the timing: the hazards are transients cleaned up
    /// by the spawn manager.
    /// </summary>
    public partial class HazardsService : IHazardsService
    {
        [Inject] private readonly IHazardsRepository _hazardsRepository;
        [Inject] private readonly ISpawnManager _spawnManager;
        [Inject] private readonly IMessageBus _messageBus;

        /// <summary>Scoped to the current wave, so starting the next one drops the previous cadence.</summary>
        private CancellationTokenSource _spawnCancellationTokenSource;

        public void StartWaveHazards(WaveConfigDTO waveConfig)
        {
            StopHazards();

            List<WaveConfigDTO.WaveHazardDTO> waveHazards = waveConfig.WaveHazards;
            if (waveHazards.Count == 0)
            {
                return;
            }

            _spawnCancellationTokenSource = new CancellationTokenSource();

            foreach (WaveConfigDTO.WaveHazardDTO waveHazard in waveHazards)
            {
                RunHazardLoop(waveHazard, _spawnCancellationTokenSource.Token).Forget();
            }
        }

        public void StopHazards()
        {
            _spawnCancellationTokenSource?.CancelAndDispose();
            _spawnCancellationTokenSource = null;
        }

        private void OnHazardDestroyedCallback(HazardTypes hazardType, Vector3 localPosition)
        {
            _messageBus.Publish(new HazardDestroyedMessage(hazardType, localPosition));
        }

        /// <summary>The wait between hazards is time scaled, so it halts with the rest of the game
        /// while paused instead of spawning into the next scene.</summary>
        private async UniTaskVoid RunHazardLoop(WaveConfigDTO.WaveHazardDTO waveHazard, CancellationToken token)
        {
            if (!_hazardsRepository.TryGetHazardConfig(waveHazard.HazardType, out HazardConfigSO config)
                || config.HazardPrefab == null)
            {
                return;
            }

            await UniTask.Delay((int)(waveHazard.FirstSpawnDelay * 1000), cancellationToken: token);

            SpawnHazard(config);

            // An unset interval means this wave wanted exactly one, not one every frame.
            if (waveHazard.MaxSpawnInterval <= 0f)
            {
                return;
            }

            while (!token.IsCancellationRequested)
            {
                float interval = Random.Range(waveHazard.MinSpawnInterval, waveHazard.MaxSpawnInterval);
                await UniTask.Delay((int)(interval * 1000), cancellationToken: token);

                SpawnHazard(config);
            }
        }

        /// <summary>Enters at a random point along the edge, angled back towards the middle so it
        /// crosses the play area instead of clipping the corner.</summary>
        private void SpawnHazard(HazardConfigSO config)
        {
            float entryRatio = Random.value;
            float lateralDrift = Random.Range(config.BaseStats.MinLateralDrift, config.BaseStats.MaxLateralDrift);
            float lateralSign = entryRatio < 0.5f ? 1f : -1f;

            BaseHazardBehaviourComponent hazard =
                _spawnManager.SpawnHazard(config, new Vector3(lateralSign * lateralDrift, 0f, -1f), entryRatio);

            if (hazard == null)
            {
                return;
            }

            hazard.OnDestroyed += OnHazardDestroyedCallback;
        }
    }
}
