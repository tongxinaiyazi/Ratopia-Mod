using SuperBow.Core;
using Xunit;

namespace SuperBow.Tests
{
    public sealed class SplashAndTooltipRulesTests
    {
        [Theory]
        [InlineData(false, true, true, 1.5f, 0f, true)]
        [InlineData(false, true, true, 1.5001f, 0f, false)]
        [InlineData(true, true, true, 0f, 0f, false)]
        [InlineData(false, false, true, 0f, 0f, false)]
        [InlineData(false, true, false, 0f, 0f, false)]
        public void Splash_filter_uses_confirmed_radius_and_guards(
            bool isPrimary, bool isEnemy, bool isAlive, float x, float y, bool expected)
        {
            Assert.Equal(expected, SplashRules.ShouldDamage(
                isPrimary, isEnemy, isAlive, 0f, 0f, x, y));
        }

        [Fact]
        public void Splash_damage_is_half_of_direct_damage()
        {
            Assert.Equal(12.5f, SplashRules.CalculateDamage(25f));
        }

        [Fact]
        public void Tooltip_only_relables_the_exact_bleed_marker()
        {
            Assert.True(TooltipRules.IsBleedMarker(313, 3f));
            Assert.False(TooltipRules.IsBleedMarker(313, 2f));
            Assert.False(TooltipRules.IsBleedMarker(215, 3f));
            Assert.Equal("流血", TooltipRules.BleedText);
        }

        [Theory]
        [InlineData(1, 1, "WoodBow", true)]
        [InlineData(0, 1, "Gradius", false)]
        [InlineData(1, 2, "WoodBow", false)]
        [InlineData(1, 1, "NobleSword", false)]
        public void Queen_bow_identity_requires_index_type_and_internal_name(
            int index, int type, string name, bool expected)
        {
            Assert.Equal(expected, QueenBowIdentity.IsMatch(index, type, name));
        }
    }
}
