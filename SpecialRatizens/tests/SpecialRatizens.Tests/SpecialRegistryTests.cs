using System.Linq;
using SpecialRatizens.Core;
using Xunit;

namespace SpecialRatizens.Tests
{
    public sealed class SpecialRegistryTests
    {
        [Fact]
        public void ReloadIsIdempotentAndRebuildMarksMatchingSavedTraits()
        {
            var definition = new SpecialRatizenDefinition(
                "Saved Rat", "#fff", "Unlock", "Female", 1, 1, 2, 3, 4,
                "Saved_A", "IconA", "Saved_B", "IconB", 10,
                "White", "Face_1", "", "Dress_1", "", "Hair_1", "", "");
            var registry = new SpecialRegistry();

            registry.Reload(new[] { definition });
            registry.Reload(new[] { definition });
            registry.RebuildUsedFromTraits(new[] { "Saved_B" });

            Assert.Single(registry.Candidates);
            Assert.True(registry.Candidates.Single().IsUsed);
        }

        [Fact]
        public void ResetSessionClearsUsedFlagsAndProbabilityBonuses()
        {
            var definition = new SpecialRatizenDefinition(
                "Rat", "#fff", "Unlock", "Male", 0, 1, 1, 1, 1,
                "A", "IA", "B", "IB", 5,
                "White", "Face_1", "", "Dress_1", "", "Hair_1", "", "");
            var registry = new SpecialRegistry();
            registry.Reload(new[] { definition });
            registry.Candidates[0].IsUsed = true;
            registry.Candidates[0].ProbabilityBonus = 42;

            registry.ResetSession();

            Assert.False(registry.Candidates[0].IsUsed);
            Assert.Equal(0, registry.Candidates[0].ProbabilityBonus);
        }
    }
}
