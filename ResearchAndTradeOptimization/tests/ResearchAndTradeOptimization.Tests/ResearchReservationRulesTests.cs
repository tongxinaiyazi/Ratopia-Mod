using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace ResearchAndTradeOptimization.Tests
{
    public sealed class ResearchReservationRulesTests
    {
        [Fact]
        public void NewReservationsUseThePersistentUnpaidMarker()
        {
            Assert.Equal(int.MinValue, Invoke<int>("GetUnpaidStartTime"));
            Assert.True(Invoke<bool>("IsUnpaid", int.MinValue));
            Assert.False(Invoke<bool>("IsUnpaid", 0));
            Assert.False(Invoke<bool>("IsUnpaid", 12345));
        }

        [Theory]
        [InlineData(0, 10, false)]
        [InlineData(9, 10, false)]
        [InlineData(10, 10, true)]
        [InlineData(11, 10, true)]
        public void UnpaidHeadStartsOnlyWhenTheFullCostIsAvailable(
            int available,
            int cost,
            bool expected)
        {
            Assert.Equal(expected, Invoke<bool>(
                "CanStartUnpaidHead",
                available,
                cost));
        }

        [Theory]
        [InlineData(0, 100, 10, false)]
        [InlineData(0, 9, 10, true)]
        [InlineData(1, 100, 10, true)]
        public void AnnouncementUsesReservedForWaitingOrNonHeadItems(
            int queueCount,
            int available,
            int cost,
            bool expectedReserved)
        {
            Assert.Equal(expectedReserved, Invoke<bool>(
                "ShouldAnnounceReservation",
                queueCount,
                available,
                cost));
        }

        [Fact]
        public void CancellationRefundsOnlyPaidItems()
        {
            Assert.False(Invoke<bool>("ShouldRefund", int.MinValue));
            Assert.True(Invoke<bool>("ShouldRefund", 0));
            Assert.True(Invoke<bool>("ShouldRefund", 12345));
        }

        private static T Invoke<T>(string methodName, params object[] arguments)
        {
            var assemblyPath = Path.Combine(
                AppContext.BaseDirectory,
                "ResearchAndTradeOptimization.dll");
            var assembly = Assembly.LoadFrom(assemblyPath);
            var rules = assembly.GetType(
                "ResearchAndTradeOptimization.Core.ResearchReservationRules",
                throwOnError: false);
            Assert.NotNull(rules);
            var method = rules.GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (T)method.Invoke(null, arguments);
        }
    }
}
