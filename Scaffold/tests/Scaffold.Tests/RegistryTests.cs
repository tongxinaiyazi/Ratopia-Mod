using ScaffoldMod.Core;
using Xunit;

namespace ScaffoldMod.Tests
{
    public sealed class RegistryTests
    {
        [Fact]
        public void AddIsUniquePerCoordinateAndRemoveIsIdempotent()
        {
            var registry = new ScaffoldRegistry();
            var first = new ScaffoldRecord(3, 4, 8000, 0);
            var duplicate = new ScaffoldRecord(3, 4, 9000, 1);

            Assert.True(registry.TryAdd(first));
            Assert.False(registry.TryAdd(duplicate));
            Assert.True(registry.TryRemove(3, 4, out var removed));
            Assert.Equal(first, removed);
            Assert.False(registry.TryRemove(3, 4, out _));
        }

        [Fact]
        public void LoadingSameRecordsTwiceDoesNotDuplicateEntries()
        {
            var registry = new ScaffoldRegistry();
            var records = new[] { new ScaffoldRecord(1, 1, 7200, 0) };

            registry.ReplaceWith(records);
            registry.ReplaceWith(records);

            Assert.Single(registry.Snapshot());
        }
    }
}
