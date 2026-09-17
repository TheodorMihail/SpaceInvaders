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
    public class ExpeditionShopServiceTests : ZenjectUnitTestFixture
    {
        private const int OfferCount = 6;

        private const int NormalPrice = 150;
        private const int RarePrice = 350;
        private const int LegendaryPrice = 700;

        private ExpeditionShopService _shopService;
        private IItemsRepository _mockItemsRepository;
        private ExpeditionShopDataConfigSO _mockShopConfig;
        private readonly List<ItemConfigSO> _itemConfigs = new();
        private readonly List<ItemRarityConfigSO> _rarityConfigs = new();

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _mockShopConfig = Substitute.For<ExpeditionShopDataConfigSO>();
            _mockShopConfig.OfferCount.Returns(OfferCount);
            _mockShopConfig.Rarities.Returns(CreateRarities());

            var mockExpeditionRepository = Substitute.For<IExpeditionRepository>();
            mockExpeditionRepository.GetShopDataConfig().Returns(_mockShopConfig);

            _mockItemsRepository = Substitute.For<IItemsRepository>();
            SetAuthoredItems(ItemRarityTypes.Normal, ItemRarityTypes.Rare, ItemRarityTypes.Legendary);

            Container.Bind<IExpeditionRepository>().FromInstance(mockExpeditionRepository);
            Container.Bind<IItemsRepository>().FromInstance(_mockItemsRepository);

            _shopService = Container.Instantiate<ExpeditionShopService>();
        }

        [TearDown]
        public override void Teardown()
        {
            Object.DestroyImmediate(_mockShopConfig);

            foreach (ItemConfigSO config in _itemConfigs)
            {
                Object.DestroyImmediate(config);
            }

            foreach (ItemRarityConfigSO config in _rarityConfigs)
            {
                Object.DestroyImmediate(config);
            }

            _itemConfigs.Clear();
            _rarityConfigs.Clear();
            base.Teardown();
        }

        [Test]
        public void RollOffers_StocksTheAuthoredCount()
        {
            List<ExpeditionShopOfferEntry> offers = _shopService.RollOffers();

            Assert.AreEqual(OfferCount, offers.Count);
        }

        [Test]
        public void RollOffers_PricesEveryOfferFromItsTier()
        {
            List<ExpeditionShopOfferEntry> offers = _shopService.RollOffers();

            foreach (ExpeditionShopOfferEntry offer in offers)
            {
                Assert.Contains(offer.Price, new[] { NormalPrice, RarePrice, LegendaryPrice });
            }
        }

        [Test]
        public void RollOffers_RollsAnItemForEveryOffer()
        {
            List<ExpeditionShopOfferEntry> offers = _shopService.RollOffers();

            foreach (ExpeditionShopOfferEntry offer in offers)
            {
                Assert.IsNotNull(offer.Item);
                Assert.IsFalse(string.IsNullOrEmpty(offer.Item.InstanceId));
                Assert.IsFalse(offer.IsSold);
            }
        }

        /// <summary>Each slot rolls its own instance, so two of the same template never collide.</summary>
        [Test]
        public void RollOffers_GivesEveryOfferItsOwnInstance()
        {
            List<ExpeditionShopOfferEntry> offers = _shopService.RollOffers();
            var instanceIds = new HashSet<string>();

            foreach (ExpeditionShopOfferEntry offer in offers)
            {
                Assert.IsTrue(instanceIds.Add(offer.Item.InstanceId));
            }
        }

        /// <summary>A shelf slot is never spent on a tier that cannot be filled.</summary>
        [Test]
        public void RollOffers_WithATierHoldingNothing_StocksTheOthersInstead()
        {
            SetAuthoredItems(ItemRarityTypes.Normal, ItemRarityTypes.Rare);

            List<ExpeditionShopOfferEntry> offers = _shopService.RollOffers();

            Assert.AreEqual(OfferCount, offers.Count);

            foreach (ExpeditionShopOfferEntry offer in offers)
            {
                Assert.AreNotEqual(LegendaryPrice, offer.Price);
            }
        }

        [Test]
        public void RollOffers_WithNothingAuthored_StocksNothing()
        {
            SetAuthoredItems();

            Assert.IsEmpty(_shopService.RollOffers());
        }

        [Test]
        public void RollOffers_WithoutAConfig_StocksNothing()
        {
            var mockExpeditionRepository = Substitute.For<IExpeditionRepository>();
            mockExpeditionRepository.GetShopDataConfig().Returns((ExpeditionShopDataConfigSO)null);

            Container.Rebind<IExpeditionRepository>().FromInstance(mockExpeditionRepository);

            Assert.IsEmpty(Container.Instantiate<ExpeditionShopService>().RollOffers());
        }

        /// <summary>Weighting decides the mix, so the heavier tier has to dominate a long run of rolls
        /// rather than merely appear.</summary>
        [Test]
        public void RollOffers_FavoursTheHeavierTier()
        {
            int normalCount = 0;
            int legendaryCount = 0;

            for (int i = 0; i < 100; i++)
            {
                foreach (ExpeditionShopOfferEntry offer in _shopService.RollOffers())
                {
                    normalCount += offer.Price == NormalPrice ? 1 : 0;
                    legendaryCount += offer.Price == LegendaryPrice ? 1 : 0;
                }
            }

            Assert.Greater(normalCount, legendaryCount);
        }

        /// <summary>Stock rolls exactly as a drop does, so the tier decides how many lines an item gets.</summary>
        [Test]
        public void RollOffers_RollsAsManyAffixesAsTheTierAllows()
        {
            const int affixCount = 3;
            SetAuthoredItems(ItemRarityTypes.Legendary);

            ItemRarityConfigSO legendary = CreateRarityConfig(ItemRarityTypes.Legendary, affixCount);
            _mockItemsRepository
                .TryGetItemRarityConfig(ItemRarityTypes.Legendary, out _)
                .Returns(call =>
                {
                    call[1] = legendary;
                    return true;
                });

            foreach (ExpeditionShopOfferEntry offer in _shopService.RollOffers())
            {
                Assert.AreEqual(affixCount, offer.Item.Affixes.Count);
            }
        }

        private static List<ExpeditionShopRarityDTO> CreateRarities()
        {
            return new List<ExpeditionShopRarityDTO>
            {
                new(ItemRarityTypes.Normal, NormalPrice, 5),
                new(ItemRarityTypes.Rare, RarePrice, 3),
                new(ItemRarityTypes.Legendary, LegendaryPrice, 1)
            };
        }

        /// <summary>Replaces the catalogue with one template per named tier, so a tier left out is one
        /// the shop cannot stock.</summary>
        private void SetAuthoredItems(params ItemRarityTypes[] rarities)
        {
            var configs = new List<ItemConfigSO>();

            foreach (ItemRarityTypes rarity in rarities)
            {
                configs.Add(CreateItemConfig(rarity));
            }

            _mockItemsRepository.GetAllItemConfigs().Returns(configs);
        }

        private ItemConfigSO CreateItemConfig(ItemRarityTypes rarity)
        {
            ItemConfigSO config = Substitute.For<ItemConfigSO>();
            config.Rarity.Returns(rarity);
            config.DropWeight.Returns(1);
            config.PossibleAffixes.Returns(new List<ItemAffixDTO>
            {
                new(ShipUpgradableStatTypes.Health, ShipStatValueTypes.Flat, 1f, 2f),
                new(ShipUpgradableStatTypes.Damage, ShipStatValueTypes.Flat, 1f, 2f),
                new(ShipUpgradableStatTypes.MoveSpeed, ShipStatValueTypes.Flat, 1f, 2f)
            });

            _itemConfigs.Add(config);
            return config;
        }

        private ItemRarityConfigSO CreateRarityConfig(ItemRarityTypes rarity, int affixCount)
        {
            ItemRarityConfigSO config = Substitute.For<ItemRarityConfigSO>();
            config.Rarity.Returns(rarity);
            config.AffixCount.Returns(affixCount);

            _rarityConfigs.Add(config);
            return config;
        }
    }
}
