using Xunit;

namespace GodViewManagement.Tests
{
    public sealed class RuntimeTickGateTests
    {
        [Fact]
        public void FirstDriverSourceInAFrameIsAccepted()
        {
            var gate = new RuntimeTickGate();

            Assert.True(gate.TryEnter(100));
        }

        [Fact]
        public void SecondDriverSourceInTheSameFrameIsRejected()
        {
            var gate = new RuntimeTickGate();

            Assert.True(gate.TryEnter(100));
            Assert.False(gate.TryEnter(100));
        }

        [Fact]
        public void DriverIsAcceptedAgainOnTheNextFrame()
        {
            var gate = new RuntimeTickGate();

            Assert.True(gate.TryEnter(100));
            Assert.True(gate.TryEnter(101));
        }
    }
}
