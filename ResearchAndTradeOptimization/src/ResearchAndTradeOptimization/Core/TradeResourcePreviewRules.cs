using System;

namespace ResearchAndTradeOptimization.Core
{
    internal readonly struct TradeResourcePreviewPlan
    {
        internal TradeResourcePreviewPlan(
            int visibleCount,
            int visibleRows)
        {
            VisibleCount = visibleCount;
            VisibleRows = visibleRows;
        }

        public int VisibleCount { get; }

        public int VisibleRows { get; }

    }

    internal readonly struct TradeResourceDetailLayoutPlan
    {
        internal TradeResourceDetailLayoutPlan(
            bool useCompactGrid,
            float contentHeight)
        {
            UseCompactGrid = useCompactGrid;
            CellWidth = useCompactGrid ? 52f : 0f;
            CellHeight = useCompactGrid ? 52f : 0f;
            HorizontalSpacing = useCompactGrid ? 2f : 0f;
            VerticalSpacing = useCompactGrid ? 2f : 0f;
            ContentHeight = useCompactGrid ? contentHeight : 0f;
        }

        public bool UseCompactGrid { get; }

        public float CellWidth { get; }

        public float CellHeight { get; }

        public float HorizontalSpacing { get; }

        public float VerticalSpacing { get; }

        public float ContentHeight { get; }

        public int Columns => 6;
    }

    internal static class TradeResourcePreviewRules
    {
        private const int ResourcesPerRow = 6;
        private const int MaximumRows = 3;
        private const int CompactThreshold = 12;
        private const float CompactCellSize = 52f;
        private const float CompactSpacing = 2f;

        internal static TradeResourcePreviewPlan CreatePlan(int actualCount)
        {
            if (actualCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(actualCount));
            }

            var visible = Math.Min(actualCount, ResourcesPerRow * MaximumRows);
            var visibleRows = visible == 0
                ? 0
                : (visible + ResourcesPerRow - 1) / ResourcesPerRow;
            return new TradeResourcePreviewPlan(
                visible,
                visibleRows);
        }

        internal static TradeResourceDetailLayoutPlan CreateDetailPlan(
            int importCount,
            int exportCount,
            int topPadding)
        {
            if (importCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(importCount));
            }

            if (exportCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(exportCount));
            }

            if (topPadding < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(topPadding));
            }

            var compact = importCount > CompactThreshold ||
                          exportCount > CompactThreshold;
            var contentHeight = topPadding +
                                MaximumRows * CompactCellSize +
                                (MaximumRows - 1) * CompactSpacing;
            return new TradeResourceDetailLayoutPlan(
                compact,
                contentHeight);
        }
    }
}
