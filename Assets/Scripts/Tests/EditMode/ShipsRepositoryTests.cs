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
    public class ShipsRepositoryTests
    {
        private IShipsRepository _shipsRepository;
        private List<PlayerSpaceshipConfigSO> _playerConfigs;
        private List<EnemySpaceshipConfigSO> _enemyConfigs;

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

        [SetUp]
        public void Setup()
        {
            _playerConfigs = new List<PlayerSpaceshipConfigSO>
            {
                ScriptableObject.CreateInstance<PlayerSpaceshipConfigSO>()
            };

            _enemyConfigs = new List<EnemySpaceshipConfigSO>
            {
                ScriptableObject.CreateInstance<EnemySpaceshipConfigSO>(),
                ScriptableObject.CreateInstance<EnemySpaceshipConfigSO>()
            };

            _shipsRepository = new ShipsRepository(CreatePlayerDataConfig(_playerConfigs), CreateEnemyDataConfig(_enemyConfigs));
        }

        [TearDown]
        public void Teardown()
        {
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
        public void TryGetPlayerConfig_WithValidType_ReturnsTrueAndConfig()
        {
            bool found = _shipsRepository.TryGetPlayerConfig(PlayerTypes.Player1, out PlayerSpaceshipConfigSO config);

            Assert.IsTrue(found);
            Assert.IsNotNull(config);
        }

        [Test]
        public void TryGetEnemyConfig_WithValidType_ReturnsTrueAndConfig()
        {
            bool found = _shipsRepository.TryGetEnemyConfig(EnemyTypes.Enemy1, out EnemySpaceshipConfigSO config);

            Assert.IsTrue(found);
            Assert.IsNotNull(config);
        }

        [Test]
        public void TryGetEnemyConfig_WithInvalidType_ReturnsFalseAndLogsError()
        {
            var emptyEnemyRepository = new ShipsRepository(CreatePlayerDataConfig(_playerConfigs), CreateEnemyDataConfig(new List<EnemySpaceshipConfigSO>()));

            bool found = emptyEnemyRepository.TryGetEnemyConfig(EnemyTypes.Enemy1, out EnemySpaceshipConfigSO config);

            Assert.IsFalse(found);
            Assert.IsNull(config);
            LogAssert.Expect(LogType.Error, "[ShipsRepository] [Error] Object with ID 'Enemy1' not found in bucket EnemySpaceshipConfigSO.");
        }
    }
}
