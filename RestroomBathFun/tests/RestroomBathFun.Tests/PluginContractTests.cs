using System;
using System.Linq;
using Mono.Cecil;
using Xunit;

namespace RestroomBathFun.Tests
{
    public sealed class PluginContractTests
    {
        [Fact]
        public void PluginIdentityAndLifecycleMatchTheReleaseContract()
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.PluginAssembly))
            {
                var plugin = module.Types.Single(type => type.FullName == "RestroomBathFun.Plugin");
                Assert.Equal("BepInEx.BaseUnityPlugin", plugin.BaseType.FullName);

                var metadata = plugin.CustomAttributes.Single(attribute =>
                    attribute.AttributeType.FullName == "BepInEx.BepInPlugin");
                Assert.Equal("cn.ratopia.restroombathfun", metadata.ConstructorArguments[0].Value);
                Assert.Equal("卫生间澡堂加乐趣", metadata.ConstructorArguments[1].Value);
                Assert.Equal("1.0.0", metadata.ConstructorArguments[2].Value);

                var awake = plugin.Methods.Single(method => method.Name == "Awake");
                var onDestroy = plugin.Methods.Single(method => method.Name == "OnDestroy");
                AssertCalls(awake, "BepInEx.Configuration.ConfigFile", "Bind");
                AssertCalls(awake, "HarmonyLib.PatchClassProcessor", "Patch");
                AssertCalls(onDestroy, "HarmonyLib.Harmony", "UnpatchSelf");
            }
        }

        [Fact]
        public void CompletionPatchCapturesAbortStateAndAppliesRewardAfterward()
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.PluginAssembly))
            {
                var patch = module.Types.Single(type =>
                    type.FullName == "RestroomBathFun.Patches.ServiceCompletionPatch");
                Assert.Contains(patch.CustomAttributes, attribute =>
                    attribute.AttributeType.FullName == "HarmonyLib.HarmonyPatch");

                var prefix = patch.Methods.Single(method => method.Name == "Prefix");
                var postfix = patch.Methods.Single(method => method.Name == "Postfix");
                Assert.Contains(prefix.CustomAttributes, attribute =>
                    attribute.AttributeType.FullName == "HarmonyLib.HarmonyPrefix");
                Assert.Contains(postfix.CustomAttributes, attribute =>
                    attribute.AttributeType.FullName == "HarmonyLib.HarmonyPostfix");
                AssertCalls(prefix, "GameUnit", "get_ServiceAborted");
                AssertCalls(postfix, "RestroomBathFun.Runtime.FunRewardRuntime", "ApplySafely");
            }
        }

        [Fact]
        public void RuntimeUsesTheOriginalFunUpdateApi()
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.PluginAssembly))
            {
                var runtime = module.Types.Single(type =>
                    type.FullName == "RestroomBathFun.Runtime.FunRewardRuntime");
                var apply = runtime.Methods.Single(method => method.Name == "ApplySafely");
                Assert.Contains(apply.Body.Instructions, instruction =>
                    instruction.Operand is MethodReference called &&
                    called.Name == "FunUpdate" &&
                    called.Parameters.Select(parameter => parameter.ParameterType.FullName)
                        .SequenceEqual(new[] { "System.Single" }));
                Assert.DoesNotContain(apply.Body.Instructions, instruction =>
                    instruction.OpCode.Code == Mono.Cecil.Cil.Code.Stfld &&
                    instruction.Operand is FieldReference field &&
                    field.Name == "m_Fun");
            }
        }

        [Fact]
        public void ConfigurationIsFileOnlyAndUsesThePlannedBounds()
        {
            var source = ContractTestPaths.ReadProjectFile(
                "src", "RestroomBathFun", "Plugin.cs");

            Assert.Contains("\"Rewards\", \"ToiletFunReward\", 25f", source);
            Assert.Contains("\"Rewards\", \"BathsFunReward\", 30f", source);
            Assert.Contains("new AcceptableValueRange<float>(0f, 100f)", source);
            Assert.DoesNotContain("OnGUI", source);
            Assert.DoesNotContain("Config.Reload", source);
            Assert.DoesNotContain("FileSystemWatcher", source);
        }

        private static void AssertCalls(
            MethodDefinition method,
            string declaringType,
            string methodName)
        {
            Assert.Contains(method.Body.Instructions, instruction =>
                instruction.Operand is MethodReference called &&
                called.DeclaringType.FullName == declaringType &&
                called.Name == methodName);
        }
    }
}
