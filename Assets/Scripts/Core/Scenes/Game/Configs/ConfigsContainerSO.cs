using SpaceInvaders.Project;
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
        [SerializeField] private ProjectDataConfigSO _projectDataConfigSO;
        [SerializeField] private TalentsDataConfigSO _talentsDataConfigSO;
        [SerializeField] private SoundsDataConfigSO _soundsDataConfigSO;
        [SerializeField] private ItemsDataConfigSO _itemsDataConfigSO;
        [SerializeField] private DropTableConfigSO _dropTableConfigSO;

        public LevelsDataConfigSO LevelsDataConfigSO => _levelsDataConfigSO;
        public PlayerDataConfigSO PlayerDataConfigSO => _playerDataConfigSO;
        public EnemyDataConfigSO EnemyDataConfigSO => _enemyDataConfigSO;
        public PowerupsDataConfigSO PowerupsDataConfigSO => _powerupsDataConfigSO;
        public ProjectDataConfigSO ProjectDataConfigSO => _projectDataConfigSO;
        public TalentsDataConfigSO TalentsDataConfigSO => _talentsDataConfigSO;
        public SoundsDataConfigSO SoundsDataConfigSO => _soundsDataConfigSO;
        public ItemsDataConfigSO ItemsDataConfigSO => _itemsDataConfigSO;
        public DropTableConfigSO DropTableConfigSO => _dropTableConfigSO;
    }
}