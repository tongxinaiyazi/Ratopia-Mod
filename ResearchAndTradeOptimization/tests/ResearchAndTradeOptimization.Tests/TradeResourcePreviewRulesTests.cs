using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace ResearchAndTradeOptimization.Tests
{
    public sealed class TradeResourcePreviewRulesTests
    {
        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 1, 1)]
        [InlineData(6, 6, 1)]
        [InlineData(7, 7, 2)]
        [InlineData(12, 12, 2)]
        [InlineData(13, 13, 3)]
        [InlineData(17, 17, 3)]
        [InlineData(18, 18, 3)]
        [InlineData(19, 18, 3)]
        public void PreviewUsesUpToThreeRowsOfSixItems(
            int actualCount,
            int expectedVisible,
            int expectedRows)
        {
            var result = InvokePlan("CreatePlan", actualCount);
            var resultType = result.GetType();

            Assert.Equal(
                expectedVisible,
                resultType.GetProperty("VisibleCount").GetValue(result));
            Assert.Equal(
                expectedRows,
                resultType.GetProperty("VisibleRows").GetValue(result));
        }

        [Theory]
        [InlineData(12, 12, false)]
        [InlineData(13, 1, true)]
        [InlineData(1, 13, true)]
        [InlineData(18, 18, true)]
        public void EitherThirdRowUsesOneCompactModeForBothDirections(
            int importCount,
            int exportCount,
            bool expectedCompact)
        {
            var result = InvokePlan(
                "CreateDetailPlan",
                importCount,
                exportCount,
                10);

            Assert.Equal(
                expectedCompact,
                Read<bool>(result, "UseCompactGrid"));
        }

        [Fact]
        public void CompactThreeRowsFitInsideTheNativeDirectionPanel()
        {
            var result = InvokePlan("CreateDetailPlan", 14, 14, 10);

            Assert.Equal(52f, Read<float>(result, "CellWidth"));
            Assert.Equal(52f, Read<float>(result, "CellHeight"));
            Assert.Equal(2f, Read<float>(result, "HorizontalSpacing"));
            Assert.Equal(2f, Read<float>(result, "VerticalSpacing"));
            Assert.Equal(170f, Read<float>(result, "ContentHeight"));
            Assert.Equal(6, Read<int>(result, "Columns"));
        }

        private static object InvokePlan(string methodName, params object[] arguments)
        {
            var path = Path.Combine(
                AppContext.BaseDirectory,
                "ResearchAndTradeOptimization.dll");
            var type = Assembly.LoadFrom(path).GetType(
                "ResearchAndTradeOptimization.Core.TradeResourcePreviewRules",
                throwOnError: false);
            Assert.NotNull(type);
            var method = type.GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return method.Invoke(null, arguments);
        }

        private static T Read<T>(object value, string propertyName)
        {
            var property = value.GetType().GetProperty(propertyName);
            Assert.NotNull(property);
            Assert.Equal(
                typeof(T),
                property.PropertyType);
            return (T)property.GetValue(value);
        }
    }
}
