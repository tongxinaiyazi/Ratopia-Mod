using System;
using System.Reflection;
using Xunit;

namespace GodViewManagement.Tests
{
    public sealed class HudVisibilityStateTests
    {
        [Fact]
        public void NewSessionStartsVisible()
        {
            var state = CreateState();

            Assert.False(ReadHidden(state));
        }

        [Fact]
        public void HideShowAndResetAreRecoverable()
        {
            var state = CreateState();

            Invoke(state, "Hide");
            Assert.True(ReadHidden(state));
            Invoke(state, "Show");
            Assert.False(ReadHidden(state));
            Invoke(state, "Hide");
            Invoke(state, "Reset");
            Assert.False(ReadHidden(state));
        }

        [Theory]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(false, false, false)]
        public void RecoveryChordRequiresShiftAndTogglePress(bool shiftPressed, bool togglePressed, bool expectedHandled)
        {
            var state = CreateState();

            Assert.Equal(expectedHandled, InvokeToggle(state, shiftPressed, togglePressed));
            Assert.False(ReadHidden(state));
        }

        [Fact]
        public void RecoveryChordTogglesVisibilityAndConsumesTheInput()
        {
            var state = CreateState();

            Assert.True(InvokeToggle(state, true, true));
            Assert.True(ReadHidden(state));
            Assert.True(InvokeToggle(state, true, true));
            Assert.False(ReadHidden(state));
        }

        private static object CreateState()
        {
            var type = typeof(ManagementModeState).Assembly.GetType("GodViewManagement.HudVisibilityState");
            Assert.NotNull(type);
            return Activator.CreateInstance(type, nonPublic: true);
        }

        private static bool ReadHidden(object state)
        {
            return (bool)state.GetType().GetProperty("IsHidden", BindingFlags.Instance | BindingFlags.Public).GetValue(state);
        }

        private static void Invoke(object state, string methodName)
        {
            state.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public).Invoke(state, null);
        }

        private static bool InvokeToggle(object state, bool shiftPressed, bool togglePressed)
        {
            return (bool)state.GetType().GetMethod("TryToggle", BindingFlags.Instance | BindingFlags.Public)
                .Invoke(state, new object[] { shiftPressed, togglePressed });
        }
    }
}
