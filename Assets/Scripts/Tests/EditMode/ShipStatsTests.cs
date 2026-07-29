using NUnit.Framework;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class ShipStatsTests
    {
        private static ShipStats CreateStats()
        {
            return new ShipStats(new ShipBaseStats());
        }

        [Test]
        public void ApplyStatBonus_WithFlatBonus_AddsFlatAmountToBase()
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.Damage, 5f, ShipStatValueTypes.Flat);

            Assert.AreEqual(15, stats.CurrentProjectileDamage);
        }

        [Test]
        public void ApplyStatBonus_WithPercentageBonus_ScalesBase()
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.Damage, 0.5f, ShipStatValueTypes.Percentage);

            Assert.AreEqual(15, stats.CurrentProjectileDamage);
        }

        [Test]
        public void ApplyStatBonus_WithFlatAndPercentageBonuses_BothCountFromBaseIndependently()
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.Damage, 5f, ShipStatValueTypes.Flat);
            stats.ApplyStatBonus(ShipUpgradableStatTypes.Damage, 0.5f, ShipStatValueTypes.Percentage);

            // base(10) + flat(5) + base*percentage(10*0.5=5) = 20
            Assert.AreEqual(20, stats.CurrentProjectileDamage);
        }

        [Test]
        public void ApplyStatBonus_OrderOfFlatAndPercentage_DoesNotChangeResult()
        {
            ShipStats flatFirst = CreateStats();
            flatFirst.ApplyStatBonus(ShipUpgradableStatTypes.Damage, 5f, ShipStatValueTypes.Flat);
            flatFirst.ApplyStatBonus(ShipUpgradableStatTypes.Damage, 0.5f, ShipStatValueTypes.Percentage);

            ShipStats percentageFirst = CreateStats();
            percentageFirst.ApplyStatBonus(ShipUpgradableStatTypes.Damage, 0.5f, ShipStatValueTypes.Percentage);
            percentageFirst.ApplyStatBonus(ShipUpgradableStatTypes.Damage, 5f, ShipStatValueTypes.Flat);

            Assert.AreEqual(flatFirst.CurrentProjectileDamage, percentageFirst.CurrentProjectileDamage);
        }

        [Test]
        public void ApplyStatBonus_WithSevereNegativeFlatBonus_FloorsAtTenPercentOfBase()
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.Damage, -100f, ShipStatValueTypes.Flat);

            Assert.AreEqual(1, stats.CurrentProjectileDamage); // 10% of base 10
        }

        [Test]
        public void ApplyStatBonus_WithSevereNegativePercentageBonus_FloorsAtTenPercentOfBase()
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.Damage, -2f, ShipStatValueTypes.Percentage);

            Assert.AreEqual(1, stats.CurrentProjectileDamage); // 10% of base 10
        }

        [Test]
        public void ApplyStatBonus_WithSevereFlatAndPercentageMaluses_StillFloorsAtTenPercentOfBase()
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.Damage, -100f, ShipStatValueTypes.Flat);
            stats.ApplyStatBonus(ShipUpgradableStatTypes.Damage, -2f, ShipStatValueTypes.Percentage);

            Assert.AreEqual(1, stats.CurrentProjectileDamage); // 10% of base 10
        }

        [Test]
        public void ApplyStatBonus_FireRate_PositiveFlatBonusStillShootsFaster()
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.FireRate, 0.2f, ShipStatValueTypes.Flat);

            Assert.Less(stats.CurrentFireRate, stats.BaseFireRate);
        }

        [Test]
        public void ApplyStatBonus_FireRate_PositivePercentageBonusStillShootsFaster()
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.FireRate, 0.5f, ShipStatValueTypes.Percentage);

            Assert.Less(stats.CurrentFireRate, stats.BaseFireRate);
        }

        [Test]
        public void ApplyStatBonus_WithNoBonuses_LeavesCurrentValueAtBase()
        {
            ShipStats stats = CreateStats();

            Assert.AreEqual(stats.BaseProjectileDamage, stats.CurrentProjectileDamage);
        }
    }
}
