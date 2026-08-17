using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace UnlimitedTradeAgreements.Tests
{
    public sealed class CoreRulesTests
    {
        [Theory]
        [InlineData(0, 7)]
        [InlineData(6, 7)]
        [InlineData(7, 7)]
        [InlineData(8, 8)]
        [InlineData(32, 32)]
        public void VisibleSlotCountKeepsSevenAndExpandsToActualCount(int count, int expected)
        {
            Assert.Equal(expected, Invoke<int>("GetVisibleSlotCount", count));
        }

        [Fact]
        public void UnlimitedLabelUsesCurrentCountAndInfinity()
        {
            Assert.Equal("12/∞", Invoke<string>("GetUnlimitedCountLabel", 12));
        }

        private static T Invoke<T>(string name, params object[] arguments)
        {
            var assembly = Assembly.LoadFrom(TestPaths.RequireFile(TestPaths.PluginAssembly));
            var type = assembly.GetType(
                "UnlimitedTradeAgreements.Core.TradeQueueRules",
                throwOnError: false);
            Assert.NotNull(type);
            var method = type.GetMethod(
                name,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (T)method.Invoke(null, arguments);
        }
    }
}
