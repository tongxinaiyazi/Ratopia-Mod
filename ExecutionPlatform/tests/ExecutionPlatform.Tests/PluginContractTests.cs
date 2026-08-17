using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace ExecutionPlatform.Tests
{
    public sealed class PluginContractTests
    {
        [Fact]
        public void PluginMetadataAndPatchSurfaceArePresent()
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.PluginAssembly))
            {
                var plugin = FindType(module, "ExecutionPlatform.Plugin");
                var attribute = plugin.CustomAttributes.Single(item =>
                    item.AttributeType.FullName == "BepInEx.BepInPlugin");
                Assert.Equal("cn.ratopia.executionplatform", attribute.ConstructorArguments[0].Value);
                Assert.Equal("处刑台", attribute.ConstructorArguments[1].Value);
                Assert.Equal("0.1.1", attribute.ConstructorArguments[2].Value);

                var expectedPatchTypes = new[]
                {
                    "ExecutionPlatform.Patches.BuildDatabasePatch",
                    "ExecutionPlatform.Patches.UnlockBuildingPatch",
                    "ExecutionPlatform.Patches.SpriteLookupPatch",
                    "ExecutionPlatform.Patches.MagicianBuildingPatch",
                    "ExecutionPlatform.Patches.BuildSetPatch",
                    "ExecutionPlatform.Patches.AddToPoolPatch",
                    "ExecutionPlatform.Patches.CitizenJobPatch",
                    "ExecutionPlatform.Patches.CitizenJobFirePatch",
                    "ExecutionPlatform.Patches.CitizenUpdatePatch",
                    "ExecutionPlatform.Patches.BeforeLoadPatch"
                };
                foreach (var typeName in expectedPatchTypes)
                {
                    Assert.NotNull(FindType(module, typeName));
                }

                var updatePatch = FindType(module, "ExecutionPlatform.Patches.CitizenUpdatePatch");
                var priority = updatePatch.CustomAttributes.Single(item =>
                    item.AttributeType.FullName == "HarmonyLib.HarmonyPriority");
                Assert.Equal(800, priority.ConstructorArguments[0].Value);
            }
        }

        [Fact]
        public void ExecutePathRemovesTransientStateBeforeVanillaDeathCalls()
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.PluginAssembly))
            {
                var runtime = FindType(module, "ExecutionPlatform.Runtime.ExecutionRuntime");
                var execute = runtime.Methods.Single(method => method.Name == "Execute");
                var calls = execute.Body.Instructions
                    .Where(instruction => instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
                    .Select(instruction => (MethodReference)instruction.Operand)
                    .ToList();

                var removeIndex = IndexOfCall(calls, "System.Collections.Generic.Dictionary`2<T_Citizen,ExecutionPlatform.Core.ExecutionStateMachine>", "Remove");
                var hpIndex = IndexOfCall(calls, "GameUnit", "HpUpdate");
                var deathIndex = IndexOfCall(calls, "GameUnit", "DeathCheck");
                Assert.True(removeIndex >= 0);
                Assert.True(hpIndex > removeIndex);
                Assert.True(deathIndex > hpIndex);
            }
        }

        [Fact]
        public void PoolFailurePathCanReturnTheCheckedOutObject()
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.PluginAssembly))
            {
                var bridge = FindType(module, "ExecutionPlatform.Runtime.ExecutionPoolBridge");
                var calls = bridge.Methods
                    .Where(method => method.HasBody)
                    .SelectMany(method => method.Body.Instructions)
                    .Where(instruction => instruction.Operand is MethodReference)
                    .Select(instruction => (MethodReference)instruction.Operand);

                Assert.Contains(calls, call =>
                    call.DeclaringType.FullName == "MemoryPool" && call.Name == "AddObj");
                Assert.Contains(calls, call =>
                    call.DeclaringType.FullName == "Building" && call.Name == "BuildingDemolition");
            }
        }

        [Fact]
        public void InvalidAssignmentPathCanUndoTheVanillaJob()
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.PluginAssembly))
            {
                var runtime = FindType(module, "ExecutionPlatform.Runtime.ExecutionRuntime");
                var onJobSet = runtime.Methods.Single(method => method.Name == "OnJobSet");
                var calls = onJobSet.Body.Instructions
                    .Where(instruction => instruction.Operand is MethodReference)
                    .Select(instruction => (MethodReference)instruction.Operand)
                    .ToList();

                Assert.Contains(calls, call => call.DeclaringType.FullName == "T_Citizen" && call.Name == "JobFire");
                Assert.Contains(calls, call =>
                    call.DeclaringType.FullName == "ExecutionPlatform.Runtime.ExecutionRuntime" &&
                    call.Name == "IsEligibleCitizen");
            }
        }

        private static int IndexOfCall(IReadOnlyList<MethodReference> calls, string declaringType, string name)
        {
            for (var index = 0; index < calls.Count; index++)
            {
                if (calls[index].DeclaringType.FullName == declaringType && calls[index].Name == name)
                {
                    return index;
                }
            }

            return -1;
        }

        private static TypeDefinition FindType(ModuleDefinition module, string fullName)
        {
            return module.Types.SingleOrDefault(type => type.FullName == fullName);
        }
    }
}
