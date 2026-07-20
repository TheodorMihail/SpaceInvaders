using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "ConfigsContainer", menuName = "SpaceInvaders/Configs Container")]
    public class ConfigsContainerSO : ScriptableObject
    {
        [Header("Configs")]
        [SerializeField] private LevelsDataConfigSO _levelsDataConfigSO;
        [SerializeField] private PlayerDataConfigSO _playerDataConfigSO;
        [SerializeField] private EnemyDataConfigSO _enemyDataConfigSO;
        [SerializeField] private PowerupsDataConfigSO _powerupsDataConfigSO;

        public LevelsDataConfigSO LevelsDataConfigSO => _levelsDataConfigSO;
        public PlayerDataConfigSO PlayerDataConfigSO => _playerDataConfigSO;
        public EnemyDataConfigSO EnemyDataConfigSO => _enemyDataConfigSO;
        public PowerupsDataConfigSO PowerupsDataConfigSO => _powerupsDataConfigSO;
    }
}