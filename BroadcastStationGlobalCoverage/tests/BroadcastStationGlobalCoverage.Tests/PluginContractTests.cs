using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Xunit;

namespace BroadcastStationGlobalCoverage.Tests
{
    public sealed class PluginContractTests
    {
        private static readonly string[] ExpectedPatchTypes =
        {
            "BroadcastStationGlobalCoverage.Patches.TelevisionSelectionPanelPatch",
            "BroadcastStationGlobalCoverage.Patches.TelevisionAutomaticSignalPatch"
        };

        [Fact]
        public void PluginMetadataAndLifecycleMatchTheReleaseIdentity()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var plugin = module.Types.Single(type => type.FullName == "BroadcastStationGlobalCoverage.Plugin");
                Assert.Equal("BepInEx.BaseUnityPlugin", plugin.BaseType.FullName);

                var metadata = plugin.CustomAttributes.Single(attribute =>
                    attribute.AttributeType.FullName == "BepInEx.BepInPlugin");
                Assert.Equal("cn.ratopia.broadcaststationglobalcoverage", metadata.ConstructorArguments[0].Value);
                Assert.Equal("广播站信号覆盖全图", metadata.ConstructorArguments[1].Value);
                Assert.Equal("0.1.1", metadata.ConstructorArguments[2].Value);

                var awake = plugin.Methods.Single(method => method.Name == "Awake" && method.Parameters.Count == 0);
                var onDestroy = plugin.Methods.Single(method => method.Name == "OnDestroy" && method.Parameters.Count == 0);
                AssertCalls(awake, "HarmonyLib.PatchClassProcessor", "Patch");
                AssertCalls(onDestroy, "HarmonyLib.Harmony", "UnpatchSelf");
                Assert.DoesNotContain(onDestroy.Body.Instructions, instruction =>
                    instruction.Operand is MethodReference method &&
                    method.DeclaringType.FullName ==
                    "BroadcastStationGlobalCoverage.Runtime.BroadcastCoverageRuntime");
            }
        }

        [Fact]
        public void AllPlannedPatchClassesArePresent()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                foreach (var patchType in ExpectedPatchTypes)
                {
                    Assert.Contains(module.Types, type => type.FullName == patchType);
                }
            }
        }

        [Fact]
        public void PluginNeverWritesTheSharedBuildingOrDatabaseRange()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var writes = module.Types
                    .SelectMany(Flatten)
                    .SelectMany(type => type.Methods)
                    .Where(method => method.HasBody)
                    .SelectMany(method => method.Body.Instructions)
                    .Where(instruction =>
                        instruction.OpCode.Code == Mono.Cecil.Cil.Code.Stfld &&
                        instruction.Operand is FieldReference field &&
                        ((field.DeclaringType.FullName == "Building" && field.Name == "m_Range") ||
                         (field.DeclaringType.FullName == "BuildInfo" && field.Name == "Range") ||
                         (field.DeclaringType.FullName == "BuildingData" && field.Name == "m_Range")))
                    .ToArray();

                Assert.Empty(writes);
            }
        }

        [Fact]
        public void PluginDoesNotWriteTheGlobalCustomBuildingRange()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                Assert.DoesNotContain(
                    module.Types.SelectMany(type => type.Methods)
                        .Where(method => method.HasBody)
                        .SelectMany(method => method.Body.Instructions),
                    instruction =>
                        instruction.OpCode.Code == Mono.Cecil.Cil.Code.Stsfld &&
                        instruction.Operand is FieldReference field &&
                        field.DeclaringType.FullName == "Defines" &&
                        field.Name == "m_MaxCustomBuildingRange");
            }
        }

        private static string GetPluginAssemblyPath()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "BroadcastStationGlobalCoverage.dll");
            Assert.True(File.Exists(path), $"Plugin assembly not found: {path}");
            return path;
        }

        private static void AssertCalls(MethodDefinition method, string typeName, string methodName)
        {
            Assert.Contains(method.Body.Instructions, instruction =>
                instruction.Operand is MethodReference called &&
                called.DeclaringType.FullName == typeName &&
                called.Name == methodName);
        }

        private static System.Collections.Generic.IEnumerable<TypeDefinition> Flatten(TypeDefinition type)
        {
            yield return type;
            foreach (var nested in type.NestedTypes.SelectMany(Flatten))
            {
                yield return nested;
            }
        }
    }
}
