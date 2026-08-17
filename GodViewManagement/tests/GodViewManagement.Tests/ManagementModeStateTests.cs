using Xunit;

namespace GodViewManagement.Tests
{
    public sealed class ManagementModeStateTests
    {
        [Fact]
        public void StartsDisabled()
        {
            var state = new ManagementModeState();

            Assert.False(state.IsEnabled);
        }

        [Fact]
        public void ToggleChangesModeAndSessionChangeForcesDisabled()
        {
            var state = new ManagementModeState();

            state.ObserveSession(new object());
            Assert.True(state.Toggle());

            state.ObserveSession(new object());

            Assert.False(state.IsEnabled);
        }

        [Fact]
        public void SameSessionDoesNotResetMode()
        {
            var state = new ManagementModeState();
            var session = new object();
            state.ObserveSession(session);
            state.Toggle();

            Assert.False(state.ObserveSession(session));
            Assert.True(state.IsEnabled);
        }
    }
}
