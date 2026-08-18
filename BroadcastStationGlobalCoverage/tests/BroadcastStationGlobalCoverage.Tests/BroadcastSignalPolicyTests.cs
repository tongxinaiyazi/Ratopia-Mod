using System.Collections.Generic;
using BroadcastStationGlobalCoverage.Core;
using Xunit;

namespace BroadcastStationGlobalCoverage.Tests
{
    public sealed class BroadcastSignalPolicyTests
    {
        [Theory]
        [InlineData(309, true)]
        [InlineData(310, false)]
        [InlineData(1, false)]
        public void OnlyBroadcastStationIsASignalSource(int buildingName, bool expected)
        {
            Assert.Equal(expected, BroadcastSignalPolicy.IsBroadcastStation(buildingName));
        }

        [Fact]
        public void ManualSelectionAddsEveryMissingSignalSourceExactlyOnce()
        {
            var television = new Candidate(310, 0, 0, true);
            var near = new Candidate(309, 2, 0, true);
            var far = new Candidate(309, 200, 200, true);
            var unrelated = new Candidate(1, 1, 1, true);
            var visible = new List<Candidate> { television, near };

            BroadcastSignalPolicy.AppendMissing(
                visible,
                new[] { television, near, far, unrelated, far },
                item => BroadcastSignalPolicy.IsBroadcastStation(item.BuildingName));

            Assert.Equal(new[] { television, near, far }, visible);
        }

        [Fact]
        public void AutomaticSelectionUsesTheNearestWorkingSourceWithoutARangeCutoff()
        {
            var stopped = new Candidate(309, 1, 1, false);
            var near = new Candidate(309, 10, 0, true);
            var far = new Candidate(309, 200, 200, true);

            var selected = BroadcastSignalPolicy.FindNearest(
                new[] { stopped, far, near },
                item => item.Working,
                item => item.SquaredDistance,
                null);

            Assert.Same(near, selected);
        }

        [Fact]
        public void AutomaticSelectionReturnsFallbackWhenNoSourceIsWorking()
        {
            var television = new Candidate(310, 0, 0, true);
            var stopped = new Candidate(309, 1, 1, false);

            var selected = BroadcastSignalPolicy.FindNearest(
                new[] { stopped },
                item => item.Working,
                item => item.SquaredDistance,
                television);

            Assert.Same(television, selected);
        }

        private sealed class Candidate
        {
            internal Candidate(int buildingName, float x, float y, bool working)
            {
                BuildingName = buildingName;
                SquaredDistance = (x * x) + (y * y);
                Working = working;
            }

            internal int BuildingName { get; }

            internal float SquaredDistance { get; }

            internal bool Working { get; }
        }
    }
}
