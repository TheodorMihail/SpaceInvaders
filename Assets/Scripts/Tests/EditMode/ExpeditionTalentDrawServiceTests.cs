using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Expedition;
using SpaceInvaders.Scenes.Game;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class ExpeditionTalentDrawServiceTests : ZenjectUnitTestFixture
    {
        private const int ChoiceCount = 3;

        /// <summary>Enough draws that a rule broken only on some rolls still shows up.</summary>
        private const int DrawSampleCount = 200;

        private IExpeditionTalentDrawService _drawService;
        private ITalentManager _mockTalentManager;
        private ExpeditionRewardsDataConfigSO _mockRewardsConfig;

        private readonly List<TalentConfigSO> _pool = new();
        private readonly List<TalentRarityConfigSO> _rarityConfigs = new();

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _pool.Clear();
            _rarityConfigs.Clear();

            _mockRewardsConfig = Substitute.For<ExpeditionRewardsDataConfigSO>();
            _mockRewardsConfig.TalentChoiceCount.Returns(ChoiceCount);
            _mockRewardsConfig.BossMinTalentRarity.Returns(TalentRarityTypes.Rare);

            var mockExpeditionRepository = Substitute.For<IExpeditionRepository>();
            mockExpeditionRepository.GetRewardsDataConfig().Returns(_mockRewardsConfig);

            var mockTalentsRepository = Substitute.For<ITalentsRepository>();
            mockTalentsRepository.GetTalentPool(GameModeTypes.Expedition).Returns(_ => _pool);
            mockTalentsRepository.GetAllTalentRarityConfigs().Returns(_ => _rarityConfigs);

            _mockTalentManager = Substitute.For<ITalentManager>();

            Container.Bind<IExpeditionRepository>().FromInstance(mockExpeditionRepository);
            Container.Bind<ITalentsRepository>().FromInstance(mockTalentsRepository);
            Container.Bind<ITalentManager>().FromInstance(_mockTalentManager);

            _drawService = Container.Instantiate<ExpeditionTalentDrawService>();
        }

        [TearDown]
        public override void Teardown()
        {
            Object.DestroyImmediate(_mockRewardsConfig);

            foreach (TalentConfigSO talent in _pool)
            {
                Object.DestroyImmediate(talent);
            }

            foreach (TalentRarityConfigSO rarity in _rarityConfigs)
            {
                Object.DestroyImmediate(rarity);
            }

            base.Teardown();
        }

        [Test]
        public void DrawTalents_OffersTheAuthoredNumberOfCards()
        {
            AddRarity(TalentRarityTypes.Common, 1);
            AddTalents(TalentRarityTypes.Common, 5);

            Assert.AreEqual(ChoiceCount, _drawService.DrawTalents(isBossDraw: false).Count);
        }

        /// <summary>A draw is a choice, so the same talent can never fill two of its cards.</summary>
        [Test]
        public void DrawTalents_NeverRepeatsWithinOneOffer()
        {
            AddRarity(TalentRarityTypes.Common, 1);
            AddTalents(TalentRarityTypes.Common, 5);

            for (int i = 0; i < DrawSampleCount; i++)
            {
                IReadOnlyList<TalentConfigSO> drawn = _drawService.DrawTalents(isBossDraw: false);
                var seen = new HashSet<string>();

                foreach (TalentConfigSO talent in drawn)
                {
                    Assert.IsTrue(seen.Add(talent.ObjectID), $"{talent.ObjectID} was offered twice.");
                }
            }
        }

        /// <summary>A pool smaller than the card count offers what it has rather than padding.</summary>
        [Test]
        public void DrawTalents_WithFewerTalentsThanCards_OffersOnlyWhatExists()
        {
            AddRarity(TalentRarityTypes.Common, 1);
            AddTalents(TalentRarityTypes.Common, 2);

            Assert.AreEqual(2, _drawService.DrawTalents(isBossDraw: false).Count);
        }

        /// <summary>The whole point of a max level: a talent with nothing left to give is not offered.</summary>
        [Test]
        public void DrawTalents_NeverOffersAMaxedTalent()
        {
            AddRarity(TalentRarityTypes.Common, 1);
            AddTalents(TalentRarityTypes.Common, 4);
            _mockTalentManager.IsMaxLevel(_pool[0].ObjectID).Returns(true);

            for (int i = 0; i < DrawSampleCount; i++)
            {
                foreach (TalentConfigSO talent in _drawService.DrawTalents(isBossDraw: false))
                {
                    Assert.AreNotEqual(_pool[0].ObjectID, talent.ObjectID);
                }
            }
        }

        [Test]
        public void DrawTalents_WithEveryTalentMaxed_OffersNothing()
        {
            AddRarity(TalentRarityTypes.Common, 1);
            AddTalents(TalentRarityTypes.Common, 3);
            _mockTalentManager.IsMaxLevel(Arg.Any<string>()).Returns(true);

            Assert.IsEmpty(_drawService.DrawTalents(isBossDraw: false));
        }

        [Test]
        public void DrawTalents_OnABossDraw_OffersNothingBelowTheMinimumRarity()
        {
            AddRarity(TalentRarityTypes.Common, 1);
            AddRarity(TalentRarityTypes.Rare, 1);
            AddTalents(TalentRarityTypes.Common, 4);
            AddTalents(TalentRarityTypes.Rare, 4);

            for (int i = 0; i < DrawSampleCount; i++)
            {
                foreach (TalentConfigSO talent in _drawService.DrawTalents(isBossDraw: true))
                {
                    Assert.GreaterOrEqual(talent.Rarity, TalentRarityTypes.Rare);
                }
            }
        }

        /// <summary>Offering nothing would read worse than offering a common, so the floor gives way
        /// when nothing is authored above it.</summary>
        [Test]
        public void DrawTalents_OnABossDrawWithNothingAboveTheFloor_FallsBackToWhatExists()
        {
            AddRarity(TalentRarityTypes.Common, 1);
            AddTalents(TalentRarityTypes.Common, 4);

            Assert.AreEqual(ChoiceCount, _drawService.DrawTalents(isBossDraw: true).Count);
        }

        /// <summary>Weights decide how often a tier comes up, so a tier weighted far higher has to
        /// dominate over many draws.</summary>
        [Test]
        public void DrawTalents_RespectsRarityWeightsOverManyDraws()
        {
            AddRarity(TalentRarityTypes.Common, 90);
            AddRarity(TalentRarityTypes.Epic, 10);
            AddTalents(TalentRarityTypes.Common, 10);
            AddTalents(TalentRarityTypes.Epic, 10);

            int commons = 0;
            int epics = 0;

            for (int i = 0; i < DrawSampleCount; i++)
            {
                foreach (TalentConfigSO talent in _drawService.DrawTalents(isBossDraw: false))
                {
                    if (talent.Rarity == TalentRarityTypes.Common)
                    {
                        commons++;
                    }
                    else
                    {
                        epics++;
                    }
                }
            }

            Assert.Greater(commons, epics, "A tier weighted nine times higher should come up more often.");
        }

        /// <summary>Unauthored tiers leave nothing to weigh, so the offer still fills out.</summary>
        [Test]
        public void DrawTalents_WithNoRarityConfigs_StillOffersCards()
        {
            AddTalents(TalentRarityTypes.Common, 5);

            Assert.AreEqual(ChoiceCount, _drawService.DrawTalents(isBossDraw: false).Count);
        }

        [Test]
        public void DrawTalents_WithAnEmptyPool_OffersNothing()
        {
            AddRarity(TalentRarityTypes.Common, 1);

            Assert.IsEmpty(_drawService.DrawTalents(isBossDraw: false));
        }

        private void AddRarity(TalentRarityTypes rarity, int drawWeight)
        {
            var config = Substitute.For<TalentRarityConfigSO>();

            config.Rarity.Returns(rarity);
            config.DrawWeight.Returns(drawWeight);

            _rarityConfigs.Add(config);
        }

        private void AddTalents(TalentRarityTypes rarity, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var config = Substitute.For<TalentConfigSO>();

                config.ObjectID.Returns($"{rarity}{i}");
                config.Rarity.Returns(rarity);

                _pool.Add(config);
            }
        }
    }
}
