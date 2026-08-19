using System;
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
    public interface IHazardsService : IDisposable, IGameEndListener
    {
        /// <summary>Replaces whatever the previous wave was sending in with this wave's own hazards.</summary>
        void StartWaveHazards(WaveConfigDTO waveConfig);
        void StopHazards();
        void NotifyHazardDestroyed(HazardTypes hazardType, Vector3 localPosition);
    }

    /// <summary>
    /// Runs the hazard cadence a wave asks for. Owns only the timing: the hazards themselves are
    /// transients that the spawn service cleans up.
    /// </summary>
    public partial class HazardsService : IHazardsService
    {
        [Inject] private readonly IHazardsRepository _hazardsRepository;
        [Inject] private readonly ISpawnService _spawnService;
        [Inject] private readonly IMessageBus _messageBus;

        /// <summary>Scoped to the current wave, so starting the next one drops the previous cadence.</summary>
        private CancellationTokenSource _spawnCancellationTokenSource;

        public void Dispose()
        {
            StopHazards();
        }

        public UniTask GameEnd()
        {
            StopHazards();
            return UniTask.CompletedTask;
        }

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

        /// <summary>Published rather than dropped on directly, so the loot manager stays the single
        /// authority on what a kill pays out.</summary>
        public void NotifyHazardDestroyed(HazardTypes hazardType, Vector3 localPosition)
        {
            _messageBus.Publish(new HazardDestroyedMessage(hazardType, localPosition));
        }

        /// <summary>The wait between hazards is time scaled, so pausing holds the next one with the
        /// rest of the game instead of letting it land in the next scene.</summary>
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

        /// <summary>Enters at a random point along the spawn edge and angles back towards the middle,
        /// so it crosses the play area rather than clipping the corner it came in at.</summary>
        private void SpawnHazard(HazardConfigSO config)
        {
            float entryRatio = Random.value;
            float lateralDrift = Random.Range(config.BaseStats.MinLateralDrift, config.BaseStats.MaxLateralDrift);
            float lateralSign = entryRatio < 0.5f ? 1f : -1f;

            _spawnService.SpawnHazard(config, new Vector3(lateralSign * lateralDrift, 0f, -1f), entryRatio);
        }
    }
}
