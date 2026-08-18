using TerrainEditor.Runtime;
using Xunit;

namespace TerrainEditor.Tests
{
    public sealed class FrameTickGateTests
    {
        [Fact]
        public void SameFrameCanOnlyEnterOnce()
        {
            var gate = new FrameTickGate();

            Assert.True(gate.TryEnter(42));
            Assert.False(gate.TryEnter(42));
            Assert.True(gate.TryEnter(43));
        }
    }
}
