using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace ResearchAndTradeOptimization.Tests
{
    public sealed class TradeAgreementRulesTests
    {
        [Theory]
        [InlineData(100, 1, true)]
        [InlineData(100, 10, true)]
        [InlineData(100, 17, true)]
        [InlineData(100, 0, false)]
        [InlineData(100, 2, false)]
        [InlineData(100, 3, false)]
        [InlineData(100, 18, false)]
        [InlineData(4001, 1, false)]
        [InlineData(4001, 12, false)]
        public void EditableAgreementsAreActiveOrdinaryGoodsOnly(
            int resource,
            int state,
            bool expected)
        {
            Assert.Equal(expected, Invoke<bool>("IsEditableAgreement", resource, state));
        }

        [Theory]
        [InlineData(7, 7, 3, true)]
        [InlineData(7, 3, 3, true)]
        [InlineData(7, 4, 3, false)]
        [InlineData(2, 0, 3, false)]
        [InlineData(2, -1, 3, false)]
        public void CountValidationPreservesAnUnchangedLegacyValueButConstrainsEdits(
            int original,
            int requested,
            int currentMaximum,
            bool expected)
        {
            Assert.Equal(
                expected,
                Invoke<bool>("IsCountValid", original, requested, currentMaximum));
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        [InlineData(2, 2)]
        [InlineData(10, 6)]
        public void CountMaximumMatchesVanillaProsperityRule(
            int prosperity,
            int expected)
        {
            Assert.Equal(expected, Invoke<int>("GetCurrentMaximumCount", prosperity));
        }

        [Theory]
        [InlineData(0, 10, false)]
        [InlineData(1, 10, false)]
        [InlineData(9, 10, false)]
        [InlineData(10, 10, true)]
        [InlineData(20, 10, true)]
        [InlineData(21, 10, false)]
        [InlineData(10, 0, false)]
        public void QuarterBoundaryExcludesDayZeroAndUsesVanillaLongestTerm(
            int totalDays,
            int dayOfQuarter,
            bool expected)
        {
            Assert.Equal(
                expected,
                Invoke<bool>("IsQuarterBoundary", totalDays, dayOfQuarter));
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(1, false)]
        [InlineData(10, false)]
        public void ZeroPeriodMeansInfinite(int period, bool expected)
        {
            Assert.Equal(expected, Invoke<bool>("IsInfinitePeriod", period));
        }

        [Theory]
        [InlineData(true, 1, 0)]
        [InlineData(true, 0, 0)]
        [InlineData(false, 1, 1)]
        [InlineData(false, 0, 0)]
        public void OrdinaryPeriodAllowsZeroInEveryEditorSession(
            bool ordinaryPeriod,
            int vanillaMinimum,
            int expected)
        {
            Assert.Equal(
                expected,
                Invoke<int>("GetPeriodMinimum", ordinaryPeriod, vanillaMinimum));
        }

        [Theory]
        [InlineData(false, 0, true)]
        [InlineData(false, 3, true)]
        [InlineData(true, 0, false)]
        [InlineData(true, 3, false)]
        [InlineData(true, 1, true)]
        [InlineData(true, 2, true)]
        [InlineData(true, 127, true)]
        public void SheetRowsAreFullyRestoredOutsideEditMode(
            bool editing,
            int rowType,
            bool expected)
        {
            Assert.Equal(
                expected,
                Invoke<bool>("IsSheetRowInteractable", editing, rowType));
        }

        private static T Invoke<T>(string methodName, params object[] arguments)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "ResearchAndTradeOptimization.dll");
            Assert.True(File.Exists(path), $"Plugin assembly not found: {path}");
            var type = Assembly.LoadFrom(path).GetType(
                "ResearchAndTradeOptimization.Core.TradeAgreementRules",
                throwOnError: true);
            var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (T)method.Invoke(null, arguments);
        }
    }
}
