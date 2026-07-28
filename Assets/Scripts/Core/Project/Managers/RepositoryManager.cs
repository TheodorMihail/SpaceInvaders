using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Project
{
    public interface IRepositoryManager
    {
        LevelConfigSO GetLevelConfig(int level);
        IReadOnlyList<LevelConfigSO> GetLevelConfigs();
        PlayerSpaceshipConfigSO GetPlayerConfig(PlayerTypes playerType);
        EnemySpaceshipConfigSO GetEnemyConfig(EnemyTypes enemyType);
        PowerupConfigSO GetPowerupConfig(PowerupTypes powerupType);
        IReadOnlyList<PowerupConfigSO> GetAllPowerupConfigs();
        float GetPowerupDropChance();
        float GetTwoStarDamageMultiplier();
        int GetLevelsCount();
        ProjectDataConfigSO GetProjectDataConfig();
        TalentConfigSO GetTalentConfig(ShipUpgradableStatTypes talentType);
        IReadOnlyList<TalentConfigSO> GetAllTalentConfigs();
        SoundConfigSO GetSoundConfig(SoundTypes soundType);
        ItemConfigSO GetItemConfig(string itemId);
        IReadOnlyList<ItemConfigSO> GetAllItemConfigs();
        ItemRarityConfigSO GetItemRarityConfig(ItemRarityTypes rarity);
        IReadOnlyList<ItemRarityConfigSO> GetAllItemRarityConfigs();
        float GetItemDropChance();
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
            ItemsDataConfigSO itemsDataConfigSO)
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
        }

        public LevelConfigSO GetLevelConfig(int level)
        {
            return Get<LevelConfigSO>($"Level {level}");
        }

        public IReadOnlyList<LevelConfigSO> GetLevelConfigs()
        {
            return GetAll<LevelConfigSO>().ToArray();
        }

        public PlayerSpaceshipConfigSO GetPlayerConfig(PlayerTypes playerType)
        {
            return Get<PlayerSpaceshipConfigSO>(playerType.ToString());
        }

        public EnemySpaceshipConfigSO GetEnemyConfig(EnemyTypes enemyType)
        {
            return Get<EnemySpaceshipConfigSO>(enemyType.ToString());
        }

        public PowerupConfigSO GetPowerupConfig(PowerupTypes powerupType)
        {
            return Get<PowerupConfigSO>(powerupType.ToString());
        }

        public IReadOnlyList<PowerupConfigSO> GetAllPowerupConfigs()
        {
            return GetAll<PowerupConfigSO>().ToArray();
        }

        public float GetPowerupDropChance()
        {
            return Get<PowerupsDataConfigSO>(nameof(PowerupsDataConfigSO)).GlobalPowerupDropChance;
        }

        public float GetTwoStarDamageMultiplier()
        {
            return Get<LevelsDataConfigSO>(nameof(LevelsDataConfigSO)).TwoStarDamageMultiplier;
        }

        public int GetLevelsCount()
        {
            return GetAll<LevelConfigSO>().Count();
        }

        public ProjectDataConfigSO GetProjectDataConfig()
        {
            return Get<ProjectDataConfigSO>(nameof(ProjectDataConfigSO));
        }

        public TalentConfigSO GetTalentConfig(ShipUpgradableStatTypes talentType)
        {
            return Get<TalentConfigSO>(talentType.ToString());
        }

        public IReadOnlyList<TalentConfigSO> GetAllTalentConfigs()
        {
            return GetAll<TalentConfigSO>().ToArray();
        }

        public SoundConfigSO GetSoundConfig(SoundTypes soundType)
        {
            return Get<SoundConfigSO>(soundType.ToString());
        }

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

        public float GetItemDropChance()
        {
            return Get<ItemsDataConfigSO>(nameof(ItemsDataConfigSO)).GlobalItemDropChance;
        }

        public IReadOnlyList<EquipmentSlotConfigDTO> GetAllEquipmentSlotConfigs()
        {
            return Get<ItemsDataConfigSO>(nameof(ItemsDataConfigSO)).SlotConfigs;
        }
    }
}
