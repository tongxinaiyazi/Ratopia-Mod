using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace ResearchAndTradeOptimization.Tests
{
    public sealed class TradeResourceStateRulesTests
    {
        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public void HighlightAppliesOnlyToVisibleSlotsCurrentlyInTrade(
            bool isVisibleSlot,
            bool isCurrentlyTrading,
            bool expected)
        {
            var result = InvokeRules(
                "ShouldHighlight",
                isVisibleSlot,
                isCurrentlyTrading);

            Assert.Equal(expected, (bool)result);
        }

        [Fact]
        public void ConfigConstantsMatchTheDocumentedFeature()
        {
            Assert.Equal("TradeDetailSlot", ReadConst("ConfigSection"));
            Assert.Equal("ActiveTradeBackgroundColor", ReadConst("ActiveTradeColorKey"));
            Assert.Equal("145,135,106", ReadConst("ActiveTradeColorDefault"));
            Assert.Equal("InfiniteTradeBackgroundColor", ReadConst("InfiniteTradeColorKey"));
            Assert.Equal("96,169,23", ReadConst("InfiniteTradeColorDefault"));
        }

        [Fact]
        public void DefaultColorMatchesTheDocumentedRgbValue()
        {
            var fallback = InvokeDefaultColor();
            Assert.Equal((byte)145, ReadChannel(fallback, "Red"));
            Assert.Equal((byte)135, ReadChannel(fallback, "Green"));
            Assert.Equal((byte)106, ReadChannel(fallback, "Blue"));
        }

        [Fact]
        public void DefaultInfiniteColorMatchesTheDocumentedRgbValue()
        {
            var fallback = InvokeDefaultInfiniteColor();
            Assert.Equal((byte)96, ReadChannel(fallback, "Red"));
            Assert.Equal((byte)169, ReadChannel(fallback, "Green"));
            Assert.Equal((byte)23, ReadChannel(fallback, "Blue"));
        }

        [Theory]
        [InlineData(true, true, false, 1)]
        [InlineData(true, true, true, 2)]
        [InlineData(true, false, false, 0)]
        [InlineData(true, false, true, 0)]
        [InlineData(false, true, false, 0)]
        [InlineData(false, true, true, 0)]
        [InlineData(false, false, false, 0)]
        public void HighlightKindDistinguishesInfiniteFromLimitedTrades(
            bool isVisibleSlot,
            bool isCurrentlyTrading,
            bool isInfinitePeriod,
            int expectedKind)
        {
            var result = InvokeRules(
                "GetHighlightKind",
                isVisibleSlot,
                isCurrentlyTrading,
                isInfinitePeriod);

            Assert.Equal(expectedKind, (int)result);
        }

        [Theory]
        [InlineData("145,135,106", (byte)145, (byte)135, (byte)106)]
        [InlineData("0,0,0", (byte)0, (byte)0, (byte)0)]
        [InlineData("255,255,255", (byte)255, (byte)255, (byte)255)]
        [InlineData(" 10 , 20 , 30 ", (byte)10, (byte)20, (byte)30)]
        public void ValidRgbTextIsParsedIntoChannels(
            string text,
            byte expectedRed,
            byte expectedGreen,
            byte expectedBlue)
        {
            var fallback = InvokeDefaultColor();
            var parsed = InvokeParseColorOrDefault(text, fallback);

            Assert.Equal(expectedRed, ReadChannel(parsed, "Red"));
            Assert.Equal(expectedGreen, ReadChannel(parsed, "Green"));
            Assert.Equal(expectedBlue, ReadChannel(parsed, "Blue"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("145,135")]
        [InlineData("145,135,106,255")]
        [InlineData("a,b,c")]
        [InlineData("256,0,0")]
        [InlineData("-1,0,0")]
        [InlineData("145.5,135,106")]
        public void InvalidRgbTextFallsBackToDefault(string text)
        {
            var fallback = InvokeDefaultColor();
            var parsed = InvokeParseColorOrDefault(text, fallback);

            Assert.Equal((byte)145, ReadChannel(parsed, "Red"));
            Assert.Equal((byte)135, ReadChannel(parsed, "Green"));
            Assert.Equal((byte)106, ReadChannel(parsed, "Blue"));
        }

        private static object InvokeDefaultColor()
        {
            var field = GetRulesType().GetField(
                "DefaultHighlightColor",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return field.GetValue(null);
        }

        private static object InvokeDefaultInfiniteColor()
        {
            var field = GetRulesType().GetField(
                "DefaultInfiniteHighlightColor",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return field.GetValue(null);
        }

        private static object InvokeParseColorOrDefault(string text, object fallback)
        {
            var method = GetRulesType().GetMethod(
                "ParseColorOrDefault",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return method.Invoke(null, new[] { text, fallback });
        }

        private static byte ReadChannel(object color, string channelName)
        {
            var property = color.GetType().GetProperty(channelName);
            Assert.NotNull(property);
            return (byte)property.GetValue(color, null);
        }

        private static object InvokeRules(string methodName, params object[] arguments)
        {
            var method = GetRulesType().GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return method.Invoke(null, arguments);
        }

        private static string ReadConst(string fieldName)
        {
            var field = GetRulesType().GetField(
                fieldName,
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(field);
            Assert.Equal(typeof(string), field.FieldType);
            return (string)field.GetValue(null);
        }

        private static System.Type GetRulesType()
        {
            var path = Path.Combine(
                AppContext.BaseDirectory,
                "ResearchAndTradeOptimization.dll");
            var type = Assembly.LoadFrom(path).GetType(
                "ResearchAndTradeOptimization.Core.TradeResourceStateRules",
                throwOnError: false);
            Assert.NotNull(type);
            return type;
        }
    }
}