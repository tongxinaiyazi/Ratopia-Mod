using Xunit;

namespace GodViewManagement.Tests
{
    public sealed class QueenInputIsolationRulesTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(20)]
        [InlineData(22)]
        [InlineData(23)]
        [InlineData(24)]
        [InlineData(25)]
        [InlineData(27)]
        [InlineData(28)]
        [InlineData(29)]
        public void DirectionalHotKeysAreSuppressedDuringEnabledQueenUpdate(int hotKeyValue)
        {
            Assert.True(QueenInputIsolationRules.ShouldSuppress(true, true, hotKeyValue));
        }

        [Fact]
        public void DoesNotSuppressWhenModeIsDisabled()
        {
            Assert.False(QueenInputIsolationRules.ShouldSuppress(false, true, 0));
        }

        [Fact]
        public void DoesNotSuppressOutsideQueenUpdate()
        {
            Assert.False(QueenInputIsolationRules.ShouldSuppress(true, false, 0));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(8)]
        [InlineData(19)]
        [InlineData(21)]
        [InlineData(26)]
        [InlineData(30)]
        public void NonDirectionalHotKeysAreNotSuppressed(int hotKeyValue)
        {
            Assert.False(QueenInputIsolationRules.ShouldSuppress(true, true, hotKeyValue));
        }
    }
}
