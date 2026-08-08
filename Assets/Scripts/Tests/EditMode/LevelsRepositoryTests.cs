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
    public class LevelsRepositoryTests
    {
        private ILevelsRepository _levelsRepository;
        private List<LevelConfigSO> _levelConfigs;

        private static LevelConfigSO CreateMockLevelConfig(int levelIndex)
        {
            var config = Substitute.For<LevelConfigSO>();

            config.Index.Returns(levelIndex);
            config.LevelName.Returns($"Level {levelIndex}");
            config.ObjectID.Returns($"Level {levelIndex}");

            return config;
        }

        private static LevelsDataConfigSO CreateLevelsDataConfig(List<LevelConfigSO> levelConfigs)
        {
            var config = Substitute.For<LevelsDataConfigSO>();
            config.LevelsConfigs.Returns(levelConfigs);
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

            _levelsRepository = new LevelsRepository(CreateLevelsDataConfig(_levelConfigs));
        }

        [TearDown]
        public void Teardown()
        {
            foreach (var config in _levelConfigs)
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void GetLevelsCount_ReturnsCorrectCount()
        {
            var count = _levelsRepository.GetLevelsCount();

            Assert.AreEqual(2, count);
        }

        [Test]
        public void TryGetLevelConfig_WithWrongID_ReturnsFalseAndLogsError()
        {
            bool found = _levelsRepository.TryGetLevelConfig(3, out LevelConfigSO config);

            Assert.IsFalse(found);
            Assert.IsNull(config);
            LogAssert.Expect(LogType.Error, "[LevelsRepository] [Error] Object with ID 'Level 3' not found in bucket LevelConfigSO.");
        }

        [Test]
        public void TryGetLevelConfig_WithValidID_ReturnsTrueAndConfig()
        {
            bool found = _levelsRepository.TryGetLevelConfig(1, out LevelConfigSO config);

            Assert.IsTrue(found);
            Assert.IsNotNull(config);
        }

        [Test]
        public void Constructor_WithEmptyLevels_SetsCountToZero()
        {
            var repository = new LevelsRepository(CreateLevelsDataConfig(new List<LevelConfigSO>()));

            Assert.AreEqual(0, repository.GetLevelsCount());
        }
    }
}
