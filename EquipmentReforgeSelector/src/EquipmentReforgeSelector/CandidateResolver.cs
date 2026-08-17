using System;
using System.Collections.Generic;

namespace EquipmentReforgeSelector
{
    public sealed class CandidateResolver
    {
        public static CandidateResolution Resolve(
            int level,
            int currentAbilityId,
            IReadOnlyList<int> abilityIds,
            IReadOnlyList<float> values)
        {
            if (level != 1 && level != 2)
            {
                return CandidateResolution.Unavailable;
            }

            if (abilityIds == null)
            {
                throw new ArgumentNullException(nameof(abilityIds));
            }

            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (abilityIds.Count != values.Count)
            {
                return CandidateResolution.Unavailable;
            }

            var candidates = new List<ReforgeCandidate>();
            for (var index = 0; index < abilityIds.Count; index++)
            {
                if (abilityIds[index] != currentAbilityId)
                {
                    candidates.Add(new ReforgeCandidate(abilityIds[index], values[index]));
                }
            }

            return new CandidateResolution(true, candidates);
        }
    }
}
