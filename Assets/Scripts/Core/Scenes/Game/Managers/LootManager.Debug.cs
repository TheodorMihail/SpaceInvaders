#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace SpaceInvaders.Scenes.Game
{
    public partial class LootManager : IDebugCommandProvider
    {
        /// <summary>Keeps the drop clear of the side edges, where half of it would be off screen.</summary>
        private const float DebugDropEdgeInset = 0.1f;

        [Inject] private readonly IPowerupsRepository _powerupsRepository;
        [Inject] private readonly ICameraManager _cameraManager;

        private int _debugPowerupIndex;

        public IReadOnlyList<DebugCommandDTO> GetDebugCommands()
        {
            return new[]
            {
                new DebugCommandDTO(DebugKeys.SpawnPowerup, "Spawn the next powerup", DebugSpawnPowerup)
            };
        }

        /// <summary>Steps through the authored powerups rather than rolling one, so a specific powerup
        /// is always a known number of presses away.</summary>
        private void DebugSpawnPowerup()
        {
            IReadOnlyList<PowerupConfigSO> configs = _powerupsRepository.GetAllPowerupConfigs();

            if (configs.Count == 0)
            {
                this.LogWarning("Debug: no powerup config authored");
                return;
            }

            PowerupConfigSO config = configs[_debugPowerupIndex % configs.Count];
            _debugPowerupIndex++;

            this.LogWarning($"Debug: Spawning powerup {config.PowerupType}");
            SpawnPowerupDrop(config, GetDebugDropLocalPosition());
        }

        /// <summary>In over the top edge at a random point along it, so it falls the way a real drop
        /// does. The prefab's plane is authored container-local and the view answers in world space,
        /// so it goes out and comes back the same way, landing on exactly the plane it started on.</summary>
        private Vector3 GetDebugDropLocalPosition()
        {
            Vector3 prefabLocalPosition = _powerupsRepository.GetPowerupPickupPrefab().transform.localPosition;
            float planeY = _spawnManager.GetContainerWorldPosition(prefabLocalPosition).y;

            (Vector3 left, Vector3 right) = _cameraManager.GetTopEdgeBounds(planeY, DebugDropEdgeInset);
            var worldPosition = new Vector3(Random.Range(left.x, right.x), planeY, left.z);

            return _spawnManager.GetContainerLocalPosition(worldPosition);
        }
    }
}
#endif
