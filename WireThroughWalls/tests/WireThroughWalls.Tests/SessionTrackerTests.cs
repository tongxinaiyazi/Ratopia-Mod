using WireThroughWalls.Core;
using Xunit;

namespace WireThroughWalls.Tests
{
    public sealed class SessionTrackerTests
    {
        [Fact]
        public void WaitsForLoadingToFinishBeforeInitialization()
        {
            var tracker = new SessionTracker<object>();
            var manager = new object();

            Assert.Equal(SessionAction.None, tracker.Observe(manager, isLoading: true));
            Assert.Equal(SessionAction.Initialize, tracker.Observe(manager, isLoading: false));
        }

        [Fact]
        public void InitializesOnlyOnceForTheSameManager()
        {
            var tracker = new SessionTracker<object>();
            var manager = new object();

            Assert.Equal(SessionAction.Initialize, tracker.Observe(manager, isLoading: false));
            tracker.MarkInitialized();

            Assert.Equal(SessionAction.None, tracker.Observe(manager, isLoading: false));
        }

        [Fact]
        public void ManagerReplacementStartsANewSession()
        {
            var tracker = new SessionTracker<object>();
            var first = new object();
            var second = new object();
            tracker.Observe(first, isLoading: false);
            tracker.MarkInitialized();

            Assert.Equal(SessionAction.Reset, tracker.Observe(second, isLoading: true));
            Assert.Equal(SessionAction.Initialize, tracker.Observe(second, isLoading: false));
        }

        [Fact]
        public void MissingManagerResetsAnInitializedSession()
        {
            var tracker = new SessionTracker<object>();
            var manager = new object();
            tracker.Observe(manager, isLoading: false);
            tracker.MarkInitialized();

            Assert.Equal(SessionAction.Reset, tracker.Observe(null, isLoading: false));
            Assert.Equal(SessionAction.None, tracker.Observe(null, isLoading: false));
        }

        [Fact]
        public void FailedInitializationCanBeRetried()
        {
            var tracker = new SessionTracker<object>();
            var manager = new object();

            Assert.Equal(SessionAction.Initialize, tracker.Observe(manager, isLoading: false));
            tracker.MarkInitializationFailed();

            Assert.Equal(SessionAction.Initialize, tracker.Observe(manager, isLoading: false));
        }
    }
}
