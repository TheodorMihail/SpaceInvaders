using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Project
{
    public interface IRepositoryManager
    {
        //Project
        ProjectDataConfigSO GetProjectDataConfig();

        //Levels
        LevelConfigSO GetLevelConfig(int level);
        IReadOnlyList<LevelConfigSO> GetLevelConfigs();
        int GetLevelsCount();
        float GetTwoStarDamageMultiplier();

        //Ships
        PlayerSpaceshipConfigSO GetPlayerConfig(PlayerTypes playerType);
        EnemySpaceshipConfigSO GetEnemyConfig(EnemyTypes enemyType);
        
        //Powerups
        PowerupConfigSO GetPowerupConfig(PowerupTypes powerupType);
        IReadOnlyList<PowerupConfigSO> GetAllPowerupConfigs();

        //Drops
        IReadOnlyList<DropCategoryWeightDTO> GetAllDropCategoryWeights();

        //Talents
        TalentConfigSO GetTalentConfig(ShipUpgradableStatTypes talentType);
        IReadOnlyList<TalentConfigSO> GetAllTalentConfigs();

        //Sounds
        SoundConfigSO GetSoundConfig(SoundTypes soundType);

        //Items
        ItemConfigSO GetItemConfig(string itemId);
        IReadOnlyList<ItemConfigSO> GetAllItemConfigs();
        ItemRarityConfigSO GetItemRarityConfig(ItemRarityTypes rarity);
        IReadOnlyList<ItemRarityConfigSO> GetAllItemRarityConfigs();
        IReadOnlyList<EquipmentSlotConfigDTO> GetAllEquipmentSlotConfigs();
    }

    public class RepositoryManager : Repository, IRepositoryManager
    {
        public RepositoryManager(
            LevelsDataConfigSO levelsDataConfigSO,
            PlayerDataConfigSO playerDataConfigSO,
            EnemyDataConfigSO enemyDataConfigSO,
            PowerupsDataConfigSO powerupsDataConfigSO,
            ProjectDataConfigSO projectDataConfigSO,
            TalentsDataConfigSO talentsDataConfigSO,
            SoundsDataConfigSO soundsDataConfigSO,
            ItemsDataConfigSO itemsDataConfigSO,
            DropTableConfigSO dropTableConfigSO)
        {
            AddObjects(levelsDataConfigSO.LevelsConfigs);
            AddObjects(playerDataConfigSO.PlayerConfigs);
            AddObjects(enemyDataConfigSO.EnemyConfigs);
            AddObjects(powerupsDataConfigSO.PowerupConfigs);
            AddObjects(talentsDataConfigSO.TalentConfigs);
            AddObjects(soundsDataConfigSO.SoundConfigs);
            AddObjects(itemsDataConfigSO.ItemConfigs);
            AddObjects(itemsDataConfigSO.RarityConfigs);

            AddObject(levelsDataConfigSO);
            AddObject(playerDataConfigSO);
            AddObject(enemyDataConfigSO);
            AddObject(powerupsDataConfigSO);
            AddObject(projectDataConfigSO);
            AddObject(talentsDataConfigSO);
            AddObject(soundsDataConfigSO);
            AddObject(itemsDataConfigSO);
            AddObject(dropTableConfigSO);
        }

        #region Project

        public ProjectDataConfigSO GetProjectDataConfig()
        {
            return Get<ProjectDataConfigSO>(nameof(ProjectDataConfigSO));
        }

        #endregion

        #region Levels

        public LevelConfigSO GetLevelConfig(int level)
        {
            return Get<LevelConfigSO>($"Level {level}");
        }

        public IReadOnlyList<LevelConfigSO> GetLevelConfigs()
        {
            return GetAll<LevelConfigSO>().ToArray();
        }

        public int GetLevelsCount()
        {
            return GetAll<LevelConfigSO>().Count();
        }

        public float GetTwoStarDamageMultiplier()
        {
            return Get<LevelsDataConfigSO>(nameof(LevelsDataConfigSO)).TwoStarDamageMultiplier;
        }

        #endregion

        #region Ships

        public PlayerSpaceshipConfigSO GetPlayerConfig(PlayerTypes playerType)
        {
            return Get<PlayerSpaceshipConfigSO>(playerType.ToString());
        }

        public EnemySpaceshipConfigSO GetEnemyConfig(EnemyTypes enemyType)
        {
            return Get<EnemySpaceshipConfigSO>(enemyType.ToString());
        }

        #endregion

        #region Powerups

        public PowerupConfigSO GetPowerupConfig(PowerupTypes powerupType)
        {
            return Get<PowerupConfigSO>(powerupType.ToString());
        }

        public IReadOnlyList<PowerupConfigSO> GetAllPowerupConfigs()
        {
            return GetAll<PowerupConfigSO>().ToArray();
        }

        #endregion

        #region Drops

        public IReadOnlyList<DropCategoryWeightDTO> GetAllDropCategoryWeights()
        {
            return Get<DropTableConfigSO>(nameof(DropTableConfigSO)).CategoryWeights;
        }

        #endregion

        #region Sounds

        public SoundConfigSO GetSoundConfig(SoundTypes soundType)
        {
            return Get<SoundConfigSO>(soundType.ToString());
        }

        #endregion

        #region Talents

        public TalentConfigSO GetTalentConfig(ShipUpgradableStatTypes talentType)
        {
            return Get<TalentConfigSO>(talentType.ToString());
        }

        public IReadOnlyList<TalentConfigSO> GetAllTalentConfigs()
        {
            return GetAll<TalentConfigSO>().ToArray();
        }

        #endregion

        #region Items

        public ItemConfigSO GetItemConfig(string itemId)
        {
            return Get<ItemConfigSO>(itemId);
        }

        public IReadOnlyList<ItemConfigSO> GetAllItemConfigs()
        {
            return GetAll<ItemConfigSO>().ToArray();
        }

        public ItemRarityConfigSO GetItemRarityConfig(ItemRarityTypes rarity)
        {
            return Get<ItemRarityConfigSO>(rarity.ToString());
        }

        public IReadOnlyList<ItemRarityConfigSO> GetAllItemRarityConfigs()
        {
            return GetAll<ItemRarityConfigSO>().ToArray();
        }

        public IReadOnlyList<EquipmentSlotConfigDTO> GetAllEquipmentSlotConfigs()
        {
            return Get<ItemsDataConfigSO>(nameof(ItemsDataConfigSO)).SlotConfigs;
        }
    
        #endregion
    }
}
