using RatopiaMod.YunQing.All.Core;
using Xunit;

namespace RatopiaMod.YunQing.All.Tests
{
    public sealed class BankExchangeRulesTests
    {
        [Theory]
        [InlineData(1, 100f)]
        [InlineData(10, 1000f)]
        [InlineData(100, 10000f)]
        [InlineData(500, 50000f)]
        public void ConfiguredMultiplierScalesTheOriginalValue(
            int multiplier,
            float expected)
        {
            Assert.Equal(expected, BankExchangeRules.Apply(100f, (BankExchangeMultiplier)multiplier));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void InvalidNonPositiveMultiplierLeavesOriginalValueUntouched(int multiplier)
        {
            Assert.Equal(100f, BankExchangeRules.Apply(100f, (BankExchangeMultiplier)multiplier));
        }
    }
}
