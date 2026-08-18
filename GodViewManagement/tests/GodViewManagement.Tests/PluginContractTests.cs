using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace GodViewManagement.Tests
{
    public sealed class PluginContractTests
    {
        private static readonly string[] RequiredPatchTypes =
        {
            "GodViewManagement.Patches.RuntimeTickPatch",
            "GodViewManagement.Patches.QueenBtnPatch",
            "GodViewManagement.Patches.QueenBtn2Patch",
            "GodViewManagement.Patches.QueenBtn3Patch",
            "GodViewManagement.Patches.QueenBtn4Patch",
            "GodViewManagement.Patches.QueenBtn5Patch",
            "GodViewManagement.Patches.QueenUpdateInputScopePatch",
            "GodViewManagement.Patches.DirectionalInputGetKeyPatch"
        };

        [Fact]
        public void PluginMetadataIsStable()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(ManagementModeState).Assembly.Location))
            {
                var plugin = module.Types.Single(type => type.FullName == "GodViewManagement.Plugin");
                var attribute = plugin.CustomAttributes.Single(item => item.AttributeType.FullName == "BepInEx.BepInPlugin");

                Assert.Equal("cn.ratopia.godviewmanagement", attribute.ConstructorArguments[0].Value);
                Assert.Equal("上帝视角管理", attribute.ConstructorArguments[1].Value);
                Assert.Equal("0.1.3", attribute.ConstructorArguments[2].Value);
            }
        }

        [Fact]
        public void EveryRequiredPatchTypeUsesHarmonyPatchDiscovery()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(ManagementModeState).Assembly.Location))
            {
                foreach (var fullName in RequiredPatchTypes)
                {
                    var patch = module.Types.Single(type => type.FullName == fullName);
                    Assert.Contains(patch.CustomAttributes,
                        attribute => attribute.AttributeType.FullName == "HarmonyLib.HarmonyPatch");
                }
            }
        }

        [Fact]
        public void EnableStopsAnyMovementStartedBeforeGodView()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(ManagementModeState).Assembly.Location))
            {
                var controller = module.Types.Single(type =>
                    type.FullName == "GodViewManagement.Runtime.GodViewCameraController");
                var enable = controller.Methods.Single(method => method.Name == "Enable");
                var stopCalls = enable.Body.Instructions.Where(instruction =>
                    instruction.Operand is MethodReference called
                    && called.DeclaringType.FullName == "T_Queen"
                    && called.Name == "CharacterStop"
                    && !called.HasParameters);

                Assert.Single(stopCalls);
            }
        }

        [Fact]
        public void PluginOwnsAStandaloneUnityUpdateDriver()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(ManagementModeState).Assembly.Location))
            {
                var plugin = module.Types.Single(type => type.FullName == "GodViewManagement.Plugin");
                var update = plugin.Methods.Single(method =>
                    method.Name == "Update"
                    && !method.HasParameters
                    && method.ReturnType.FullName == "System.Void");

                Assert.Contains(update.Body.Instructions,
                    instruction => instruction.Operand is MethodReference called
                                   && called.DeclaringType.FullName == "GameMgr"
                                   && called.Name == "get_Instance");
                Assert.Contains(update.Body.Instructions,
                    instruction => instruction.Operand is FieldReference field
                                   && field.DeclaringType.FullName == "GameMgr"
                                   && field.Name == "_TileMgr");
                Assert.Contains(update.Body.Instructions,
                    instruction => instruction.Operand is MethodReference called
                                   && called.DeclaringType.FullName == "GodViewManagement.Plugin"
                                   && called.Name == "DriveRuntime");
            }
        }

        [Fact]
        public void AwakeProtectsTheSharedBepInExHostBeforePublishingPluginState()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(ManagementModeState).Assembly.Location))
            {
                var plugin = module.Types.Single(type => type.FullName == "GodViewManagement.Plugin");
                var awake = plugin.Methods.Single(method => method.Name == "Awake" && !method.HasParameters);
                var setHideFlags = awake.Body.Instructions.Single(instruction =>
                    instruction.Operand is MethodReference called
                    && called.DeclaringType.FullName == "UnityEngine.Object"
                    && called.Name == "set_hideFlags");
                var dontDestroy = awake.Body.Instructions.Single(instruction =>
                    instruction.Operand is MethodReference called
                    && called.DeclaringType.FullName == "UnityEngine.Object"
                    && called.Name == "DontDestroyOnLoad");
                var publishInstance = awake.Body.Instructions.Single(instruction =>
                    instruction.Operand is MethodReference called
                    && called.DeclaringType.FullName == "GodViewManagement.Plugin"
                    && called.Name == "set_Instance");

                Assert.Contains(awake.Body.Instructions.TakeWhile(item => item != setHideFlags),
                    instruction => instruction.OpCode.Code == Code.Ldc_I4_S
                                   && Convert.ToInt32(instruction.Operand) == 61);
                Assert.True(setHideFlags.Offset < publishInstance.Offset);
                Assert.True(dontDestroy.Offset < publishInstance.Offset);
            }
        }

        [Fact]
        public void HudFactoryProvidesHideInsteadOfAnAlwaysVisibleModeToggle()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(ManagementModeState).Assembly.Location))
            {
                var hud = module.Types.Single(type => type.FullName == "GodViewManagement.Runtime.GodViewHud");
                var factory = hud.Methods.Single(method => method.Name == "TryCreate");

                Assert.Equal(
                    new[] { "game", "openSettings", "restoreDefault", "hideHud", "closeSettings" },
                    factory.Parameters.Select(parameter => parameter.Name).ToArray());
            }
        }

        [Fact]
        public void HudUsesOneLeftShiftedSettingsLauncher()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(ManagementModeState).Assembly.Location))
            {
                var hud = module.Types.Single(type => type.FullName == "GodViewManagement.Runtime.GodViewHud");
                var factory = hud.Methods.Single(method => method.Name == "TryCreate");
                var strings = factory.Body.Instructions
                    .Where(instruction => instruction.OpCode.Code == Code.Ldstr)
                    .Select(instruction => (string)instruction.Operand)
                    .ToArray();

                Assert.DoesNotContain("Mode", strings);
                Assert.Contains("Settings", strings);
                Assert.Contains("HideHud", strings);
                Assert.Contains(factory.Body.Instructions,
                    instruction => instruction.OpCode.Code == Code.Ldc_R4
                                   && Convert.ToSingle(instruction.Operand) == -420f);
            }
        }

        [Theory]
        [InlineData("ShowSettings")]
        [InlineData("HideSettings")]
        [InlineData("Refresh")]
        public void HudOperationsCheckUnityLifetimeBeforeTouchingSceneObjects(string methodName)
        {
            using (var module = ModuleDefinition.ReadModule(typeof(ManagementModeState).Assembly.Location))
            {
                var hud = module.Types.Single(type =>
                    type.FullName == "GodViewManagement.Runtime.GodViewHud");
                var method = hud.Methods.Single(item => item.Name == methodName);

                Assert.Contains(method.Body.Instructions,
                    instruction => instruction.Operand is MethodReference called
                                   && called.DeclaringType.FullName == "GodViewManagement.Runtime.GodViewHud"
                                   && called.Name == "get_IsAlive");
            }
        }

        [Fact]
        public void FailSafeResetDropsDestroyedHudInsteadOfRefreshingIt()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(ManagementModeState).Assembly.Location))
            {
                var runtime = module.Types.Single(type =>
                    type.FullName == "GodViewManagement.Runtime.GodViewRuntime");
                var reset = runtime.Methods.Single(method => method.Name == "FailSafeReset");

                Assert.Contains(reset.Body.Instructions,
                    instruction => instruction.Operand is MethodReference called
                                   && called.DeclaringType.FullName == "GodViewManagement.Runtime.GodViewHud"
                                   && called.Name == "get_IsAlive");
            }
        }

        [Fact]
        public void PluginDoesNotReferenceForbiddenBuildOrTestLibraries()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(ManagementModeState).Assembly.Location))
            {
                var forbidden = new[] { "Mono.Cecil", "xunit.core", "Microsoft.NET.Test.Sdk" };
                Assert.DoesNotContain(module.AssemblyReferences,
                    reference => forbidden.Contains(reference.Name, StringComparer.OrdinalIgnoreCase));
            }
        }
    }
}
