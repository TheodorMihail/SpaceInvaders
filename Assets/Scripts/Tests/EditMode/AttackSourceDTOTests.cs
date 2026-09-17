using NUnit.Framework;
using SpaceInvaders.Scenes.Game;
using UnityEngine;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class AttackSourceDTOTests
    {
        private const float FloatTolerance = 0.0001f;

        /// <summary>Crit cannot be authored away: every stat floors at a fraction of its base, so the
        /// chance lands on that floor rather than on zero.</summary>
        private static ShipStats CreateStatsWithMinimumCrit()
        {
            var stats = new ShipStats(new ShipBaseStats());
            stats.ApplyStatBonus(ShipUpgradableStatTypes.CritChance, -1f, ShipStatValueTypes.Flat);

            return stats;
        }

        /// <summary>The roll is compared against however it landed, since a floored chance still fires
        /// now and then and an assertion that assumed otherwise would only fail occasionally.</summary>
        private static int ExpectedDamage(ShipStats stats, float damageMultiplier, bool isCritical)
        {
            float damage = stats.CurrentProjectileDamage * damageMultiplier;

            return Mathf.RoundToInt(isCritical ? damage * stats.CurrentCritDamage : damage);
        }

        [Test]
        public void FromStats_LeavesTheRollUnscaled()
        {
            ShipStats stats = CreateStatsWithMinimumCrit();
            AttackSourceDTO source = AttackSourceDTO.FromStats(stats);

            int damage = source.RollDamage(out bool isCritical);

            Assert.AreEqual(ExpectedDamage(stats, 1f, isCritical), damage);
        }

        [Test]
        public void RollDamage_AppliesTheDamageMultiplier()
        {
            ShipStats stats = CreateStatsWithMinimumCrit();
            var source = new AttackSourceDTO(stats, 3f, 1f);

            int damage = source.RollDamage(out bool isCritical);

            Assert.AreEqual(ExpectedDamage(stats, 3f, isCritical), damage);
        }

        /// <summary>The stats are held live, so a buff landing after the shot was fired still counts.
        /// This is what stops the DTO becoming a snapshot.</summary>
        [Test]
        public void RollDamage_ReadsTheStatsAsTheyAreNow()
        {
            ShipStats stats = CreateStatsWithMinimumCrit();
            var source = new AttackSourceDTO(stats, 1f, 1f);

            int beforeBuff = source.RollDamage(out _);
            stats.ApplyStatBonus(ShipUpgradableStatTypes.Damage, 50f, ShipStatValueTypes.Flat);

            Assert.Greater(source.RollDamage(out _), beforeBuff);
        }

        [Test]
        public void ProjectileSpeed_AppliesItsOwnMultiplier()
        {
            ShipStats stats = CreateStatsWithMinimumCrit();
            var source = new AttackSourceDTO(stats, 1f, 0.5f);

            Assert.AreEqual(stats.CurrentProjectileSpeed * 0.5f, source.ProjectileSpeed, FloatTolerance);
        }

        /// <summary>A default struct reaches damage code whenever a projectile outlives its shooter.</summary>
        [Test]
        public void RollDamage_WithNoAttacker_DealsNothing()
        {
            var source = default(AttackSourceDTO);

            Assert.AreEqual(0, source.RollDamage(out bool isCritical));
            Assert.IsFalse(isCritical);
        }
    }
}
