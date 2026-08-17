using System.Collections.Generic;
using WireThroughWalls.Core;
using Xunit;

namespace WireThroughWalls.Tests
{
    public sealed class ScopedListMaskTests
    {
        [Fact]
        public void RemoveWhereRestoresTheExactOriginalOrder()
        {
            var items = new List<string> { "wire-a", "wall", "wire-b", "road" };

            using (ScopedListMask<string>.RemoveWhere(items, item => item.StartsWith("wire")))
            {
                Assert.Equal(new[] { "wall", "road" }, items);
            }

            Assert.Equal(new[] { "wire-a", "wall", "wire-b", "road" }, items);
        }

        [Fact]
        public void DisposeIsIdempotent()
        {
            var items = new List<string> { "wire", "wall" };
            var scope = ScopedListMask<string>.RemoveWhere(items, item => item == "wire");

            scope.Dispose();
            scope.Dispose();

            Assert.Equal(new[] { "wire", "wall" }, items);
        }

        [Fact]
        public void NestedMasksRestoreTheirOwnViews()
        {
            var items = new List<string> { "wire-a", "wall", "wire-b", "road" };
            var outer = ScopedListMask<string>.RemoveWhere(items, item => item.StartsWith("wire"));
            var inner = ScopedListMask<string>.RemoveWhere(items, item => item == "wall");

            Assert.Equal(new[] { "road" }, items);

            inner.Dispose();
            Assert.Equal(new[] { "wall", "road" }, items);

            outer.Dispose();
            Assert.Equal(new[] { "wire-a", "wall", "wire-b", "road" }, items);
        }

        [Fact]
        public void EmptyMatchLeavesTheListUntouched()
        {
            var items = new List<string> { "wall", "road" };

            using (ScopedListMask<string>.RemoveWhere(items, item => item == "wire"))
            {
                Assert.Equal(new[] { "wall", "road" }, items);
            }

            Assert.Equal(new[] { "wall", "road" }, items);
        }
    }
}
