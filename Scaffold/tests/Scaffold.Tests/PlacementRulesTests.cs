using ScaffoldMod.Core;
using Xunit;

namespace ScaffoldMod.Tests
{
    public sealed class PlacementRulesTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void AllowsUnsupportedAndAllBuildingOverlays(int kind)
        {
            Assert.True(ScaffoldPlacementRules.CanPlace((ScaffoldCellKind)kind, alreadyHasScaffold: false));
        }

        [Theory]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        public void RejectsSolidAndExistingLadderCells(int kind)
        {
            Assert.False(ScaffoldPlacementRules.CanPlace((ScaffoldCellKind)kind, alreadyHasScaffold: false));
        }

        [Fact]
        public void RejectsDuplicateScaffold()
        {
            Assert.False(ScaffoldPlacementRules.CanPlace(ScaffoldCellKind.Empty, alreadyHasScaffold: true));
        }
    }
}
