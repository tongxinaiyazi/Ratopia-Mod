using System;
using System.Collections.Generic;

namespace EquipmentReforgeSelector
{
    public sealed class InlineCandidatePlan
    {
        private InlineCandidatePlan(IReadOnlyList<InlineCandidateRow> rows)
        {
            Rows = rows;
        }

        public IReadOnlyList<InlineCandidateRow> Rows { get; }

        public static InlineCandidatePlan Create(
            IReadOnlyList<ReforgeCandidate> candidates,
            ReforgeCandidate? selected)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            var rows = new InlineCandidateRow[candidates.Count];
            for (var index = 0; index < candidates.Count; index++)
            {
                rows[index] = new InlineCandidateRow(
                    index,
                    candidates[index],
                    selected.HasValue && candidates[index] == selected.Value);
            }

            return new InlineCandidatePlan(rows);
        }
    }
}
