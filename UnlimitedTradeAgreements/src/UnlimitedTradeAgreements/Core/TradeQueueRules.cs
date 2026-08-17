using System;

namespace UnlimitedTradeAgreements.Core
{
    internal static class TradeQueueRules
    {
        internal const int VanillaVisibleSlotCount = 7;

        internal static int GetVisibleSlotCount(int tradeCount)
        {
            return Math.Max(VanillaVisibleSlotCount, tradeCount);
        }

        internal static string GetUnlimitedCountLabel(int currentCount)
        {
            return currentCount + "/∞";
        }
    }
}
