using ScaffoldMod.Core;
using Xunit;

namespace ScaffoldMod.Tests
{
    public sealed class GameClockTests
    {
        [Fact]
        public void LifetimeIsExactlyFiveFullGameDays()
        {
            Assert.Equal(7200, ScaffoldClock.LifetimeMinutes);
            Assert.Equal(11200, ScaffoldClock.GetExpiryMinute(4000));
        }

        [Fact]
        public void ExpiryOccursAtBoundaryButNotOneMinuteBefore()
        {
            Assert.False(ScaffoldClock.IsExpired(11199, 11200));
            Assert.True(ScaffoldClock.IsExpired(11200, 11200));
        }

        [Fact]
        public void PausingDoesNotConsumeLifetimeWhenTheGameMinuteDoesNotAdvance()
        {
            var expiry = ScaffoldClock.GetExpiryMinute(2400);

            Assert.Equal("5天0小时", ScaffoldClock.FormatRemaining(2400, expiry));
            Assert.Equal("5天0小时", ScaffoldClock.FormatRemaining(2400, expiry));
            Assert.False(ScaffoldClock.IsExpired(2400, expiry));
        }

        [Theory]
        [InlineData(0, 7200, "5天0小时")]
        [InlineData(60, 1500, "1天0小时")]
        [InlineData(100, 159, "不足1小时")]
        [InlineData(100, 100, "已到期")]
        public void RemainingTimeUsesChineseDaysAndHours(int now, int expiry, string expected)
        {
            Assert.Equal(expected, ScaffoldClock.FormatRemaining(now, expiry));
        }
    }
}
