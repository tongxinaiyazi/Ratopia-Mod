using Xunit;

namespace GodViewManagement.Tests
{
    public sealed class InputBindingRulesTests
    {
        [Theory]
        [InlineData("LeftShift")]
        [InlineData("RightCtrl")]
        [InlineData("LeftAlt")]
        [InlineData("LeftMeta")]
        [InlineData("AltGr")]
        [InlineData("LeftWindows")]
        [InlineData("RightCommand")]
        [InlineData("RightApple")]
        public void ModifierOnlyKeysAreRejected(string key)
        {
            Assert.Equal(BindingDecision.ModifierOnly, InputBindingRules.Evaluate(key, false));
        }

        [Fact]
        public void EscapeCancelsCapture()
        {
            Assert.Equal(BindingDecision.Cancelled, InputBindingRules.Evaluate("Escape", false));
        }

        [Fact]
        public void ConflictingKeyIsRejected()
        {
            Assert.Equal(BindingDecision.Conflict, InputBindingRules.Evaluate("Tab", true));
        }

        [Fact]
        public void FreeKeyIsAccepted()
        {
            Assert.Equal(BindingDecision.Accepted, InputBindingRules.Evaluate("M", false));
        }
    }
}
