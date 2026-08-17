using System;
using System.Linq;
using Mono.Cecil;
using Xunit;

namespace SleepAcceleration.Tests
{
    public sealed class PluginContractTests
    {
        [Fact]
        public void PluginIdentityAndLifecycleMatchTheReleaseContract()
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.PluginAssembly))
            {
                var plugin = module.Types.Single(type => type.FullName == "SleepAcceleration.Plugin");
                Assert.Equal("BepInEx.BaseUnityPlugin", plugin.BaseType.FullName);

                var metadata = plugin.CustomAttributes.Single(attribute =>
                    attribute.AttributeType.FullName == "BepInEx.BepInPlugin");
                Assert.Equal("cn.ratopia.sleepacceleration", metadata.ConstructorArguments[0].Value);
                Assert.Equal("睡觉加速", metadata.ConstructorArguments[1].Value);
                Assert.Equal("0.1.0", metadata.ConstructorArguments[2].Value);

                var awake = plugin.Methods.Single(method => method.Name == "Awake");
                var onDestroy = plugin.Methods.Single(method => method.Name == "OnDestroy");
                Assert.DoesNotContain(plugin.Methods, method => method.Name == "Update");
                AssertCalls(awake, "UnityEngine.Object", "DontDestroyOnLoad");
                AssertCalls(awake, "HarmonyLib.PatchClassProcessor", "Patch");
                AssertCalls(onDestroy, "SleepAcceleration.Plugin", "TryShutdownRuntime");
                AssertCalls(onDestroy, "HarmonyLib.Harmony", "UnpatchSelf");

                var shutdown = plugin.Methods.Single(method => method.Name == "TryShutdownRuntime");
                AssertCalls(shutdown, "SleepAcceleration.Runtime.SleepAccelerationRuntime", "Shutdown");
            }
        }

        [Fact]
        public void QueenUpdatePatchUsesTheStableGameLoopTarget()
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.PluginAssembly))
            {
                var patch = module.Types.Single(type =>
                    type.FullName == "SleepAcceleration.Patches.QueenUpdatePatch");
                AssertHarmonyPatch(patch, "T_Queen", "Update");

                var postfix = patch.Methods.Single(method => method.Name == "Postfix");
                AssertCalls(postfix, "SleepAcceleration.Runtime.SleepAccelerationRuntime", "TickSafely");
            }
        }

        [Fact]
        public void UserSpeedPatchObservesOnlyThePersistentPlayerSpeedEntryPoint()
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.PluginAssembly))
            {
                var patch = module.Types.Single(type =>
                    type.FullName == "SleepAcceleration.Patches.UserSpeedChangePatch");
                AssertHarmonyPatch(patch, "SystemMgr", "ApplyUserGameSpeed");

                var postfix = patch.Methods.Single(method => method.Name == "Postfix");
                AssertCalls(
                    postfix,
                    "SleepAcceleration.Runtime.SleepAccelerationRuntime",
                    "NotifyUserSpeedChangedSafely");
            }
        }

        [Fact]
        public void RuntimeUsesUnscaledTimeOriginalPauseAndOriginalSpeedApis()
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.PluginAssembly))
            {
                var runtime = module.Types.Single(type =>
                    type.FullName == "SleepAcceleration.Runtime.SleepAccelerationRuntime");
                var tick = runtime.Methods.Single(method => method.Name == "TickSafely");
                AssertCalls(tick, "UnityEngine.Time", "get_unscaledDeltaTime");
                AssertCalls(tick, "SystemMgr", "IsGamePause");
                AssertCalls(tick, "SleepAcceleration.Core.SleepAccelerationController", "Tick");

                var resetSession = runtime.Methods.Single(method =>
                    method.Name == "ResetSessionIfPresent");
                AssertCalls(resetSession, "SleepAcceleration.Core.SleepAccelerationController", "Reset");

                var gateway = module.Types.Single(type =>
                    type.FullName == "SleepAcceleration.Runtime.RatopiaGameSpeedGateway");
                var selectedSpeed = gateway.Methods.Single(method => method.Name == "get_UserSelectedSpeed");
                var setSpeed = gateway.Methods.Single(method => method.Name == "SetTemporarySpeed");
                AssertReadsField(selectedSpeed, "PlayDataMgr", "m_UserGameSpeed");
                AssertCalls(setSpeed, "SystemMgr", "SetTimeScale");
            }
        }

        private static void AssertHarmonyPatch(
            TypeDefinition patch,
            string targetType,
            string targetMethod)
        {
            var attribute = patch.CustomAttributes.Single(item =>
                item.AttributeType.FullName == "HarmonyLib.HarmonyPatch");
            Assert.Equal(targetType, ((TypeReference)attribute.ConstructorArguments[0].Value).FullName);
            Assert.Equal(targetMethod, attribute.ConstructorArguments[1].Value);
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

        private static void AssertReadsField(
            MethodDefinition method,
            string declaringType,
            string fieldName)
        {
            Assert.Contains(method.Body.Instructions, instruction =>
                instruction.Operand is FieldReference field &&
                field.DeclaringType.FullName == declaringType &&
                field.Name == fieldName);
        }
    }
}
