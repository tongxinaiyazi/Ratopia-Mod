using System;

namespace ScaffoldMod.Core
{
    internal static class ScaffoldClock
    {
        internal const int LifetimeMinutes = 5 * 24 * 60;

        internal static int GetExpiryMinute(int createdAtMinute)
        {
            return checked(createdAtMinute + LifetimeMinutes);
        }

        internal static bool IsExpired(int nowMinute, int expiryMinute)
        {
            return nowMinute >= expiryMinute;
        }

        internal static string FormatRemaining(int nowMinute, int expiryMinute)
        {
            var remaining = expiryMinute - nowMinute;
            if (remaining <= 0)
            {
                return "已到期";
            }

            if (remaining < 60)
            {
                return "不足1小时";
            }

            var days = remaining / (24 * 60);
            var hours = remaining % (24 * 60) / 60;
            return days > 0
                ? string.Format("{0}天{1}小时", days, hours)
                : string.Format("{0}小时", hours);
        }
    }
}
