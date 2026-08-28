using NUnit.Framework;
using SpaceInvaders.Scenes.Game;
using UnityEngine;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class ShipStatsTests
    {
        private const float FloatTolerance = 0.0001f;
        private const int RollSampleCount = 20;
        private const int MagazineSize = 10;

        private static ShipStats CreateStats()
        {
            return new ShipStats(new ShipBaseStats());
        }

        /// <summary>The default base stats author no magazine, so one is granted as a bonus.</summary>
        private static ShipStats CreateStatsWithMagazine()
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.MagazineSize, MagazineSize, ShipStatValueTypes.Flat);
            stats.RefillAmmo();

            return stats;
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

        [Test]
        public void CritStats_WithoutBonuses_UseTheirDefaults()
        {
            ShipStats stats = CreateStats();

            Assert.AreEqual(0.1f, stats.CurrentCritChance, FloatTolerance);
            Assert.AreEqual(2f, stats.CurrentCritDamage, FloatTolerance);
        }

        [Test]
        public void ApplyStatBonus_CritChance_WithFlatBonus_AddsFlatAmountToBase()
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.CritChance, 0.15f, ShipStatValueTypes.Flat);

            Assert.AreEqual(0.25f, stats.CurrentCritChance, FloatTolerance);
        }

        [Test]
        public void ApplyStatBonus_CritDamage_WithPercentageBonus_ScalesBase()
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.CritDamage, 0.5f, ShipStatValueTypes.Percentage);

            Assert.AreEqual(3f, stats.CurrentCritDamage, FloatTolerance);
        }

        [Test]
        public void CurrentCritChance_WithHugeBonus_ClampsToOne()
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.CritChance, 5f, ShipStatValueTypes.Flat);

            Assert.AreEqual(1f, stats.CurrentCritChance, FloatTolerance);
        }

        [Test]
        public void CurrentCritDamage_WithSevereMalus_NeverDropsBelowOne()
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.CritDamage, -100f, ShipStatValueTypes.Flat);

            Assert.AreEqual(1f, stats.CurrentCritDamage, FloatTolerance);
        }

        /// <summary>The default base magazine is 0 (unlimited), so ammo tests grant one the way
        /// equipment does: a flat bonus followed by a refill.</summary>
        private static ShipStats CreateStatsWithMagazine(int magazineSize)
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.MagazineSize, magazineSize, ShipStatValueTypes.Flat);
            stats.RefillAmmo();

            return stats;
        }

        /// <summary>Unlimited ammo is an authored flag, not an empty magazine.</summary>
        [Test]
        public void Ammo_WithUnlimitedAmmo_IsNeverOutOfAmmo()
        {
            ShipStats stats = CreateStats();
            stats.SetUnlimitedAmmo(true);

            Assert.IsTrue(stats.HasUnlimitedAmmo);
            Assert.IsFalse(stats.IsOutOfAmmo);
        }

        [Test]
        public void Ammo_WithoutMagazineAndWithoutTheFlag_RunsOut()
        {
            ShipStats stats = CreateStats();

            Assert.IsFalse(stats.HasUnlimitedAmmo);
            Assert.IsTrue(stats.IsOutOfAmmo);
        }

        [Test]
        public void TryConsumeAmmo_WithUnlimitedAmmo_AlwaysSucceeds()
        {
            ShipStats stats = CreateStats();
            stats.SetUnlimitedAmmo(true);

            for (int i = 0; i < RollSampleCount; i++)
            {
                Assert.IsTrue(stats.TryConsumeAmmo());
            }

            Assert.IsFalse(stats.IsOutOfAmmo);
        }

        [Test]
        public void RefillAmmo_AfterMagazineBonus_FillsToTheRaisedMaximum()
        {
            ShipStats stats = CreateStatsWithMagazine(20);

            Assert.AreEqual(20, stats.CurrentMaxAmmo);
            Assert.AreEqual(20, stats.CurrentAmmo);
            Assert.IsFalse(stats.HasUnlimitedAmmo);
        }

        [Test]
        public void TryConsumeAmmo_SpendsOneRoundPerCall_UntilTheMagazineIsEmpty()
        {
            ShipStats stats = CreateStatsWithMagazine(3);

            Assert.IsTrue(stats.TryConsumeAmmo());
            Assert.AreEqual(2, stats.CurrentAmmo);

            Assert.IsTrue(stats.TryConsumeAmmo());
            Assert.IsTrue(stats.TryConsumeAmmo());

            Assert.IsTrue(stats.IsOutOfAmmo);
            Assert.IsFalse(stats.TryConsumeAmmo());
        }

        [Test]
        public void TryConsumeAmmo_RaisesAmmoChangedWithTheRemainingRounds()
        {
            ShipStats stats = CreateStatsWithMagazine(5);

            int reportedAmmo = -1;
            int reportedMax = -1;
            stats.AmmoChanged += (current, max) =>
            {
                reportedAmmo = current;
                reportedMax = max;
            };

            stats.TryConsumeAmmo();

            Assert.AreEqual(4, reportedAmmo);
            Assert.AreEqual(5, reportedMax);
        }

        [Test]
        public void ReloadDuration_WithoutBonuses_UsesItsDefault()
        {
            ShipStats stats = CreateStats();

            Assert.AreEqual(1.5f, stats.CurrentReloadDuration, FloatTolerance);
        }

        [Test]
        public void ApplyStatBonus_ReloadSpeed_PositiveFlatBonusStillReloadsFaster()
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.ReloadSpeed, 0.5f, ShipStatValueTypes.Flat);

            Assert.Less(stats.CurrentReloadDuration, stats.BaseReloadSpeed);
        }

        [Test]
        public void ApplyStatBonus_ReloadSpeed_PositivePercentageBonusStillReloadsFaster()
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.ReloadSpeed, 0.5f, ShipStatValueTypes.Percentage);

            Assert.Less(stats.CurrentReloadDuration, stats.BaseReloadSpeed);
        }

        [Test]
        public void ApplyStatBonus_PowerupDuration_AddsFlatSeconds()
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.PowerupDuration, 2f, ShipStatValueTypes.Flat);

            Assert.AreEqual(2f, stats.CurrentPowerupDuration, FloatTolerance);
        }

        [Test]
        public void SetUnlimitedAmmo_WhileActive_SpendsNoRounds()
        {
            ShipStats stats = CreateStatsWithMagazine();
            stats.SetUnlimitedAmmo(true);

            Assert.IsTrue(stats.TryConsumeAmmo());
            Assert.AreEqual(MagazineSize, stats.CurrentAmmo);
        }

        [Test]
        public void SetUnlimitedAmmo_WhenSwitchedOff_SpendsRoundsAgain()
        {
            ShipStats stats = CreateStatsWithMagazine();
            stats.SetUnlimitedAmmo(true);
            stats.SetUnlimitedAmmo(false);

            Assert.IsTrue(stats.TryConsumeAmmo());
            Assert.AreEqual(MagazineSize - 1, stats.CurrentAmmo);
        }

        [Test]
        public void SetUnlimitedAmmo_WithTheSameValueTwice_RaisesTheChangeOnlyOnce()
        {
            ShipStats stats = CreateStatsWithMagazine();
            int raisedCount = 0;
            stats.UnlimitedAmmoChanged += _ => raisedCount++;

            stats.SetUnlimitedAmmo(true);
            stats.SetUnlimitedAmmo(true);
            stats.SetUnlimitedAmmo(false);

            Assert.AreEqual(2, raisedCount);
        }

        [Test]
        public void RollOutgoingDamage_WithDamageMultiplier_ScalesTheRoll()
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.CritChance, -1f, ShipStatValueTypes.Flat);

            int unscaled = stats.RollOutgoingDamage(out _);
            int doubled = stats.RollOutgoingDamage(2f, out _);

            Assert.AreEqual(unscaled * 2, doubled);
        }

        /// <summary>Guards the bit-for-bit equivalence the unscaled overload forwards to.</summary>
        [Test]
        public void RollOutgoingDamage_WithMultiplierOfOne_MatchesTheUnscaledRoll()
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.CritChance, 1f, ShipStatValueTypes.Flat);

            for (int i = 0; i < RollSampleCount; i++)
            {
                Assert.AreEqual(stats.RollOutgoingDamage(out _), stats.RollOutgoingDamage(1f, out _));
            }
        }

        [Test]
        public void RollOutgoingDamage_WithGuaranteedCrit_MultipliesDamage()
        {
            ShipStats stats = CreateStats();
            stats.ApplyStatBonus(ShipUpgradableStatTypes.CritChance, 1f, ShipStatValueTypes.Flat);

            int expectedDamage = Mathf.RoundToInt(stats.CurrentProjectileDamage * stats.CurrentCritDamage);

            for (int i = 0; i < RollSampleCount; i++)
            {
                int damage = stats.RollOutgoingDamage(out bool isCritical);

                Assert.IsTrue(isCritical);
                Assert.AreEqual(expectedDamage, damage);
            }
        }
    }
}
