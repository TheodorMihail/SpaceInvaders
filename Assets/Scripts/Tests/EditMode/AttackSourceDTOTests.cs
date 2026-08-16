using NUnit.Framework;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class AttackSourceDTOTests
    {
        private const float FloatTolerance = 0.0001f;

        /// <summary>Crit is switched off so a roll is deterministic and multipliers can be compared.</summary>
        private static ShipStats CreateStatsWithoutCrit()
        {
            var stats = new ShipStats(new ShipBaseStats());
            stats.ApplyStatBonus(ShipUpgradableStatTypes.CritChance, -1f, ShipStatValueTypes.Flat);

            return stats;
        }

        [Test]
        public void FromStats_LeavesTheRollUnscaled()
        {
            ShipStats stats = CreateStatsWithoutCrit();
            AttackSourceDTO source = AttackSourceDTO.FromStats(stats);

            Assert.AreEqual(stats.CurrentProjectileDamage, source.RollDamage(out _));
        }

        [Test]
        public void RollDamage_AppliesTheDamageMultiplier()
        {
            ShipStats stats = CreateStatsWithoutCrit();
            var source = new AttackSourceDTO(stats, 3f, 1f);

            Assert.AreEqual(stats.CurrentProjectileDamage * 3, source.RollDamage(out _));
        }

        /// <summary>The stats are held live, so a buff landing after the shot was fired still counts.
        /// This is what stops the DTO becoming a snapshot.</summary>
        [Test]
        public void RollDamage_ReadsTheStatsAsTheyAreNow()
        {
            ShipStats stats = CreateStatsWithoutCrit();
            var source = new AttackSourceDTO(stats, 1f, 1f);

            int beforeBuff = source.RollDamage(out _);
            stats.ApplyStatBonus(ShipUpgradableStatTypes.Damage, 50f, ShipStatValueTypes.Flat);

            Assert.Greater(source.RollDamage(out _), beforeBuff);
        }

        [Test]
        public void ProjectileSpeed_AppliesItsOwnMultiplier()
        {
            ShipStats stats = CreateStatsWithoutCrit();
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
