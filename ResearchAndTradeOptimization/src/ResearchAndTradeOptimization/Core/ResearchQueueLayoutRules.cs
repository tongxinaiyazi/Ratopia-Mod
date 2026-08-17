using System;

namespace ResearchAndTradeOptimization.Core
{
    internal readonly struct ResearchQueueDisplayPlan
    {
        internal ResearchQueueDisplayPlan(
            int visibleResearchCount,
            int displayedSlotCount,
            bool showOverflow)
        {
            VisibleResearchCount = visibleResearchCount;
            DisplayedSlotCount = displayedSlotCount;
            ShowOverflow = showOverflow;
        }

        public int VisibleResearchCount { get; }

        public int DisplayedSlotCount { get; }

        public bool ShowOverflow { get; }
    }

    internal static class ResearchQueueLayoutRules
    {
        internal const int MinimumSummarySlotCount = 4;
        internal const int MaximumVisibleResearchCount = 5;

        private const float MinimumHorizontalStep = 1f;
        private const float FallbackHorizontalStep = 100f;
        private const float OriginalContentPadding = 20f;

        internal static float GetHorizontalStep(
            float firstX,
            float secondX,
            float firstWidth)
        {
            var observed = Math.Abs(secondX - firstX);
            return observed >= MinimumHorizontalStep
                ? observed
                : Math.Max(firstWidth, FallbackHorizontalStep);
        }

        internal static int GetSlotCapacity(
            float firstCardRight,
            float viewportRight,
            float horizontalStep)
        {
            if (horizontalStep < MinimumHorizontalStep ||
                viewportRight < firstCardRight)
            {
                return 0;
            }

            return 1 + (int)Math.Floor(
                (viewportRight - firstCardRight) / horizontalStep);
        }

        internal static ResearchQueueDisplayPlan CreateDisplayPlan(int queueCount)
        {
            var safeQueueCount = Math.Max(0, queueCount);
            if (safeQueueCount <= MaximumVisibleResearchCount)
            {
                return new ResearchQueueDisplayPlan(
                    safeQueueCount,
                    safeQueueCount,
                    false);
            }

            return new ResearchQueueDisplayPlan(
                MaximumVisibleResearchCount,
                MaximumVisibleResearchCount + 1,
                true);
        }

        internal static NodePosition GetRowPosition(
            NodePosition first,
            float horizontalStep,
            int index)
        {
            return new NodePosition(
                first.X + (horizontalStep * index),
                first.Y);
        }

        internal static float GetCanvasFallbackRight(
            float firstCardLeft,
            float canvasLeft,
            float canvasRight)
        {
            var safeMargin = Math.Max(0f, firstCardLeft - canvasLeft);
            return canvasRight - safeMargin;
        }

        internal static float GetContentWidth(
            int displayedSlotCount,
            float cardWidth,
            float horizontalStep)
        {
            if (displayedSlotCount <= 0)
            {
                return OriginalContentPadding;
            }

            return cardWidth +
                   ((displayedSlotCount - 1) * horizontalStep) +
                   OriginalContentPadding;
        }

        internal static float GetHorizontalAlignmentShift(
            float areaLeft,
            float canvasLeft)
        {
            return canvasLeft - areaLeft;
        }
    }
}
