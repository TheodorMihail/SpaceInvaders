using NSubstitute;
using NUnit.Framework;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class RepositoryManagerTests
    {
        private IRepositoryManager _repositoryManager;
        private List<LevelConfigSO> _levelConfigs;
        private List<PlayerSpaceshipConfigSO> _playerConfigs;
        private List<EnemySpaceshipConfigSO> _enemyConfigs;
        private List<PowerupConfigSO> _powerupConfigs;

        private static LevelConfigSO CreateMockLevelConfig(int levelIndex)
        {
            var config = Substitute.For<LevelConfigSO>();

            config.Index.Returns(levelIndex);
            config.LevelName.Returns($"Level {levelIndex}");
            config.ObjectID.Returns($"Level {levelIndex}");

            return config;
        }

        [SetUp]
        public void Setup()
        {
            _levelConfigs = new List<LevelConfigSO>
            {
                CreateMockLevelConfig(1),
                CreateMockLevelConfig(2),
            };

            _playerConfigs = new List<PlayerSpaceshipConfigSO>
            {
                ScriptableObject.CreateInstance<PlayerSpaceshipConfigSO>()
            };

            _enemyConfigs = new List<EnemySpaceshipConfigSO>
            {
                ScriptableObject.CreateInstance<EnemySpaceshipConfigSO>(),
                ScriptableObject.CreateInstance<EnemySpaceshipConfigSO>()
            };

            _powerupConfigs = new List<PowerupConfigSO>();

            _repositoryManager = new RepositoryManager(
                CreateLevelsDataConfig(_levelConfigs),
                CreatePlayerDataConfig(_playerConfigs),
                CreateEnemyDataConfig(_enemyConfigs),
                CreatePowerupsDataConfig(_powerupConfigs, 0f),
                CreateProjectSettingsConfig());
        }

        private static LevelsDataConfigSO CreateLevelsDataConfig(List<LevelConfigSO> levelConfigs)
        {
            var config = Substitute.For<LevelsDataConfigSO>();
            config.LevelsConfigs.Returns(levelConfigs);
            return config;
        }

        private static PlayerDataConfigSO CreatePlayerDataConfig(List<PlayerSpaceshipConfigSO> playerConfigs)
        {
            var config = Substitute.For<PlayerDataConfigSO>();
            config.PlayerConfigs.Returns(playerConfigs);
            return config;
        }

        private static EnemyDataConfigSO CreateEnemyDataConfig(List<EnemySpaceshipConfigSO> enemyConfigs)
        {
            var config = Substitute.For<EnemyDataConfigSO>();
            config.EnemyConfigs.Returns(enemyConfigs);
            return config;
        }

        private static PowerupsDataConfigSO CreatePowerupsDataConfig(List<PowerupConfigSO> powerupConfigs, float globalPowerupDropChance)
        {
            var config = Substitute.For<PowerupsDataConfigSO>();
            config.PowerupConfigs.Returns(powerupConfigs);
            config.GlobalPowerupDropChance.Returns(globalPowerupDropChance);
            return config;
        }

        private static ProjectDataConfigSO CreateProjectSettingsConfig()
        {
            return Substitute.For<ProjectDataConfigSO>();
        }

        [TearDown]
        public void Teardown()
        {
            foreach (var config in _levelConfigs)
            {
                Object.DestroyImmediate(config);
            }

            foreach (var config in _playerConfigs)
            {
                Object.DestroyImmediate(config);
            }

            foreach (var config in _enemyConfigs)
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void GetLevelsCount_ReturnsCorrectCount()
        {
            var count = _repositoryManager.GetLevelsCount();

            Assert.AreEqual(2, count);
        }

        [Test]
        public void GetLevelConfig_WithValidTypeAndWrongID_ReturnsDefaultAndLogsError()
        {
            var config = _repositoryManager.GetLevelConfig(3);
            Assert.IsNull(config);
            LogAssert.Expect(LogType.Error, "[RepositoryManager] [Error] Object with ID 'Level 3' not found in bucket LevelConfigSO.");
        }

        [Test]
        public void GetLevelConfig_WithValidType_ReturnsConfig()
        {
            var config = _repositoryManager.GetLevelConfig(1);
            Assert.IsNotNull(config);
        }

        [Test]
        public void GetPlayerConfig_WithValidType_ReturnsConfig()
        {
            var config = _repositoryManager.GetPlayerConfig(PlayerTypes.Player1);
            Assert.IsNotNull(config);
        }

        [Test]
        public void GetEnemyConfig_WithValidType_ReturnsConfig()
        {
            var config = _repositoryManager.GetEnemyConfig(EnemyTypes.Enemy1);
            Assert.IsNotNull(config);
        }

        [Test]
        public void Constructor_WithEmptyLevels_SetsCountToZero()
        {
            var emptyLevels = new List<LevelConfigSO>();
            var emptyPlayers = new List<PlayerSpaceshipConfigSO>();
            var emptyEnemies = new List<EnemySpaceshipConfigSO>();
            var emptyPowerups = new List<PowerupConfigSO>();

            var manager = new RepositoryManager(
                CreateLevelsDataConfig(emptyLevels),
                CreatePlayerDataConfig(emptyPlayers),
                CreateEnemyDataConfig(emptyEnemies),
                CreatePowerupsDataConfig(emptyPowerups, 0f),
                CreateProjectSettingsConfig());

            Assert.AreEqual(0, manager.GetLevelsCount());
        }

        [Test]
        public void Constructor_AddsAllConfigsToRepository()
        {
            Assert.DoesNotThrow(() => {
                _repositoryManager.GetLevelConfig(1);
                _repositoryManager.GetPlayerConfig(PlayerTypes.Player1);
                _repositoryManager.GetEnemyConfig(EnemyTypes.Enemy1);
            });
        }
    }
}
