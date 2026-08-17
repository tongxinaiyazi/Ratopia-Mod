using System;
using System.Globalization;

namespace ResearchAndTradeOptimization.Core
{
    internal enum TradeHighlightKind
    {
        None = 0,
        Limited = 1,
        Infinite = 2
    }

    internal readonly struct TradeHighlightColor
    {
        internal TradeHighlightColor(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public byte Red { get; }

        public byte Green { get; }

        public byte Blue { get; }
    }

    internal static class TradeResourceStateRules
    {
        internal const string ConfigSection = "TradeDetailSlot";
        internal const string ActiveTradeColorKey = "ActiveTradeBackgroundColor";
        internal const string ActiveTradeColorDefault = "145,135,106";
        internal const string InfiniteTradeColorKey = "InfiniteTradeBackgroundColor";
        internal const string InfiniteTradeColorDefault = "96,169,23";

        internal static readonly TradeHighlightColor DefaultHighlightColor =
            ParseColorOrDefault(ActiveTradeColorDefault, new TradeHighlightColor(145, 135, 106));

        internal static readonly TradeHighlightColor DefaultInfiniteHighlightColor =
            ParseColorOrDefault(InfiniteTradeColorDefault, new TradeHighlightColor(96, 169, 23));

        internal static bool ShouldHighlight(
            bool isVisibleSlot,
            bool isCurrentlyTrading)
        {
            return isVisibleSlot && isCurrentlyTrading;
        }

        internal static TradeHighlightKind GetHighlightKind(
            bool isVisibleSlot,
            bool isCurrentlyTrading,
            bool isInfinitePeriod)
        {
            if (!ShouldHighlight(isVisibleSlot, isCurrentlyTrading))
            {
                return TradeHighlightKind.None;
            }

            return isInfinitePeriod
                ? TradeHighlightKind.Infinite
                : TradeHighlightKind.Limited;
        }

        internal static TradeHighlightColor ParseColorOrDefault(
            string text,
            TradeHighlightColor fallback)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return fallback;
            }

            var parts = text.Split(',');
            if (parts.Length != 3)
            {
                return fallback;
            }

            var channels = new byte[3];
            for (var index = 0; index < parts.Length; index++)
            {
                if (!byte.TryParse(
                        parts[index].Trim(),
                        NumberStyles.None,
                        CultureInfo.InvariantCulture,
                        out var value))
                {
                    return fallback;
                }

                channels[index] = value;
            }

            return new TradeHighlightColor(channels[0], channels[1], channels[2]);
        }
    }
}
