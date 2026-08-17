namespace RatopiaMod.YunQing.All.Core
{
    internal static class BankExchangeRules
    {
        internal static float Apply(float originalValue, BankExchangeMultiplier multiplier)
        {
            var numericMultiplier = (int)multiplier;
            return numericMultiplier >= 1
                ? originalValue * numericMultiplier
                : originalValue;
        }
    }
}
