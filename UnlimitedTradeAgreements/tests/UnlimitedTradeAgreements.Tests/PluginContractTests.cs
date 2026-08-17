using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Xunit;

namespace UnlimitedTradeAgreements.Tests
{
    public sealed class PluginContractTests
    {
        [Fact]
        public void PluginIdentityAndIncompatibilityAreExact()
        {
            using (var module = LoadPlugin())
            {
                var plugin = FindType(module, "UnlimitedTradeAgreements.Plugin");
                var identity = plugin.CustomAttributes.Single(attribute =>
                    attribute.AttributeType.FullName == "BepInEx.BepInPlugin");
                Assert.Equal("cn.ratopia.unlimitedtradeagreements", identity.ConstructorArguments[0].Value);
                Assert.Equal("贸易站去除最大队列限制", identity.ConstructorArguments[1].Value);
                Assert.Equal("0.1.0", identity.ConstructorArguments[2].Value);

                var incompatibility = plugin.CustomAttributes.Single(attribute =>
                    attribute.AttributeType.FullName == "BepInEx.BepInIncompatibility");
                Assert.Equal(
                    "cn.ratopia.unlimitedresearchandtradequeues",
                    incompatibility.ConstructorArguments[0].Value);
            }
        }

        [Fact]
        public void PluginContainsAndInstallsOnlyTheThreeTradeQueuePatches()
        {
            using (var module = LoadPlugin())
            {
                var expected = new Dictionary<string, (string type, string method)>
                {
                    ["UnlimitedTradeAgreements.Patches.TradeAgreementLimitPatch"] =
                        ("CasselGames.Diplomatic.Data.DiplomaticCountryData", "IsFullTradeAgreement"),
                    ["UnlimitedTradeAgreements.Patches.TradeLayoutPatch"] =
                        ("CasselGames.Diplomatic.UI.DiplomaticTradeLayoutUI", "UpdateSlot"),
                    ["UnlimitedTradeAgreements.Patches.TradeWorldDetailPatch"] =
                        ("CasselGames.Diplomatic.UI.DiplomaticWorldDetailUI", "Refresh")
                };

                var patchTypes = module.Types
                    .Where(type => type.CustomAttributes.Any(attribute =>
                        attribute.AttributeType.FullName == "HarmonyLib.HarmonyPatch"))
                    .ToArray();
                Assert.Equal(expected.Keys.OrderBy(item => item),
                    patchTypes.Select(type => type.FullName).OrderBy(item => item));

                foreach (var patchType in patchTypes)
                {
                    var attribute = patchType.CustomAttributes.Single(item =>
                        item.AttributeType.FullName == "HarmonyLib.HarmonyPatch");
                    var target = expected[patchType.FullName];
                    Assert.Equal(target.type,
                        ((TypeReference)attribute.ConstructorArguments[0].Value).FullName);
                    Assert.Equal(target.method, attribute.ConstructorArguments[1].Value);
                }

                var initializer = FindType(module, "UnlimitedTradeAgreements.Plugin")
                    .Methods.Single(method => method.Name == ".cctor");
                var installed = initializer.Body.Instructions
                    .Select(instruction => instruction.Operand as TypeReference)
                    .Where(type => type != null && expected.ContainsKey(type.FullName))
                    .Select(type => type.FullName)
                    .Distinct()
                    .OrderBy(item => item)
                    .ToArray();
                Assert.Equal(expected.Keys.OrderBy(item => item), installed);
            }
        }

        [Fact]
        public void LaterResearchAndTradeOptimizationFeaturesAreAbsent()
        {
            using (var module = LoadPlugin())
            {
                var forbidden = new[]
                {
                    "ResearchQueue",
                    "FullTradeResource",
                    "TradeResourcePreview",
                    "TradeAgreementEdit",
                    "QuarterlyTradePrice"
                };
                Assert.DoesNotContain(module.Types, type =>
                    forbidden.Any(fragment => type.FullName.Contains(fragment)));
                Assert.DoesNotContain(module.Types, type =>
                    type.FullName.IndexOf("Config", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    type.FullName.IndexOf("Save", StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        private static ModuleDefinition LoadPlugin()
        {
            return ModuleDefinition.ReadModule(TestPaths.RequireFile(TestPaths.PluginAssembly));
        }

        private static TypeDefinition FindType(ModuleDefinition module, string fullName)
        {
            var type = module.Types.SingleOrDefault(item => item.FullName == fullName);
            Assert.NotNull(type);
            return type;
        }
    }
}
