using SpecialRatizens.Core;
using Xunit;

namespace SpecialRatizens.Tests
{
    public sealed class ProsperityBaselinePolicyTests
    {
        [Fact]
        public void RequiresTheSameNonEmptyLevelSequence()
        {
            Assert.True(ProsperityBaselinePolicy.Matches(new[] { 1, 2, 3 }, new[] { 1, 2, 3 }));
            Assert.False(ProsperityBaselinePolicy.Matches(new int[0], new int[0]));
            Assert.False(ProsperityBaselinePolicy.Matches(new[] { 1, 2, 3 }, new[] { 1, 2 }));
            Assert.False(ProsperityBaselinePolicy.Matches(new[] { 1, 3 }, new[] { 1, 2 }));
            Assert.False(ProsperityBaselinePolicy.Matches(null, new[] { 1 }));
            Assert.False(ProsperityBaselinePolicy.Matches(new[] { 1 }, null));
        }

        [Fact]
        public void BonusAlwaysUsesTheBaselineAndNeverAccumulates()
        {
            var baseline = new[] { 2, 3, 4 };

            Assert.Equal(new[] { 7, 8, 9 }, ProsperityBaselinePolicy.ApplyBonus(baseline, 5));
            Assert.Equal(new[] { 7, 8, 9 }, ProsperityBaselinePolicy.ApplyBonus(baseline, 5));
            Assert.Equal(baseline, ProsperityBaselinePolicy.ApplyBonus(baseline, 0));
        }
    }
}
