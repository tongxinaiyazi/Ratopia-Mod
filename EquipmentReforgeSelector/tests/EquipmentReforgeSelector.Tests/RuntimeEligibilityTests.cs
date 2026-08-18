using EquipmentReforgeSelector;
using Xunit;

namespace EquipmentReforgeSelector.Tests
{
    public sealed class RuntimeEligibilityTests
    {
        [Theory]
        [InlineData(false, true, 3, 1, true)]
        [InlineData(false, true, 3, 2, true)]
        [InlineData(true, true, 3, 1, false)]
        [InlineData(false, false, 3, 1, false)]
        [InlineData(false, true, 2, 1, false)]
        [InlineData(false, true, 3, 0, false)]
        [InlineData(false, true, 3, 3, false)]
        public void Selector_visibility_matches_the_supported_reforge_context(
            bool isRobot,
            bool isUpgrade,
            int buildType,
            int level,
            bool expected)
        {
            Assert.Equal(expected, RuntimeEligibility.ShouldShow(isRobot, isUpgrade, buildType, level));
        }
    }
}
