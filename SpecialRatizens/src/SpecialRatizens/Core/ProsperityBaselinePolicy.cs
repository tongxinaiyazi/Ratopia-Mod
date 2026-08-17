using System;
using System.Collections.Generic;

namespace SpecialRatizens.Core
{
    internal static class ProsperityBaselinePolicy
    {
        internal static bool Matches(IReadOnlyList<int> liveLevels, IReadOnlyList<int> baselineLevels)
        {
            if (liveLevels == null || baselineLevels == null || liveLevels.Count == 0 ||
                liveLevels.Count != baselineLevels.Count)
            {
                return false;
            }

            for (var index = 0; index < liveLevels.Count; index++)
            {
                if (liveLevels[index] != baselineLevels[index])
                {
                    return false;
                }
            }

            return true;
        }

        internal static int[] ApplyBonus(IReadOnlyList<int> baselinePolicyCounts, int bonus)
        {
            if (baselinePolicyCounts == null)
            {
                throw new ArgumentNullException(nameof(baselinePolicyCounts));
            }

            var result = new int[baselinePolicyCounts.Count];
            for (var index = 0; index < result.Length; index++)
            {
                result[index] = baselinePolicyCounts[index] + bonus;
            }

            return result;
        }
    }
}
