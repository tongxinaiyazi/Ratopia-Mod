namespace ResearchAndTradeOptimization.Core
{
    internal static class ResearchReservationRules
    {
        private const int UnpaidStartTime = int.MinValue;

        internal static int GetUnpaidStartTime()
        {
            return UnpaidStartTime;
        }

        internal static bool IsUnpaid(int startTime)
        {
            return startTime == UnpaidStartTime;
        }

        internal static bool CanStartUnpaidHead(int availablePoints, int researchCost)
        {
            return researchCost >= 0 && availablePoints >= researchCost;
        }

        internal static bool ShouldAnnounceReservation(
            int currentQueueCount,
            int availablePoints,
            int researchCost)
        {
            return currentQueueCount > 0 ||
                   !CanStartUnpaidHead(availablePoints, researchCost);
        }

        internal static bool ShouldRefund(int startTime)
        {
            return !IsUnpaid(startTime);
        }
    }
}
