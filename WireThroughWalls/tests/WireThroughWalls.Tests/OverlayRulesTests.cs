using WireThroughWalls.Core;
using Xunit;

namespace WireThroughWalls.Tests
{
    public sealed class OverlayRulesTests
    {
        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public void BuildTypeIsHiddenOnlyForAWireOverARealForegroundOwner(
            bool candidateIsWire,
            bool hasForegroundOwner,
            bool expected)
        {
            Assert.Equal(expected, OverlayRules.ShouldHideBuildType(candidateIsWire, hasForegroundOwner));
        }

        [Theory]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, true, false)]
        [InlineData(false, false, false)]
        public void OnlyWireAndForegroundBlueprintsCanShare(
            bool candidateIsWire,
            bool existingIsWire,
            bool expected)
        {
            Assert.Equal(expected, OverlayRules.CanBlueprintsShare(candidateIsWire, existingIsWire));
        }

        [Theory]
        [InlineData(true, false, true)]
        [InlineData(true, true, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public void CoordinationRunsOnlyForAWireCandidateOrExistingWire(
            bool candidateIsWire,
            bool hasExistingWire,
            bool expected)
        {
            Assert.Equal(expected, OverlayRules.RequiresCoordination(candidateIsWire, hasExistingWire));
        }

        [Theory]
        [InlineData(false, 2, true)]
        [InlineData(true, 58, false)]
        [InlineData(false, 55, false)]
        [InlineData(false, 0, false)]
        public void CompletedWireListIsMaskedOnlyForRoadCompletion(
            bool candidateIsWire,
            int candidateAbility,
            bool expected)
        {
            Assert.Equal(
                expected,
                OverlayRules.ShouldMaskCompletedWiresDuringCompletion(candidateIsWire, candidateAbility));
        }
    }
}
