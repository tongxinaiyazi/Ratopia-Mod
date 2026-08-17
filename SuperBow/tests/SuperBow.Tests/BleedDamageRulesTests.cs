using SuperBow.Core;
using Xunit;

namespace SuperBow.Tests
{
    public sealed class BleedDamageRulesTests
    {
        [Theory]
        [InlineData(50f, 0.03f, 1.5f)]
        [InlineData(2400f, 0.01f, 24f)]
        [InlineData(0f, 0.03f, 0f)]
        public void Exact_damage_preserves_the_max_health_percentage(
            float maxHealth,
            float fraction,
            float expected)
        {
            Assert.Equal(expected, BleedDamageRules.CalculateExact(
                maxHealth,
                fraction), 3);
        }

        [Theory]
        [InlineData(1.49f, 1)]
        [InlineData(1.5f, 2)]
        [InlineData(24f, 24)]
        [InlineData(0.03f, 1)]
        [InlineData(0f, 0)]
        public void Floating_percentage_is_rounded_for_discrete_damage(
            float exactDamage,
            int expected)
        {
            Assert.Equal(expected, DamageDisplayRules.RoundForDisplay(exactDamage));
        }

        [Theory]
        [InlineData(50f, 0.03f, 2)]
        [InlineData(160f, 0.03f, 5)]
        [InlineData(2400f, 0.01f, 24)]
        [InlineData(0f, 0.03f, 0)]
        public void Applied_damage_is_the_same_integer_used_by_the_damage_text(
            float maxHealth,
            float fraction,
            int expected)
        {
            Assert.Equal(expected, BleedDamageRules.CalculateApplied(
                maxHealth,
                fraction));
        }
    }
}
