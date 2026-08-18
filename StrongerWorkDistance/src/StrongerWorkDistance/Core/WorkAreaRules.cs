using System.Collections.Generic;

namespace StrongerWorkDistance.Core
{
    public static class WorkAreaRules
    {
        public static IReadOnlyList<WorkOffset> CreateExpandedOffsets()
        {
            var offsets = new List<WorkOffset>(25);
            offsets.AddRange(new[]
            {
                new WorkOffset(-1, 0),
                new WorkOffset(1, 0),
                new WorkOffset(0, 0),
                new WorkOffset(-1, 1),
                new WorkOffset(0, 1),
                new WorkOffset(1, 1),
                new WorkOffset(-1, -1),
                new WorkOffset(0, -1),
                new WorkOffset(1, -1),
                new WorkOffset(0, -2),
                new WorkOffset(-1, -2),
                new WorkOffset(1, -2),
                new WorkOffset(-2, 0),
                new WorkOffset(2, 0),
                new WorkOffset(-2, 1),
                new WorkOffset(2, 1),
                new WorkOffset(-2, -1),
                new WorkOffset(2, -1),
                new WorkOffset(-2, -2),
                new WorkOffset(2, -2),
                new WorkOffset(-2, -3),
                new WorkOffset(-1, -3),
                new WorkOffset(0, -3),
                new WorkOffset(1, -3),
                new WorkOffset(2, -3)
            });

            return offsets;
        }
    }
}
