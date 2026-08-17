using System;
using SuperBow.Core;
using Xunit;

namespace SuperBow.Tests
{
    public sealed class CombatTargetRulesTests
    {
        [Theory]
        [InlineData(100f, 90f, true)]
        [InlineData(100f, 100f, false)]
        [InlineData(100f, 101f, false)]
        [InlineData(0f, 0f, false)]
        public void Vanilla_hit_is_confirmed_only_by_hp_decrease(
            float before, float after, bool expected)
        {
            Assert.Equal(expected, HitConfirmation.DidTakeDamage(before, after));
        }

        [Fact]
        public void Target_kinds_cover_every_vanilla_arrow_damage_branch()
        {
            Assert.Equal(
                new[] { "GameUnit", "AnimalBody", "MapObject", "Building" },
                Enum.GetNames(typeof(CombatTargetKind)));
        }
    }
}
