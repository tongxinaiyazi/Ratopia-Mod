using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using StrongerWorkDistance.Core;
using Xunit;

namespace StrongerWorkDistance.Tests
{
    public sealed class AtomicListUpdaterTests
    {
        [Fact]
        public void ReplaceBothReplacesBothListsAndRemainsIdempotent()
        {
            var first = new List<int> { 10, 11 };
            var second = new List<int> { 20, 21 };
            var replacement = new[] { 1, 2, 3 };

            AtomicListUpdater.ReplaceBoth(first, second, replacement);
            AtomicListUpdater.ReplaceBoth(first, second, replacement);

            Assert.Equal(replacement, first);
            Assert.Equal(replacement, second);
        }

        [Fact]
        public void ReplaceBothRollsBackBothListsWhenTheSecondReplacementFails()
        {
            var first = new List<int> { 10, 11 };
            var second = new ThrowOnceCollection<int>(new[] { 20, 21 })
            {
                ThrowOnNextInsert = true
            };

            Assert.Throws<InvalidOperationException>(() =>
                AtomicListUpdater.ReplaceBoth(first, second, new[] { 1, 2, 3 }));

            Assert.Equal(new[] { 10, 11 }, first);
            Assert.Equal(new[] { 20, 21 }, second);
        }

        [Fact]
        public void ReplaceBothRejectsNullInputsBeforeMutatingEitherList()
        {
            var first = new List<int> { 10, 11 };
            var second = new List<int> { 20, 21 };
            var replacement = new[] { 1, 2, 3 };

            Assert.Equal(
                "first",
                Assert.Throws<ArgumentNullException>(() =>
                    AtomicListUpdater.ReplaceBoth<int>(null, second, replacement)).ParamName);
            Assert.Equal(
                "second",
                Assert.Throws<ArgumentNullException>(() =>
                    AtomicListUpdater.ReplaceBoth<int>(first, null, replacement)).ParamName);
            Assert.Equal(
                "replacement",
                Assert.Throws<ArgumentNullException>(() =>
                    AtomicListUpdater.ReplaceBoth<int>(first, second, null)).ParamName);

            Assert.Equal(new[] { 10, 11 }, first);
            Assert.Equal(new[] { 20, 21 }, second);
        }

        private sealed class ThrowOnceCollection<T> : Collection<T>
        {
            public ThrowOnceCollection(IEnumerable<T> values)
            {
                foreach (var value in values)
                {
                    Items.Add(value);
                }
            }

            public bool ThrowOnNextInsert { get; set; }

            protected override void InsertItem(int index, T item)
            {
                if (ThrowOnNextInsert)
                {
                    ThrowOnNextInsert = false;
                    throw new InvalidOperationException("Injected replacement failure.");
                }

                base.InsertItem(index, item);
            }
        }
    }
}
