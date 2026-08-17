using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace ResearchAndTradeOptimization.Tests
{
    public sealed class ResearchQueueLayoutRulesTests
    {
        [Theory]
        [InlineData(20f, 180f, 150f, 160f)]
        [InlineData(20f, 20f, 150f, 150f)]
        [InlineData(20f, 20.001f, 80f, 100f)]
        public void HorizontalStepUsesOriginalFirstRowOrSafeFallback(
            float firstX,
            float secondX,
            float width,
            float expected)
        {
            Assert.Equal(
                expected,
                Invoke<float>("GetHorizontalStep", firstX, secondX, width));
        }

        [Theory]
        [InlineData(170f, 970f, 160f, 6)]
        [InlineData(170f, 649f, 160f, 3)]
        [InlineData(170f, 169f, 160f, 0)]
        public void SlotCapacityCountsOnlyFullyVisibleCards(
            float firstRight,
            float viewportRight,
            float step,
            int expected)
        {
            Assert.Equal(
                expected,
                Invoke<int>("GetSlotCapacity", firstRight, viewportRight, step));
        }

        [Theory]
        [InlineData(0, 0, 0, false)]
        [InlineData(1, 1, 1, false)]
        [InlineData(5, 5, 5, false)]
        [InlineData(6, 5, 6, true)]
        [InlineData(8, 5, 6, true)]
        [InlineData(100, 5, 6, true)]
        public void DisplayPlanShowsTheEarliestFiveThenOverflow(
            int queueCount,
            int visibleResearch,
            int displayedSlots,
            bool overflow)
        {
            var plan = Invoke<object>("CreateDisplayPlan", queueCount);
            Assert.Equal(visibleResearch, Read<int>(plan, "VisibleResearchCount"));
            Assert.Equal(displayedSlots, Read<int>(plan, "DisplayedSlotCount"));
            Assert.Equal(overflow, Read<bool>(plan, "ShowOverflow"));
        }

        [Fact]
        public void EveryPositionStaysOnTheFirstRow()
        {
            var result = InvokePoint(20f, 80f, 160f, 5);
            Assert.Equal(820f, result.x);
            Assert.Equal(80f, result.y);
        }

        [Theory]
        [InlineData(20f, 0f, 1000f, 980f)]
        [InlineData(-10f, 0f, 1000f, 1000f)]
        public void CanvasFallbackPreservesTheSafeRightMargin(
            float firstCardLeft,
            float canvasLeft,
            float canvasRight,
            float expected)
        {
            Assert.Equal(
                expected,
                Invoke<float>(
                    "GetCanvasFallbackRight",
                    firstCardLeft,
                    canvasLeft,
                    canvasRight));
        }

        [Theory]
        [InlineData(0, 150f, 160f, 20f)]
        [InlineData(1, 150f, 160f, 170f)]
        [InlineData(6, 150f, 160f, 970f)]
        public void ContentWidthTracksOnlyDisplayedSlots(
            int slots,
            float width,
            float step,
            float expected)
        {
            Assert.Equal(
                expected,
                Invoke<float>("GetContentWidth", slots, width, step));
        }

        [Theory]
        [InlineData(-1091.8f, -960f, 131.8f)]
        [InlineData(-960f, -960f, 0f)]
        [InlineData(-900f, -960f, -60f)]
        public void HorizontalAlignmentShiftPinsTheAreaToTheCanvasLeft(
            float areaLeft,
            float canvasLeft,
            float expected)
        {
            Assert.Equal(
                expected,
                Invoke<float>(
                    "GetHorizontalAlignmentShift",
                    areaLeft,
                    canvasLeft),
                3);
        }

        private static T Invoke<T>(string name, params object[] args)
        {
            var type = Load().GetType(
                "ResearchAndTradeOptimization.Core.ResearchQueueLayoutRules",
                false);
            Assert.NotNull(type);
            var method = type.GetMethod(
                name,
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (T)method.Invoke(null, args);
        }

        private static T Read<T>(object value, string name)
        {
            return (T)value.GetType().GetProperty(name).GetValue(value);
        }

        private static (float x, float y) InvokePoint(
            float firstX,
            float firstY,
            float step,
            int index)
        {
            var assembly = Load();
            var pointType = assembly.GetType(
                "ResearchAndTradeOptimization.Core.NodePosition",
                true);
            var rules = assembly.GetType(
                "ResearchAndTradeOptimization.Core.ResearchQueueLayoutRules",
                true);
            var point = Activator.CreateInstance(
                pointType,
                new object[] { firstX, firstY });
            var method = rules.GetMethod(
                "GetRowPosition",
                BindingFlags.Static | BindingFlags.NonPublic);
            var result = method.Invoke(null, new[] { point, (object)step, index });
            return (
                (float)pointType.GetProperty("X").GetValue(result),
                (float)pointType.GetProperty("Y").GetValue(result));
        }

        private static Assembly Load()
        {
            return Assembly.LoadFrom(Path.Combine(
                AppContext.BaseDirectory,
                "ResearchAndTradeOptimization.dll"));
        }
    }
}
