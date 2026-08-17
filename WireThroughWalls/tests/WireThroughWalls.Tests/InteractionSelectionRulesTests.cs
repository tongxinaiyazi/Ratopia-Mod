using WireThroughWalls.Core;
using Xunit;

namespace WireThroughWalls.Tests
{
    public sealed class InteractionSelectionRulesTests
    {
        [Fact]
        public void SelectedForegroundReplacesLastEnteredWireForInteraction()
        {
            var foreground = new object();
            var wire = new object();

            Assert.Same(
                foreground,
                InteractionSelectionRules.PreferSelectedTarget(foreground, wire));
        }

        [Fact]
        public void MissingSelectedTargetKeepsLastEnteredTarget()
        {
            var wire = new object();

            Assert.Same(
                wire,
                InteractionSelectionRules.PreferSelectedTarget<object>(null, wire));
        }
    }
}
