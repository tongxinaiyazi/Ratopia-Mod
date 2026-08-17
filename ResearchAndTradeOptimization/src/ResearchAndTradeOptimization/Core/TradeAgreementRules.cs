using System;

namespace ResearchAndTradeOptimization.Core
{
    internal static class TradeAgreementRules
    {
        internal const int WattResource = 4001;
        private const int RunningState = 1;
        private const int FirstTroubleState = 10;
        private const int LastTroubleState = 17;

        internal static bool IsEditableAgreement(int resource, int state)
        {
            return resource != WattResource &&
                   (state == RunningState ||
                    (state >= FirstTroubleState && state <= LastTroubleState));
        }

        internal static bool IsCountValid(
            int originalCount,
            int requestedCount,
            int currentMaximum)
        {
            if (requestedCount <= 0)
            {
                return false;
            }

            return requestedCount == originalCount ||
                   requestedCount <= Math.Max(1, currentMaximum);
        }

        internal static int GetCurrentMaximumCount(int prosperityLevel)
        {
            var exclusiveMaximum = prosperityLevel / 2 + 2;
            if (exclusiveMaximum <= 1)
            {
                exclusiveMaximum = 2;
            }

            return exclusiveMaximum - 1;
        }

        internal static bool IsQuarterBoundary(int totalDays, int dayOfQuarter)
        {
            return totalDays > 0 &&
                   dayOfQuarter > 0 &&
                   totalDays % dayOfQuarter == 0;
        }

        internal static bool IsInfinitePeriod(int goalTradeCount)
        {
            return goalTradeCount == 0;
        }

        internal static int GetPeriodMinimum(
            bool ordinaryPeriod,
            int vanillaMinimum)
        {
            return ordinaryPeriod ? 0 : vanillaMinimum;
        }

        internal static bool IsSheetRowInteractable(bool editing, int rowType)
        {
            return !editing || rowType == 1 || rowType == 2 || rowType == 127;
        }
    }
}
