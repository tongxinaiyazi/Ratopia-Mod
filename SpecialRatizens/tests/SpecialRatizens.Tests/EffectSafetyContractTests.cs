using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SpecialRatizens.Tests
{
    public sealed class EffectSafetyContractTests
    {
        [Fact]
        public void OmegaPowerFillRejectsMissingBotOrGridBeforeReadingWattage()
        {
            var body = MethodBody("static bool FillUpGbotPower(GBot bot)");
            var botGuard = body.IndexOf("bot == null", StringComparison.Ordinal);
            var gridGuard = body.IndexOf("SuperElecLine == null", StringComparison.Ordinal);
            var wattRead = body.IndexOf("SuperElecLine.m_Watt", StringComparison.Ordinal);

            Assert.True(botGuard >= 0);
            Assert.True(gridGuard >= 0);
            Assert.True(wattRead >= 0);
            Assert.True(botGuard < wattRead);
            Assert.True(gridGuard < wattRead);
        }

        [Fact]
        public void InactiveOmegaDoesNotEvaluateItsDivisionFormula()
        {
            var body = MethodBody("static void AMJ7_LZJX_Effect(GBot bot = null)");

            Assert.Contains("float value = canUse ?", body);
            Assert.Contains(": 0f", body);
        }

        [Fact]
        public void PikachuChecksTheElectricalGridBeforeReadingItsFields()
        {
            var body = MethodBody("static void PKQ_SWFT_Effect(T_Citizen citizen, Building_ThermalGenerator building)");
            var lookup = body.IndexOf("SearchElecInfo", StringComparison.Ordinal);
            var nullGuard = body.IndexOf("== null", lookup, StringComparison.Ordinal);
            var wattRead = body.IndexOf(".m_Watt", lookup, StringComparison.Ordinal);

            Assert.True(lookup >= 0);
            Assert.True(nullGuard > lookup);
            Assert.True(wattRead > nullGuard);
        }

        [Fact]
        public void WorkCompletionRejectsMissingBuildingOrWorker()
        {
            var body = MethodBody("public static void MasonryInfo_WorkUpdate_Postfix(MasonryInfo __instance, ref float d_time)");

            Assert.Contains("building == null", body);
            Assert.Contains("worker == null", body);
            Assert.True(body.IndexOf("worker == null", StringComparison.Ordinal) <
                        body.IndexOf("CitizenIsSpecialUnit", StringComparison.Ordinal));
        }

        [Fact]
        public void ExportPriceRejectsMissingTradeItemBeforeLoggingItsName()
        {
            var body = MethodBody("public static bool DiplomaticCountryResourceData_TradeMyKingdomToCountryPrice(");
            var nullGuard = body.IndexOf("____info == null", StringComparison.Ordinal);
            var nameRead = body.IndexOf("____info.T_Name", StringComparison.Ordinal);

            Assert.True(nullGuard >= 0);
            Assert.True(nameRead > nullGuard);
        }

        private static string MethodBody(string signature)
        {
            var source = File.ReadAllText(Path.Combine(
                GetProjectRoot(), "src", "SpecialRatizens", "Legacy", "CustomMOD.cs"));
            var signatureIndex = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.True(signatureIndex >= 0, $"Method signature not found: {signature}");
            var openBrace = source.IndexOf('{', signatureIndex);
            Assert.True(openBrace >= 0);

            var depth = 0;
            for (var index = openBrace; index < source.Length; index++)
            {
                if (source[index] == '{') depth++;
                if (source[index] != '}') continue;
                depth--;
                if (depth == 0)
                {
                    return source.Substring(openBrace, index - openBrace + 1);
                }
            }

            throw new InvalidDataException($"Method body is not balanced: {signature}");
        }

        private static string GetProjectRoot()
        {
            return typeof(EffectSafetyContractTests).Assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "ProjectRoot")
                .Value;
        }
    }
}
