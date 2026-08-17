using System;
using System.Collections.Generic;

namespace EquipmentReforgeSelector
{
    internal readonly struct CandidateNavigationRow
    {
        public CandidateNavigationRow(int? upIndex, int? downIndex)
        {
            UpIndex = upIndex;
            DownIndex = downIndex;
        }

        public int? UpIndex { get; }

        public int? DownIndex { get; }
    }

    internal sealed class CandidateNavigationPlan
    {
        private CandidateNavigationPlan(IReadOnlyList<CandidateNavigationRow> rows)
        {
            Rows = rows;
        }

        public IReadOnlyList<CandidateNavigationRow> Rows { get; }

        public int? InitialFocusIndex => null;

        public static CandidateNavigationPlan Create(int rowCount)
        {
            if (rowCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rowCount));
            }

            var rows = new CandidateNavigationRow[rowCount];
            for (var index = 0; index < rowCount; index++)
            {
                rows[index] = new CandidateNavigationRow(
                    index > 0 ? (int?)index - 1 : null,
                    index + 1 < rowCount ? (int?)index + 1 : null);
            }

            return new CandidateNavigationPlan(rows);
        }
    }
}
