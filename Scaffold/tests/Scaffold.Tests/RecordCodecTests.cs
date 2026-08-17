using System.Collections.Generic;
using ScaffoldMod.Core;
using Xunit;

namespace ScaffoldMod.Tests
{
    public sealed class RecordCodecTests
    {
        [Fact]
        public void RoundTripsInStableCoordinateOrder()
        {
            var records = new[]
            {
                new ScaffoldRecord(9, 2, 9100, 1),
                new ScaffoldRecord(1, 5, 8200, 0)
            };

            var encoded = ScaffoldRecordCodec.Encode(records);
            var decoded = ScaffoldRecordCodec.Decode(encoded);

            Assert.Equal("v1|1,5,8200,0;9,2,9100,1", encoded);
            Assert.Equal(records.Length, decoded.Count);
            Assert.Equal(new ScaffoldRecord(1, 5, 8200, 0), decoded[0]);
            Assert.Equal(new ScaffoldRecord(9, 2, 9100, 1), decoded[1]);
        }

        [Fact]
        public void MalformedEntriesAreIgnoredWithoutLosingValidEntries()
        {
            IReadOnlyList<ScaffoldRecord> records = ScaffoldRecordCodec.Decode("v1|bad;4,7,9000,3;1,2,x,0;4,7,9100,2");

            Assert.Single(records);
            Assert.Equal(new ScaffoldRecord(4, 7, 9100, 2), records[0]);
        }

        [Fact]
        public void InvalidUnderlyingNodeValuesAreDiscardedAsDamagedRecords()
        {
            var records = ScaffoldRecordCodec.Decode("v1|1,2,8000,-1;3,4,9000,5;5,6,9100,4");

            Assert.Single(records);
            Assert.Equal(new ScaffoldRecord(5, 6, 9100, 4), records[0]);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("v2|1,2,3,4")]
        public void EmptyOrUnknownPayloadReturnsNoRecords(string payload)
        {
            Assert.Empty(ScaffoldRecordCodec.Decode(payload));
        }
    }
}
