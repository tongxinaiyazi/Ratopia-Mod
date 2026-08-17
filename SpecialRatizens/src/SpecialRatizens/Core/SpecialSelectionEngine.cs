using System;
using System.Collections.Generic;
using System.Linq;

namespace SpecialRatizens.Core
{
    internal sealed class SpecialCandidateState
    {
        public SpecialCandidateState(string name, int grade, int baseProbability, bool isUsed)
        {
            Name = name;
            Grade = grade;
            BaseProbability = baseProbability;
            IsUsed = isUsed;
        }

        public string Name { get; }
        public int Grade { get; }
        public int BaseProbability { get; }
        public int ProbabilityBonus { get; set; }
        public bool IsUsed { get; set; }
        public int RealProbability => BaseProbability + ProbabilityBonus;
    }

    internal static class SpecialSelectionEngine
    {
        private const int GuaranteedThreshold = 10000;

        public static SpecialCandidateState Select(
            IEnumerable<SpecialCandidateState> candidates,
            int prosperityLevel,
            Func<int, int, int> nextRandom)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }
            if (nextRandom == null)
            {
                throw new ArgumentNullException(nameof(nextRandom));
            }

            var available = candidates.Where(item => !item.IsUsed).ToList();
            var selected = available
                .Where(item => item.RealProbability >= GuaranteedThreshold)
                .OrderBy(item => item.BaseProbability)
                .FirstOrDefault();

            foreach (var candidate in available)
            {
                if (selected == null)
                {
                    var adjustedProbability = (int)(candidate.RealProbability * (1f + prosperityLevel * 0.05f));
                    if (nextRandom(0, GuaranteedThreshold) < adjustedProbability)
                    {
                        selected = candidate;
                    }
                }

                candidate.ProbabilityBonus++;
            }

            return selected;
        }

        public static void MarkRecruited(
            IEnumerable<SpecialCandidateState> candidates,
            SpecialCandidateState recruited)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }
            if (recruited == null)
            {
                throw new ArgumentNullException(nameof(recruited));
            }

            recruited.IsUsed = true;
            foreach (var candidate in candidates.Where(item => item.Grade == recruited.Grade))
            {
                candidate.ProbabilityBonus = 0;
            }
        }
    }
}
