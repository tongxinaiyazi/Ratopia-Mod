using System.Linq;
using StrongerWorkDistance.Core;
using Xunit;

namespace StrongerWorkDistance.Tests
{
    public sealed class WorkAreaRulesTests
    {
        private static readonly WorkOffset[] OriginalOffsets =
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
            new WorkOffset(1, -2)
        };

        [Fact]
        public void ExpandedOffsetsCoverTheCompleteFiveByFiveRectangle()
        {
            var offsets = WorkAreaRules.CreateExpandedOffsets();

            Assert.Equal(25, offsets.Count);
            Assert.Equal(25, offsets.Select(offset => $"{offset.X},{offset.Y}").Distinct().Count());
            Assert.All(offsets, offset => Assert.InRange(offset.X, -2, 2));
            Assert.All(offsets, offset => Assert.InRange(offset.Y, -3, 1));
        }

        [Fact]
        public void ExpandedOffsetsKeepTheOriginalSearchOrderAsTheirPrefix()
        {
            var offsets = WorkAreaRules.CreateExpandedOffsets();

            Assert.Equal(
                OriginalOffsets.Select(offset => (offset.X, offset.Y)),
                offsets.Take(OriginalOffsets.Length).Select(offset => (offset.X, offset.Y)));
        }

        [Fact]
        public void ExpandedOffsetsAppendNewPositionsInThePlannedOrder()
        {
            var expected = new[]
            {
                (-2, 0), (2, 0),
                (-2, 1), (2, 1),
                (-2, -1), (2, -1),
                (-2, -2), (2, -2),
                (-2, -3), (-1, -3), (0, -3), (1, -3), (2, -3)
            };

            var actual = WorkAreaRules.CreateExpandedOffsets()
                .Skip(OriginalOffsets.Length)
                .Select(offset => (offset.X, offset.Y));

            Assert.Equal(expected, actual);
        }
    }
}
