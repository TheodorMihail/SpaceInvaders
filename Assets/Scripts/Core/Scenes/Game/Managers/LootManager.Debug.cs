#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public partial class LootManager
    {
        /// <summary>Keeps the drop away from the side edges.</summary>
        private const float DebugDropEdgeInset = 0.1f;

        [Inject] private readonly IPowerupsRepository _powerupsRepository;
        [Inject] private readonly ICameraManager _cameraManager;

        private int _debugPowerupIndex;

        /// <summary>Cycles through the powerups in order instead of rolling a random one.</summary>
        public void DebugSpawnPowerup()
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

        /// <summary>A random point along the top edge. The prefab plane is container-local and the
        /// screen bounds are world, so it is converted out and back to stay on the same plane.</summary>
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
