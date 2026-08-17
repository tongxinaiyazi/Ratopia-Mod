using System;
using System.Collections.Generic;
using SpecialRatizens.Core;
using Xunit;

namespace SpecialRatizens.Tests
{
    public sealed class SpecialSelectionEngineTests
    {
        [Fact]
        public void SelectsAnAvailableCandidateUsingProsperityAdjustedProbabilityAndIncrementsAllAvailableBonuses()
        {
            var first = new SpecialCandidateState("First", 0, 100, false);
            var second = new SpecialCandidateState("Second", 0, 100, false);
            var randomValues = new Queue<int>(new[] { 149, 9999 });

            var selected = SpecialSelectionEngine.Select(
                new[] { first, second },
                prosperityLevel: 10,
                nextRandom: (min, max) => randomValues.Dequeue());

            Assert.Same(first, selected);
            Assert.Equal(1, first.ProbabilityBonus);
            Assert.Equal(1, second.ProbabilityBonus);
        }

        [Fact]
        public void ExcludesUsedCandidatesAndForcesTheLowestBaseProbabilityAtThreshold()
        {
            var used = new SpecialCandidateState("Used", 0, 5, true) { ProbabilityBonus = 20000 };
            var high = new SpecialCandidateState("High", 1, 50, false) { ProbabilityBonus = 9950 };
            var low = new SpecialCandidateState("Low", 1, 10, false) { ProbabilityBonus = 9990 };

            var selected = SpecialSelectionEngine.Select(
                new[] { used, high, low },
                prosperityLevel: 0,
                nextRandom: (min, max) => throw new InvalidOperationException("forced selection must not roll"));

            Assert.Same(low, selected);
            Assert.Equal(20000, used.ProbabilityBonus);
        }

        [Fact]
        public void RecruitingCandidateResetsProbabilityBonusForItsGradeOnly()
        {
            var sameA = new SpecialCandidateState("A", 2, 10, false) { ProbabilityBonus = 4 };
            var sameB = new SpecialCandidateState("B", 2, 20, false) { ProbabilityBonus = 8 };
            var other = new SpecialCandidateState("C", 1, 10, false) { ProbabilityBonus = 7 };

            SpecialSelectionEngine.MarkRecruited(new[] { sameA, sameB, other }, sameA);

            Assert.True(sameA.IsUsed);
            Assert.Equal(0, sameA.ProbabilityBonus);
            Assert.Equal(0, sameB.ProbabilityBonus);
            Assert.Equal(7, other.ProbabilityBonus);
        }
    }
}
