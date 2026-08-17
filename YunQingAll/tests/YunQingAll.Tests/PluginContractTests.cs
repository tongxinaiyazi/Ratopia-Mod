using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using RatopiaMod.YunQing.All.Core;
using Xunit;

namespace RatopiaMod.YunQing.All.Tests
{
    public sealed class PluginContractTests
    {
        private const string PluginTypeName = "RatopiaMod.YunQing.All.Plugin";

        private static readonly IReadOnlyDictionary<string, string> RequiredPatchMethods =
            new Dictionary<string, string>
            {
                ["RatopiaMod.YunQing.All.Patches.FishDrownCheckPatch"] = "Prefix",
                ["RatopiaMod.YunQing.All.Patches.MonkfishDrownCheckPatch"] = "Prefix",
                ["RatopiaMod.YunQing.All.Patches.ExchangeTicketPatch"] = "Postfix",
                ["RatopiaMod.YunQing.All.Patches.BankExchangePatch"] = "Postfix"
            };

        [Fact]
        public void PluginMetadataAndAssemblyVersionAreStable()
        {
            using (var module = OpenPluginModule())
            {
                var plugin = FindType(module, PluginTypeName);
                var attribute = plugin.CustomAttributes.Single(item =>
                    item.AttributeType.FullName == "BepInEx.BepInPlugin");

                Assert.Equal("RatopiaMod.YunQing.YunQingAll", attribute.ConstructorArguments[0].Value);
                Assert.Equal("YunQingAll", attribute.ConstructorArguments[1].Value);
                Assert.Equal("2.2.0", attribute.ConstructorArguments[2].Value);
                Assert.Equal(new Version(2, 2, 0, 0), module.Assembly.Name.Version);
            }
        }

        [Fact]
        public void ConfigurationSchemaAndDefaultsRemainCompatible()
        {
            using (var module = OpenPluginModule())
            {
                var plugin = FindType(module, PluginTypeName);

                AssertConstant(plugin, "CommonConfigSection", "Common");
                AssertConstant(plugin, "GuiConfigSection", "GUI");
                AssertConstant(plugin, "FishConfigKey", "IsActiveFishDrownInTheWater");
                AssertConstant(plugin, "ExchangeModeConfigKey", "CustomExchangeRateMode");
                AssertConstant(plugin, "BankMultiplierConfigKey", "BankExchangeMultiplier");
                AssertConstant(plugin, "GuiToggleKeyConfigKey", "GuiToggleKey");
                AssertConstant(plugin, "DefaultFishFeatureEnabled", true);
                AssertConstant(plugin, "DefaultExchangeRateMode", 2);
                AssertConstant(plugin, "DefaultBankExchangeMultiplier", 1);
                AssertConstant(plugin, "DefaultGuiToggleKeyCode", 290);
            }
        }

        [Fact]
        public void PluginContainsExpectedLifecycleAndExactlyFourDiagnosticPatchTypes()
        {
            using (var module = OpenPluginModule())
            {
                var plugin = FindType(module, PluginTypeName);
                foreach (var methodName in new[] { "Awake", "Update", "OnGUI", "OnDestroy" })
                {
                    Assert.Single(plugin.Methods, method => method.Name == methodName);
                }

                var patchTypes = module.Types
                    .Where(type => type.CustomAttributes.Any(attribute =>
                        attribute.AttributeType.FullName == "HarmonyLib.HarmonyPatch"))
                    .ToArray();

                Assert.Equal(RequiredPatchMethods.Keys.OrderBy(name => name),
                    patchTypes.Select(type => type.FullName).OrderBy(name => name));
            }
        }

        [Fact]
        public void EveryRuntimePatchContainsAnExceptionFallback()
        {
            using (var module = OpenPluginModule())
            {
                foreach (var expected in RequiredPatchMethods)
                {
                    var patchType = FindType(module, expected.Key);
                    var patchMethod = patchType.Methods.Single(method => method.Name == expected.Value);

                    Assert.Contains(patchMethod.Body.ExceptionHandlers, handler =>
                        handler.HandlerType == ExceptionHandlerType.Catch
                        && handler.CatchType.FullName == "System.Exception");
                }
            }
        }

        [Fact]
        public void PatchInstallationUsesInfoLevelEvidenceLogs()
        {
            using (var module = OpenPluginModule())
            {
                var plugin = FindType(module, PluginTypeName);
                var installer = plugin.Methods.Single(method => method.Name == "PatchAllWithDiagnostics");
                var infoLogCalls = installer.Body.Instructions.Count(instruction =>
                    instruction.Operand is MethodReference method
                    && method.Name == "LogInfo"
                    && method.DeclaringType.FullName == "BepInEx.Logging.ManualLogSource");

                Assert.True(infoLogCalls >= 2, "Patch installation start and completion must be visible at Info level.");
            }
        }

        [Fact]
        public void EveryRuntimePatchRecordsFirstInvocationEvidence()
        {
            using (var module = OpenPluginModule())
            {
                foreach (var expected in RequiredPatchMethods)
                {
                    var patchType = FindType(module, expected.Key);
                    var patchMethod = patchType.Methods.Single(method => method.Name == expected.Value);
                    Assert.Contains(patchMethod.Body.Instructions, instruction =>
                        instruction.Operand is MethodReference method
                        && method.Name == "LogPatchInvocationOnce"
                        && method.DeclaringType.FullName == PluginTypeName);
                }
            }
        }

        [Fact]
        public void PluginReferencesExactBepInEx5AndHarmonyWithoutForbiddenBuildLibraries()
        {
            using (var module = OpenPluginModule())
            {
                var bepInEx = module.AssemblyReferences.Single(reference => reference.Name == "BepInEx");
                var harmony = module.AssemblyReferences.Single(reference => reference.Name == "0Harmony");
                Assert.Equal(new Version(5, 4, 23, 5), bepInEx.Version);
                Assert.Equal(new Version(2, 9, 0, 0), harmony.Version);

                var forbidden = new[]
                {
                    "Mono.Cecil",
                    "xunit.core",
                    "Microsoft.NET.Test.Sdk",
                    "BepInEx.Unity.Mono"
                };
                Assert.DoesNotContain(module.AssemblyReferences,
                    reference => forbidden.Contains(reference.Name, StringComparer.OrdinalIgnoreCase));
                Assert.DoesNotContain(module.Types, type => type.Name == "CheatPanelLocalizer");
            }
        }

        private static ModuleDefinition OpenPluginModule()
        {
            return ModuleDefinition.ReadModule(typeof(ExchangeTicketSelector).Assembly.Location);
        }

        private static TypeDefinition FindType(ModuleDefinition module, string fullName)
        {
            return module.Types.Single(type => type.FullName == fullName);
        }

        private static void AssertConstant(TypeDefinition type, string fieldName, object expected)
        {
            var field = type.Fields.Single(item => item.Name == fieldName);
            Assert.True(field.HasConstant);
            Assert.Equal(expected, field.Constant);
        }
    }
}
