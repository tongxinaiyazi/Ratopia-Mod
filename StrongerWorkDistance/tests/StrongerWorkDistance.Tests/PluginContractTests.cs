using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Xunit;

namespace StrongerWorkDistance.Tests
{
    public sealed class PluginContractTests
    {
        [Fact]
        public void PluginAndPatchExposeThePlannedRuntimeSurface()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var plugin = module.Types.SingleOrDefault(type => type.FullName == "StrongerWorkDistance.Plugin");
                var patch = module.Types.SingleOrDefault(type =>
                    type.FullName == "StrongerWorkDistance.Patches.SystemMgrAwakePatch");
                var runtime = module.Types.SingleOrDefault(type =>
                    type.FullName == "StrongerWorkDistance.Runtime.WorkAreaRuntime");

                Assert.NotNull(plugin);
                Assert.NotNull(patch);
                Assert.NotNull(runtime);
                Assert.Equal("BepInEx.BaseUnityPlugin", plugin.BaseType.FullName);

                var metadata = plugin.CustomAttributes.Single(attribute =>
                    attribute.AttributeType.FullName == "BepInEx.BepInPlugin");
                Assert.Equal("cn.ratopia.strongerworkdistance", metadata.ConstructorArguments[0].Value);
                Assert.Equal("更强大的工作距离", metadata.ConstructorArguments[1].Value);
                Assert.Equal("0.1.0", metadata.ConstructorArguments[2].Value);

                Assert.Contains(plugin.Methods, method => method.Name == "Awake" && method.Parameters.Count == 0);
                Assert.Contains(plugin.Methods, method => method.Name == "OnDestroy" && method.Parameters.Count == 0);

                var patchMetadata = patch.CustomAttributes.Single(attribute =>
                    attribute.AttributeType.FullName == "HarmonyLib.HarmonyPatch");
                Assert.Equal("SystemMgr", ((TypeReference)patchMetadata.ConstructorArguments[0].Value).FullName);
                Assert.Equal("Awake", patchMetadata.ConstructorArguments[1].Value);
                Assert.Contains(patch.Methods, method =>
                    method.Name == "Postfix" &&
                    method.IsStatic &&
                    method.ReturnType.FullName == "System.Void" &&
                    method.Parameters.Select(parameter => parameter.ParameterType.FullName)
                        .SequenceEqual(new[] { "SystemMgr" }));

                Assert.Contains(runtime.Methods, method =>
                    method.Name == "Apply" &&
                    method.IsStatic &&
                    method.ReturnType.FullName == "System.Void" &&
                    method.Parameters.Select(parameter => parameter.ParameterType.FullName)
                        .SequenceEqual(new[] { "SystemMgr" }));
            }
        }

        [Fact]
        public void RuntimeWiringInstallsOnePatchAndAppliesBothListsSafely()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var plugin = module.Types.Single(type => type.FullName == "StrongerWorkDistance.Plugin");
                var patch = module.Types.Single(type =>
                    type.FullName == "StrongerWorkDistance.Patches.SystemMgrAwakePatch");
                var runtime = module.Types.Single(type =>
                    type.FullName == "StrongerWorkDistance.Runtime.WorkAreaRuntime");

                var awake = plugin.Methods.Single(method => method.Name == "Awake");
                var onDestroy = plugin.Methods.Single(method => method.Name == "OnDestroy");
                AssertCalls(awake, "HarmonyLib.Harmony", ".ctor");
                AssertCalls(awake, "HarmonyLib.PatchClassProcessor", "Patch");
                AssertCalls(onDestroy, "HarmonyLib.Harmony", "UnpatchSelf");

                var postfix = patch.Methods.Single(method => method.Name == "Postfix");
                AssertCalls(postfix, "StrongerWorkDistance.Runtime.WorkAreaRuntime", "Apply");
                Assert.NotEmpty(postfix.Body.ExceptionHandlers);

                var apply = runtime.Methods.Single(method => method.Name == "Apply");
                AssertCalls(apply, "StrongerWorkDistance.Core.WorkAreaRules", "CreateExpandedOffsets");
                AssertCalls(apply, "StrongerWorkDistance.Core.AtomicListUpdater", "ReplaceBoth");
                AssertReadsField(apply, "SystemMgr", "List_WM_EnableArea");
                AssertReadsField(apply, "SystemMgr", "List_BP_Ld_EnableArea");
                Assert.DoesNotContain(apply.Body.Instructions, instruction =>
                    instruction.Operand is FieldReference field &&
                    field.DeclaringType.FullName == "SystemMgr" &&
                    field.Name == "List_Queen_EnableArea");
            }
        }

        private static string GetPluginAssemblyPath()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "StrongerWorkDistance.dll");
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

        private static void AssertReadsField(MethodDefinition method, string typeName, string fieldName)
        {
            Assert.Contains(method.Body.Instructions, instruction =>
                instruction.Operand is FieldReference field &&
                field.DeclaringType.FullName == typeName &&
                field.Name == fieldName);
        }
    }
}
