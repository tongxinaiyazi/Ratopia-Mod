using PopulationCustomizer.Core;
using Xunit;

namespace PopulationCustomizer.Tests
{
    public sealed class LimitRulesAndSettingsCodecTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(999)]
        public void Resolve_ReturnsEnabledValidCustomLimit(int customLimit)
        {
            Assert.Equal(customLimit, LimitRules.Resolve(75, true, customLimit));
        }

        [Fact]
        public void Resolve_ReturnsVanillaLimitWhenCustomLimitIsDisabled()
        {
            Assert.Equal(75, LimitRules.Resolve(75, false, 0));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(1000)]
        public void Resolve_ReturnsVanillaLimitWhenEnabledCustomLimitIsInvalid(int customLimit)
        {
            Assert.Equal(75, LimitRules.Resolve(75, true, customLimit));
        }

        [Theory]
        [InlineData("0", 0)]
        [InlineData("999", 999)]
        public void TryParse_AcceptsDecimalValuesInRange(string text, int expected)
        {
            Assert.True(LimitRules.TryParse(text, out var actual));
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("-1")]
        [InlineData("1000")]
        [InlineData("abc")]
        [InlineData("1.5")]
        public void TryParse_RejectsMissingOrInvalidValues(string text)
        {
            Assert.False(LimitRules.TryParse(text, out _));
        }

        [Fact]
        public void Vanilla_DisablesBothCustomLimits()
        {
            var settings = LimitSettings.Vanilla;

            Assert.False(settings.CitizenEnabled);
            Assert.Equal(0, settings.CitizenLimit);
            Assert.False(settings.RatronEnabled);
            Assert.Equal(0, settings.RatronLimit);
        }

        [Fact]
        public void Serialize_UsesExactVersionOneWireFormat()
        {
            var settings = new LimitSettings(true, 999, false, 0);

            Assert.Equal("v1|1|999|0|0", SettingsCodec.Serialize(settings));
        }

        [Fact]
        public void TryDeserialize_ReadsValidVersionOneWireFormat()
        {
            Assert.True(SettingsCodec.TryDeserialize("v1|1|0|1|999", out var settings));
            Assert.True(settings.CitizenEnabled);
            Assert.Equal(0, settings.CitizenLimit);
            Assert.True(settings.RatronEnabled);
            Assert.Equal(999, settings.RatronLimit);
        }

        [Theory]
        [InlineData("v2|1|50|1|60")]
        [InlineData("v1|1|50|1")]
        [InlineData("v1|2|50|1|60")]
        [InlineData("v1|1|1000|1|60")]
        public void TryDeserialize_RejectsMalformedDataAndReturnsVanillaSettings(string text)
        {
            Assert.False(SettingsCodec.TryDeserialize(text, out var settings));
            Assert.False(settings.CitizenEnabled);
            Assert.Equal(0, settings.CitizenLimit);
            Assert.False(settings.RatronEnabled);
            Assert.Equal(0, settings.RatronLimit);
        }
    }
}
