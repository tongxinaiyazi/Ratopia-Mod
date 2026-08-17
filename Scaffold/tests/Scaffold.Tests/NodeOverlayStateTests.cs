using ScaffoldMod.Core;
using Xunit;

namespace Scaffold.Tests
{
    public sealed class NodeOverlayStateTests
    {
        [Fact]
        public void ApplyingOverlayPreservesTheUnderlyingNode()
        {
            var state = new ScaffoldNodeState(2);

            Assert.Equal(2, state.UnderlyingNodeType);
            Assert.Equal(3, state.OverlayNodeType);
        }

        [Fact]
        public void FinishingANodeRebuildCapturesTheNewUnderlyingState()
        {
            var state = new ScaffoldNodeState(0);

            state.CaptureRebuiltUnderlyingNode(2);

            Assert.Equal(2, state.UnderlyingNodeType);
            Assert.Equal(3, state.OverlayNodeType);
        }

        [Fact]
        public void RemovingOverlayRestoresTheMostRecentUnderlyingState()
        {
            var state = new ScaffoldNodeState(1);
            state.CaptureRebuiltUnderlyingNode(4);

            Assert.Equal(4, state.RestoreNodeType());
        }

        [Fact]
        public void LadderOverlayNeverOverwritesStoredUnderlyingState()
        {
            var state = new ScaffoldNodeState(2);

            state.CaptureRuntimeNode(3);

            Assert.Equal(2, state.UnderlyingNodeType);
        }

        [Fact]
        public void NonOverlayRuntimeNodeBecomesTheLatestUnderlyingState()
        {
            var state = new ScaffoldNodeState(0);

            state.CaptureRuntimeNode(4);

            Assert.Equal(4, state.UnderlyingNodeType);
        }
    }
}
