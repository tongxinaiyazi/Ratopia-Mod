using WireThroughWalls.Core;
using Xunit;

namespace WireThroughWalls.Tests
{
    public sealed class ScopedStateTests
    {
        [Fact]
        public void ScopedFlagRemainsActiveUntilEveryNestedScopeIsDisposed()
        {
            var flag = new ScopedFlag();
            var outer = flag.Enter();
            var inner = flag.Enter();

            Assert.True(flag.IsActive);
            outer.Dispose();
            Assert.True(flag.IsActive);
            inner.Dispose();
            Assert.False(flag.IsActive);
        }

        [Fact]
        public void ScopedFlagDisposeIsIdempotent()
        {
            var flag = new ScopedFlag();
            var scope = flag.Enter();

            scope.Dispose();
            scope.Dispose();

            Assert.False(flag.IsActive);
        }

        [Fact]
        public void ScopedMembershipReferenceCountsOverlappingScopes()
        {
            var membership = new ScopedMembership<string>();
            var outer = membership.Enter(new[] { "1,1", "2,2" });
            var inner = membership.Enter(new[] { "1,1" });

            outer.Dispose();
            Assert.True(membership.Contains("1,1"));
            Assert.False(membership.Contains("2,2"));

            inner.Dispose();
            Assert.False(membership.Contains("1,1"));
        }

        [Fact]
        public void ScopedMembershipIgnoresRepeatedDispose()
        {
            var membership = new ScopedMembership<int>();
            var scope = membership.Enter(new[] { 4, 4 });

            scope.Dispose();
            scope.Dispose();

            Assert.False(membership.Contains(4));
        }
    }
}
