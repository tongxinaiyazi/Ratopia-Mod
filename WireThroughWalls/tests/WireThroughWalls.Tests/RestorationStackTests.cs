using System;
using System.Collections.Generic;
using WireThroughWalls.Core;
using Xunit;

namespace WireThroughWalls.Tests
{
    public sealed class RestorationStackTests
    {
        [Fact]
        public void DisposeRunsEveryRestorationInReverseOrder()
        {
            var calls = new List<int>();
            var stack = new RestorationStack();
            stack.Push(new TrackingDisposable(() => calls.Add(1)));
            stack.Push(new TrackingDisposable(() => calls.Add(2)));
            stack.Push(new TrackingDisposable(() => calls.Add(3)));

            stack.Dispose();

            Assert.Equal(new[] { 3, 2, 1 }, calls);
        }

        [Fact]
        public void DisposeContinuesRestoringAfterOneScopeThrows()
        {
            var calls = new List<int>();
            var stack = new RestorationStack();
            stack.Push(new TrackingDisposable(() => calls.Add(1)));
            stack.Push(new TrackingDisposable(() => throw new InvalidOperationException("boom")));
            stack.Push(new TrackingDisposable(() => calls.Add(3)));

            var error = Assert.Throws<AggregateException>(() => stack.Dispose());

            Assert.Single(error.InnerExceptions);
            Assert.Equal(new[] { 3, 1 }, calls);
        }

        [Fact]
        public void DisposeIsIdempotent()
        {
            var count = 0;
            var stack = new RestorationStack();
            stack.Push(new TrackingDisposable(() => count++));

            stack.Dispose();
            stack.Dispose();

            Assert.Equal(1, count);
        }

        private sealed class TrackingDisposable : IDisposable
        {
            private Action _action;

            internal TrackingDisposable(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                var action = _action;
                _action = null;
                action?.Invoke();
            }
        }
    }
}
