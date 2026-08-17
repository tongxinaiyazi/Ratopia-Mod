using WireThroughWalls.Core;
using Xunit;

namespace WireThroughWalls.Tests
{
    public sealed class DemolitionSelectionRulesTests
    {
        [Theory]
        [InlineData(true, true, false, 1)]
        [InlineData(true, true, true, 2)]
        [InlineData(true, false, true, 1)]
        [InlineData(false, true, false, 2)]
        [InlineData(false, false, true, 0)]
        public void SelectionMatchesForegroundDefaultAndAltWireOverride(
            bool hasForeground,
            bool hasWire,
            bool altPressed,
            int expected)
        {
            Assert.Equal(
                (DemolitionTargetPreference)expected,
                DemolitionSelectionRules.GetPreference(hasForeground, hasWire, altPressed));
        }
    }
}
