using System.Linq;
using SuperBow.Core;
using Xunit;

namespace SuperBow.Tests
{
    public sealed class BleedTrackerTests
    {
        [Fact]
        public void Normal_target_ticks_at_one_two_and_three_seconds()
        {
            var tracker = new BleedTracker<string>();
            tracker.ApplyOrRefresh("rat", 0f);

            Assert.Empty(tracker.Advance(0.99f, _ => true, _ => false));
            Assert.Single(tracker.Advance(1f, _ => true, _ => false));
            Assert.Single(tracker.Advance(2f, _ => true, _ => false));
            var final = tracker.Advance(3f, _ => true, _ => false);

            Assert.Single(final);
            Assert.Equal(0.03f, final[0].Fraction);
            Assert.Equal(0, tracker.Count);
        }

        [Fact]
        public void Boss_ticks_use_one_percent()
        {
            var tracker = new BleedTracker<string>();
            tracker.ApplyOrRefresh("boss", 0f);

            var ticks = tracker.Advance(3f, _ => true, _ => true);

            Assert.Equal(3, ticks.Count);
            Assert.All(ticks, tick => Assert.Equal(0.01f, tick.Fraction));
        }

        [Fact]
        public void Refresh_extends_expiry_without_stacking_or_resetting_next_tick()
        {
            var tracker = new BleedTracker<string>();
            tracker.ApplyOrRefresh("rat", 0f);
            tracker.ApplyOrRefresh("rat", 0.5f);

            Assert.Equal(1, tracker.Count);
            Assert.Single(tracker.Advance(1f, _ => true, _ => false));
            Assert.Equal(2, tracker.Advance(3.5f, _ => true, _ => false).Count);
            Assert.Equal(0, tracker.Count);
        }

        [Fact]
        public void Large_frame_emits_catch_up_ticks_and_invalid_targets_are_removed()
        {
            var tracker = new BleedTracker<string>();
            tracker.ApplyOrRefresh("alive", 0f);
            tracker.ApplyOrRefresh("gone", 0f);

            var ticks = tracker.Advance(2.2f, target => target == "alive", _ => false);

            Assert.Equal(new[] { "alive", "alive" }, ticks.Select(tick => tick.Target).ToArray());
            Assert.Equal(1, tracker.Count);
            tracker.Clear();
            Assert.Equal(0, tracker.Count);
        }
    }
}
