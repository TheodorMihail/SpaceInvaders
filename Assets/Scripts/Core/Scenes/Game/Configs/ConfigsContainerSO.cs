using System.Collections.Generic;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "ConfigsContainer", menuName = "SpaceInvaders/Configs Container")]
    public class ConfigsContainerSO : ScriptableObject
    {
        [Header("Configs")]
        [SerializeField] private List<LevelConfigSO> _levelsConfigsSO;
        [SerializeField] private List<PlayerSpaceshipConfigSO> _playerConfigsSO;
        [SerializeField] private List<EnemySpaceshipConfigSO> _enemyConfigsSO;
        [SerializeField] private List<PowerupConfigSO> _powerupConfigsSO;

        [Header("Powerup Settings")]
        [SerializeField, Range(0f, 1f)] private float _globalPowerupDropChance = 0.15f;

        public List<LevelConfigSO> LevelsConfigsSO => _levelsConfigsSO;
        public List<PlayerSpaceshipConfigSO> PlayerConfigsSO => _playerConfigsSO;
        public List<EnemySpaceshipConfigSO> EnemyConfigsSO => _enemyConfigsSO;
        public List<PowerupConfigSO> PowerupConfigsSO => _powerupConfigsSO;
        public float GlobalPowerupDropChance => _globalPowerupDropChance;
    }
}