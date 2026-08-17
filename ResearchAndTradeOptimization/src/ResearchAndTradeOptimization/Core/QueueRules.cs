using System;

namespace ResearchAndTradeOptimization.Core
{
    internal readonly struct NodePosition
    {
        public NodePosition(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; }

        public float Y { get; }
    }

    internal static class QueueRules
    {
        private const int VanillaResearchLimit = 3;
        internal const int OriginalTradeSlotCount = 7;
        private const float FallbackNodeSpacing = 100f;
        private const float MinimumStepSquared = 0.01f;

        internal static int GetResearchLimit(bool visibleCapacityAvailable)
        {
            return visibleCapacityAvailable ? int.MaxValue : VanillaResearchLimit;
        }

        internal static int GetTradeDisplaySlotCount(int goodsTradeCount)
        {
            return Math.Max(OriginalTradeSlotCount, goodsTradeCount);
        }

        internal static string GetUnlimitedCountLabel(int currentCount)
        {
            return currentCount + "/∞";
        }

        internal static NodePosition GetNextNodePosition(NodePosition previous, NodePosition current)
        {
            var stepX = current.X - previous.X;
            var stepY = current.Y - previous.Y;
            if ((stepX * stepX) + (stepY * stepY) < MinimumStepSquared)
            {
                stepX = FallbackNodeSpacing;
                stepY = 0f;
            }

            return new NodePosition(current.X + stepX, current.Y + stepY);
        }
    }
}
