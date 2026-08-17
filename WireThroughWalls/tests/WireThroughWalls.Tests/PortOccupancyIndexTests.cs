using System.Linq;
using WireThroughWalls.Core;
using Xunit;

namespace WireThroughWalls.Tests
{
    public sealed class PortOccupancyIndexTests
    {
        [Fact]
        public void ForegroundPortWinsRegardlessOfRegistrationOrder()
        {
            var index = new PortOccupancyIndex<string, string>();
            index.Register("10,20", 30, PortOwnerKind.HeavyWire, "wire");
            index.Register("10,20", 20, PortOwnerKind.WireRoad, "road");
            index.Register("10,20", 99, PortOwnerKind.ForegroundBuilding, "battery");

            Assert.True(index.TryGetRepresentative("10,20", out var representative));
            Assert.Equal("battery", representative.Value);
        }

        [Fact]
        public void SameKindUsesLowestOwnerIdAsRepresentative()
        {
            var index = new PortOccupancyIndex<string, string>();
            index.Register("10,20", 30, PortOwnerKind.HeavyWire, "later");
            index.Register("10,20", 10, PortOwnerKind.HeavyWire, "earlier");

            Assert.True(index.TryGetRepresentative("10,20", out var representative));
            Assert.Equal(10, representative.OwnerId);
        }

        [Fact]
        public void RegisteringTheSameOwnerAgainReplacesInsteadOfDuplicating()
        {
            var index = new PortOccupancyIndex<string, string>();
            index.Register("10,20", 10, PortOwnerKind.HeavyWire, "old");
            index.Register("10,20", 10, PortOwnerKind.HeavyWire, "new");

            var owners = index.GetOwners("10,20");
            Assert.Single(owners);
            Assert.Equal("new", owners[0].Value);
        }

        [Fact]
        public void ReclassifyingAnOwnerDoesNotLeaveAStaleDuplicate()
        {
            var index = new PortOccupancyIndex<string, string>();
            index.Register("10,20", 10, PortOwnerKind.ForegroundBuilding, "early");
            index.Register("10,20", 10, PortOwnerKind.HeavyWire, "classified");

            var owners = index.GetOwners("10,20");
            Assert.Single(owners);
            Assert.Equal(PortOwnerKind.HeavyWire, owners[0].Kind);
            Assert.Equal("classified", owners[0].Value);
        }

        [Fact]
        public void RemovingRepresentativePromotesTheBestSurvivor()
        {
            var index = new PortOccupancyIndex<string, string>();
            index.Register("10,20", 90, PortOwnerKind.ForegroundBuilding, "foreground");
            index.Register("10,20", 20, PortOwnerKind.WireRoad, "road");
            index.Register("10,20", 10, PortOwnerKind.HeavyWire, "wire");

            Assert.True(index.Remove("10,20", 90));
            Assert.True(index.TryGetRepresentative("10,20", out var representative));
            Assert.Equal("road", representative.Value);
        }

        [Fact]
        public void MultiOwnerPositionsExcludeSingleOwnerCells()
        {
            var index = new PortOccupancyIndex<string, string>();
            index.Register("single", 1, PortOwnerKind.HeavyWire, "wire");
            index.Register("overlap", 2, PortOwnerKind.HeavyWire, "wire");
            index.Register("overlap", 3, PortOwnerKind.ForegroundBuilding, "device");

            Assert.Equal(new[] { "overlap" }, index.MultiOwnerPositions.ToArray());
        }

        [Fact]
        public void ClearRemovesEverySessionEntry()
        {
            var index = new PortOccupancyIndex<string, string>();
            index.Register("10,20", 1, PortOwnerKind.HeavyWire, "wire");

            index.Clear();

            Assert.Empty(index.GetOwners("10,20"));
            Assert.Empty(index.MultiOwnerPositions);
        }
    }
}
