using SpaceInvaders.Project;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "ConfigsContainer", menuName = "SpaceInvaders/Configs Container")]
    public class ConfigsContainerSO : ScriptableObject
    {
        [Header("Project")]
        [SerializeField] private ProjectDataConfigSO _projectDataConfigSO;
        [SerializeField] private SoundsDataConfigSO _soundsDataConfigSO;

        [Header("Game Modes")]
        [SerializeField] private CampaignDataConfigSO _campaignDataConfigSO;
        [SerializeField] private ExpeditionDataConfigSO _expeditionDataConfigSO;

        [Header("Gameplay")]
        [SerializeField] private GameDataConfigSO _gameDataConfigSO;
        [SerializeField] private LevelsDataConfigSO _levelsDataConfigSO;
        [SerializeField] private PowerupsDataConfigSO _powerupsDataConfigSO;
        [SerializeField] private HazardsDataConfigSO _hazardsDataConfigSO;

        [Header("Ships")]
        [SerializeField] private PlayerDataConfigSO _playerDataConfigSO;
        [SerializeField] private EnemyDataConfigSO _enemyDataConfigSO;

        [Header("Progression")]
        [SerializeField] private ItemsDataConfigSO _itemsDataConfigSO;
        [SerializeField] private TalentRaritiesDataConfigSO _talentRaritiesDataConfigSO;

        public ProjectDataConfigSO ProjectDataConfigSO => _projectDataConfigSO;
        public SoundsDataConfigSO SoundsDataConfigSO => _soundsDataConfigSO;

        public CampaignDataConfigSO CampaignDataConfigSO => _campaignDataConfigSO;
        public ExpeditionDataConfigSO ExpeditionDataConfigSO => _expeditionDataConfigSO;

        public GameDataConfigSO GameDataConfigSO => _gameDataConfigSO;
        public LevelsDataConfigSO LevelsDataConfigSO => _levelsDataConfigSO;
        public PowerupsDataConfigSO PowerupsDataConfigSO => _powerupsDataConfigSO;
        public HazardsDataConfigSO HazardsDataConfigSO => _hazardsDataConfigSO;

        public PlayerDataConfigSO PlayerDataConfigSO => _playerDataConfigSO;
        public EnemyDataConfigSO EnemyDataConfigSO => _enemyDataConfigSO;

        public ItemsDataConfigSO ItemsDataConfigSO => _itemsDataConfigSO;
        public TalentRaritiesDataConfigSO TalentRaritiesDataConfigSO => _talentRaritiesDataConfigSO;
    }
}
