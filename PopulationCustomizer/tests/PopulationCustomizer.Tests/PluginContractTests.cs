using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PopulationCustomizer.Core;
using Xunit;

namespace PopulationCustomizer.Tests
{
    public sealed class PluginContractTests
    {
        private static readonly string[] RequiredPatchTypes =
        {
            "PopulationCustomizer.Patches.CitizenLimitPatch",
            "PopulationCustomizer.Patches.RatronLimitPatch",
            "PopulationCustomizer.Patches.GameDataLoadPatch",
            "PopulationCustomizer.Patches.GameDataResetPatch",
            "PopulationCustomizer.Patches.StatisticsCitizenListUiPatch"
        };

        [Fact]
        public void PluginMetadataIsStable()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(LimitSettings).Assembly.Location))
            {
                var plugin = module.Types.Single(type => type.FullName == "PopulationCustomizer.Plugin");
                var attribute = plugin.CustomAttributes.Single(item => item.AttributeType.FullName == "BepInEx.BepInPlugin");

                Assert.Equal("cn.ratopia.populationcustomizer", attribute.ConstructorArguments[0].Value);
                Assert.Equal("人口自定义", attribute.ConstructorArguments[1].Value);
                Assert.Equal("0.1.3", attribute.ConstructorArguments[2].Value);
            }
        }

        [Fact]
        public void EveryRequiredPatchTypeUsesHarmonyPatchDiscovery()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(LimitSettings).Assembly.Location))
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
        public void LimitPatchesDelegateToRuntimeResolver()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(LimitSettings).Assembly.Location))
            {
                AssertCalls(module, "PopulationCustomizer.Patches.CitizenLimitPatch", "Postfix", "ResolveCitizen");
                AssertCalls(module, "PopulationCustomizer.Patches.RatronLimitPatch", "Postfix", "ResolveRatron");
            }
        }

        [Fact]
        public void LifecyclePatchesUseLoadedGameDataInsteadOfTileManagerAwake()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(LimitSettings).Assembly.Location))
            {
                AssertCalls(
                    module,
                    "PopulationCustomizer.Patches.GameDataLoadPatch",
                    "Postfix",
                    "PopulationCustomizer.Plugin",
                    "BeginGameSession");
                AssertCalls(
                    module,
                    "PopulationCustomizer.Patches.GameDataResetPatch",
                    "Prefix",
                    "PopulationCustomizer.Plugin",
                    "ResetGameSession");
                AssertCalls(
                    module,
                    "PopulationCustomizer.Patches.StatisticsCitizenListUiPatch",
                    "Postfix",
                    "PopulationCustomizer.Plugin",
                    "AttachStatisticsCitizenListUi");
                Assert.DoesNotContain(module.Types,
                    type => type.FullName == "PopulationCustomizer.Patches.GameSessionPatch");
                Assert.DoesNotContain(module.Types,
                    type => type.FullName == "PopulationCustomizer.Patches.CitizenUiPatch");
            }
        }

        [Fact]
        public void SavingSettingsVerifiesTheValueWrittenToModsData()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(LimitSettings).Assembly.Location))
            {
                AssertCalls(
                    module,
                    "PopulationCustomizer.Runtime.SaveSettingsStore",
                    "TrySaveCurrent",
                    "Utility.Savable.SavableData",
                    "GetValue");
            }
        }

        [Fact]
        public void EditingEitherLimitAutomaticallyEnablesThatCustomizationToggle()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(LimitSettings).Assembly.Location))
            {
                const string panelName = "PopulationCustomizer.Runtime.PopulationSettingsPanel";
                var panel = module.Types.Single(type => type.FullName == panelName);
                var build = panel.Methods.Single(method => method.Name == "Build");
                var buildCalls = build.Body.Instructions
                    .Select(instruction => instruction.Operand)
                    .OfType<MethodReference>()
                    .ToArray();

                Assert.True(buildCalls.Count(called =>
                    called.DeclaringType.FullName == "TMPro.TMP_InputField" &&
                    called.Name == "get_onValueChanged") >= 2);
                AssertCalls(module, panelName, "HandleCitizenInputChanged", "UnityEngine.UI.Toggle", "set_isOn");
                AssertCalls(module, panelName, "HandleRatronInputChanged", "UnityEngine.UI.Toggle", "set_isOn");
            }
        }

        [Fact]
        public void StatisticsEntryClonesNativeButtonClearsListenersAndMovesBeforeSearch()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(LimitSettings).Assembly.Location))
            {
                var panel = module.Types.Single(type =>
                    type.FullName == "PopulationCustomizer.Runtime.PopulationSettingsPanel");
                var calls = panel.Methods
                    .Where(method => method.HasBody)
                    .SelectMany(method => method.Body.Instructions)
                    .Select(instruction => instruction.Operand)
                    .OfType<MethodReference>()
                    .ToArray();

                Assert.Contains(calls, called =>
                    called.DeclaringType.FullName == "UnityEngine.Object" &&
                    called.Name == "Instantiate");
                Assert.Contains(calls, called =>
                    called.DeclaringType.FullName == "UnityEngine.Events.UnityEventBase" &&
                    called.Name == "RemoveAllListeners");
                Assert.Contains(calls, called =>
                    called.DeclaringType.FullName == "UnityEngine.UI.Selectable" &&
                    called.Name == "get_targetGraphic");
                Assert.Contains(calls, called =>
                    called.DeclaringType.FullName == "UnityEngine.Transform" &&
                    called.Name == "SetSiblingIndex");
            }
        }

        [Fact]
        public void StatisticsEntryUsesExplicitAnchoredPositionInsteadOfRelyingOnHeaderLayout()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(LimitSettings).Assembly.Location))
            {
                var panel = module.Types.Single(type =>
                    type.FullName == "PopulationCustomizer.Runtime.PopulationSettingsPanel");
                var method = panel.Methods.Single(item => item.Name == "CreateStatisticsEntry");
                var calls = method.Body.Instructions
                    .Select(instruction => instruction.Operand)
                    .OfType<MethodReference>()
                    .ToArray();

                Assert.True(calls.Count(called =>
                    called.DeclaringType.FullName == "UnityEngine.RectTransform" &&
                    called.Name == "get_anchoredPosition") >= 2);
                Assert.Contains(calls, called =>
                    called.DeclaringType.FullName == "UnityEngine.RectTransform" &&
                    called.Name == "set_anchoredPosition");
            }
        }

        [Fact]
        public void SettingsStoreUsesStableKeyAndSavableDataApi()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(LimitSettings).Assembly.Location))
            {
                var store = module.Types.Single(type => type.FullName == "PopulationCustomizer.Runtime.SaveSettingsStore");
                var key = store.Fields.Single(field => field.Name == "SettingsKey");
                Assert.Equal("cn.ratopia.populationcustomizer.settings", key.Constant);

                AssertCalls(module, store.FullName, "LoadCurrent", "Utility.Savable.SavableData", "GetValue");
                AssertCalls(module, store.FullName, "TrySaveCurrent", "Utility.Savable.SavableData", "AddData");
                AssertCalls(module, store.FullName, "TryRemoveCurrent", "Utility.Savable.SavableData", "Remove");
            }
        }

        [Fact]
        public void PanelCapturesAndRestoresOriginalActionMap()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(LimitSettings).Assembly.Location))
            {
                const string panel = "PopulationCustomizer.Runtime.PopulationSettingsPanel";
                AssertCalls(module, panel, "CaptureInputMap", "CasselGames.Input.InputMgr", "get_NowActionMapKey");
                AssertCalls(module, panel, "CaptureInputMap", "CasselGames.Input.InputMgr", "SetActionMap");
                AssertCalls(module, panel, "RestoreInputMap", "CasselGames.Input.InputMgr", "SetDefaultActionMap");
                AssertCalls(module, panel, "RestoreInputMap", "CasselGames.Input.InputMgr", "SetActionMap");
            }
        }

        [Fact]
        public void SettingsOverlayIsDetachedAsARootCanvas()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(LimitSettings).Assembly.Location))
            {
                var panel = module.Types.Single(type =>
                    type.FullName == "PopulationCustomizer.Runtime.PopulationSettingsPanel");
                var build = panel.Methods.Single(method => method.Name == "Build");
                var instructions = build.Body.Instructions;

                Assert.Contains(instructions, instruction =>
                    instruction.OpCode == OpCodes.Callvirt &&
                    instruction.Operand is MethodReference called &&
                    called.DeclaringType.FullName == "UnityEngine.Transform" &&
                    called.Name == "SetParent" &&
                    instruction.Previous?.OpCode == OpCodes.Ldc_I4_0 &&
                    instruction.Previous.Previous?.OpCode == OpCodes.Ldnull);
            }
        }

        [Fact]
        public void ApplyingSettingsDoesNotInvokeGameSave()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(LimitSettings).Assembly.Location))
            {
                var plugin = module.Types.Single(type => type.FullName == "PopulationCustomizer.Plugin");
                var apply = plugin.Methods.Single(method => method.Name == "ApplySettings");
                Assert.DoesNotContain(apply.Body.Instructions,
                    instruction => instruction.Operand is MethodReference called &&
                                   called.DeclaringType.FullName == "PlayDataMgr" &&
                                   called.Name == "Save");
            }
        }

        [Fact]
        public void PluginDoesNotReferenceBuildOrTestLibraries()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(LimitSettings).Assembly.Location))
            {
                var forbidden = new[] { "Mono.Cecil", "xunit.core", "Microsoft.NET.Test.Sdk" };
                Assert.DoesNotContain(module.AssemblyReferences,
                    reference => forbidden.Contains(reference.Name, StringComparer.OrdinalIgnoreCase));
            }
        }

        private static void AssertCalls(ModuleDefinition module, string typeName, string methodName, string calledName)
        {
            AssertCalls(module, typeName, methodName, "PopulationCustomizer.Runtime.LimitRuntime", calledName);
        }

        private static void AssertCalls(
            ModuleDefinition module,
            string typeName,
            string methodName,
            string calledType,
            string calledName)
        {
            var type = module.Types.Single(item => item.FullName == typeName);
            var method = type.Methods.Single(item => item.Name == methodName);
            Assert.Contains(method.Body.Instructions,
                instruction => instruction.Operand is MethodReference called &&
                               called.DeclaringType.FullName == calledType &&
                               called.Name == calledName);
        }
    }
}
