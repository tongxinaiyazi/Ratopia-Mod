using System;
using System.Collections.Generic;

namespace EquipmentReforgeSelector
{
    public sealed class CandidateResolution
    {
        private static readonly IReadOnlyList<ReforgeCandidate> EmptyCandidates = new ReforgeCandidate[0];

        public static readonly CandidateResolution Unavailable = new CandidateResolution(false, EmptyCandidates);

        public CandidateResolution(bool isAvailable, IReadOnlyList<ReforgeCandidate> candidates)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            IsAvailable = isAvailable;
            Candidates = candidates;
        }

        public bool IsAvailable { get; private set; }

        public IReadOnlyList<ReforgeCandidate> Candidates { get; private set; }
    }
}
