using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace ResearchAndTradeOptimization.Tests
{
    public sealed class CoreRulesTests
    {
        [Fact]
        public void ResearchLimitFallsBackToVanillaWhenVisibleCapacityIsUnavailable()
        {
            Assert.Equal(3, Invoke<int>("GetResearchLimit", false));
        }

        [Fact]
        public void ResearchLimitIsEffectivelyUnlimitedWhenVisibleCapacityIsReady()
        {
            Assert.Equal(int.MaxValue, Invoke<int>("GetResearchLimit", true));
        }

        [Theory]
        [InlineData(0, 7)]
        [InlineData(6, 7)]
        [InlineData(7, 7)]
        [InlineData(8, 8)]
        [InlineData(32, 32)]
        public void TradeDisplayKeepsSevenSlotsAndExpandsToTheActualCount(int goodsCount, int expected)
        {
            Assert.Equal(expected, Invoke<int>("GetTradeDisplaySlotCount", goodsCount));
        }

        [Fact]
        public void UnlimitedCountLabelUsesTheInfinitySymbol()
        {
            Assert.Equal("12/∞", Invoke<string>("GetUnlimitedCountLabel", 12));
        }

        [Fact]
        public void NextNodePositionContinuesTheObservedSpacing()
        {
            var result = InvokePoint(100f, 10f, 200f, 15f);
            Assert.Equal(300f, result.x);
            Assert.Equal(20f, result.y);
        }

        [Fact]
        public void NextNodePositionUsesHorizontalFallbackWhenSpacingIsZero()
        {
            var result = InvokePoint(200f, 15f, 200f, 15f);
            Assert.Equal(300f, result.x);
            Assert.Equal(15f, result.y);
        }

        private static T Invoke<T>(string methodName, params object[] arguments)
        {
            var rules = LoadPluginAssembly().GetType(
                "ResearchAndTradeOptimization.Core.QueueRules",
                throwOnError: false);
            Assert.NotNull(rules);
            var method = rules.GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (T)method.Invoke(null, arguments);
        }

        private static (float x, float y) InvokePoint(
            float previousX,
            float previousY,
            float currentX,
            float currentY)
        {
            var assembly = LoadPluginAssembly();
            var pointType = assembly.GetType(
                "ResearchAndTradeOptimization.Core.NodePosition",
                throwOnError: false);
            var rules = assembly.GetType(
                "ResearchAndTradeOptimization.Core.QueueRules",
                throwOnError: false);
            Assert.NotNull(pointType);
            Assert.NotNull(rules);

            var constructor = pointType.GetConstructor(new[] { typeof(float), typeof(float) });
            Assert.NotNull(constructor);
            var previous = constructor.Invoke(new object[] { previousX, previousY });
            var current = constructor.Invoke(new object[] { currentX, currentY });
            var method = rules.GetMethod(
                "GetNextNodePosition",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(method);
            var result = method.Invoke(null, new[] { previous, current });
            var x = (float)pointType.GetProperty("X").GetValue(result);
            var y = (float)pointType.GetProperty("Y").GetValue(result);
            return (x, y);
        }

        private static Assembly LoadPluginAssembly()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "ResearchAndTradeOptimization.dll");
            Assert.True(File.Exists(path), $"Plugin assembly not found: {path}");
            return Assembly.LoadFrom(path);
        }
    }
}
