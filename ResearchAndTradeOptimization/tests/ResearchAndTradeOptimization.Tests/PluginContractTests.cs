using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Xunit;

namespace ResearchAndTradeOptimization.Tests
{
    public sealed class PluginContractTests
    {
        [Fact]
        public void PluginMetadataAndLifecycleMatchTheReleaseContract()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var plugin = FindType(module, "ResearchAndTradeOptimization.Plugin");
                Assert.Equal("BepInEx.BaseUnityPlugin", plugin.BaseType.FullName);
                var metadata = plugin.CustomAttributes.Single(attribute =>
                    attribute.AttributeType.FullName == "BepInEx.BepInPlugin");
                Assert.Equal("cn.ratopia.unlimitedresearchandtradequeues", metadata.ConstructorArguments[0].Value);
                Assert.Equal("研究与贸易优化", metadata.ConstructorArguments[1].Value);
                Assert.Equal("0.3.0", metadata.ConstructorArguments[2].Value);

                var awake = plugin.Methods.Single(method => method.Name == "Awake");
                var onDestroy = plugin.Methods.Single(method => method.Name == "OnDestroy");
                AssertCalls(awake, "HarmonyLib.PatchClassProcessor", "Patch");
                AssertCalls(awake, "HarmonyLib.Harmony", "UnpatchSelf");
                AssertCalls(onDestroy, "HarmonyLib.Harmony", "UnpatchSelf");
                Assert.NotEmpty(awake.Body.ExceptionHandlers);
            }
        }

        [Fact]
        public void PatchClassesTargetTheInspectedMethods()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var expected = new Dictionary<string, (string typeName, string methodName)>
                {
                    ["ResearchAndTradeOptimization.Patches.ResearchQueueLimitPatch"] =
                        ("Tech_RPInfo", "UpgradBtn"),
                    ["ResearchAndTradeOptimization.Patches.ResearchQueueViewPatch"] =
                        ("ResearchingGroup", "ResearchingGroupSet"),
                    ["ResearchAndTradeOptimization.Patches.ResearchProgressPatch"] =
                        ("ResearchUI", "UpdateUpgradeNode"),
                    ["ResearchAndTradeOptimization.Patches.ResearchRefreshPatch"] =
                        ("UpgradeNode", "Refresh"),
                    ["ResearchAndTradeOptimization.Patches.ResearchRefundPatch"] =
                        ("Tech_RPInfo", "RemoveUpgradeNode"),
                    ["ResearchAndTradeOptimization.Patches.TradeAgreementLimitPatch"] =
                        ("CasselGames.Diplomatic.Data.DiplomaticCountryData", "IsFullTradeAgreement"),
                    ["ResearchAndTradeOptimization.Patches.TradeLayoutPatch"] =
                        ("CasselGames.Diplomatic.UI.DiplomaticTradeLayoutUI", "UpdateSlot"),
                    ["ResearchAndTradeOptimization.Patches.TradeWorldDetailPatch"] =
                        ("CasselGames.Diplomatic.UI.DiplomaticWorldDetailUI", "Refresh"),
                    ["ResearchAndTradeOptimization.Patches.TradeResourcePreviewPatch"] =
                        ("CasselGames.Diplomatic.UI.DiplomaticWorldDetailResourceLayoutUI", "SetData"),
                    ["ResearchAndTradeOptimization.Patches.FullTradeResourceSetPatch"] =
                        ("CasselGames.Diplomatic.Data.DiplomaticCountryData", "SetTradeResource"),
                    ["ResearchAndTradeOptimization.Patches.TradeResourceLoadPatch"] =
                        ("CasselGames.Diplomatic.Data.DiplomaticCountryData", "SetSavableData"),
                    ["ResearchAndTradeOptimization.Patches.TradeDetailModifySlotPatch"] =
                        ("CasselGames.Diplomatic.UI.DiplomaticTradeDetailUI", "Refresh"),
                    ["ResearchAndTradeOptimization.Patches.TradeModifyEventPatch"] =
                        ("CasselGames.Diplomatic.UI.DiplomaticUI", "OnTradeDetailEvent"),
                    ["ResearchAndTradeOptimization.Patches.TradeSheetLayoutEditPatch"] =
                        ("CasselGames.Diplomatic.UI.DiplomaticTradeSheetLayoutUI", "SetData"),
                    ["ResearchAndTradeOptimization.Patches.TradeSheetDetailSlotEditPatch"] =
                        ("CasselGames.Diplomatic.UI.DiplomaticTradeSheetDetailSlotUI", "SetData"),
                    ["ResearchAndTradeOptimization.Patches.TradeSheetSubmitEditPatch"] =
                        ("CasselGames.Diplomatic.UI.DiplomaticTradeSheetUI", "OnSubmitedEvent"),
                    ["ResearchAndTradeOptimization.Patches.TradeSheetHideEditPatch"] =
                        ("CasselGames.Diplomatic.UI.DiplomaticTradeSheetUI", "Hide"),
                    ["ResearchAndTradeOptimization.Patches.TradeSheetCleanUpEditPatch"] =
                        ("CasselGames.Diplomatic.UI.DiplomaticTradeSheetUI", "CleanUp"),
                    ["ResearchAndTradeOptimization.Patches.QuarterlyTradePricePatch"] =
                        ("CasselGames.Diplomatic.Data.DiplomaticCountryPackage", "RunProcessDaily")
                };

                var patchTypes = module.Types
                    .Where(type => type.Namespace == "ResearchAndTradeOptimization.Patches")
                    .ToArray();
                Assert.Equal(expected.Count, patchTypes.Length);

                foreach (var pair in expected)
                {
                    var patch = patchTypes.Single(type => type.FullName == pair.Key);
                    var attribute = patch.CustomAttributes.Single(item =>
                        item.AttributeType.FullName == "HarmonyLib.HarmonyPatch");
                    Assert.Equal(
                        pair.Value.typeName,
                        ((TypeReference)attribute.ConstructorArguments[0].Value).FullName);
                    Assert.Equal(pair.Value.methodName, attribute.ConstructorArguments[1].Value);
                }
            }
        }

        [Fact]
        public void PatchAdaptersDelegateToFocusedRuntimeAndTranspilerTypes()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                AssertCalls(
                    FindMethod(module, "ResearchAndTradeOptimization.Patches.ResearchQueueLimitPatch", "Transpiler"),
                    "ResearchAndTradeOptimization.Patching.ResearchQueueTranspiler",
                    "Rewrite");
                AssertCalls(
                    FindMethod(module, "ResearchAndTradeOptimization.Patches.ResearchQueueLimitPatch", "Transpiler"),
                    "ResearchAndTradeOptimization.Patching.ResearchReservationEnqueueTranspiler",
                    "Rewrite");
                AssertCalls(
                    FindMethod(module, "ResearchAndTradeOptimization.Patches.ResearchProgressPatch", "Transpiler"),
                    "ResearchAndTradeOptimization.Patching.ResearchProgressTranspiler",
                    "Rewrite");
                AssertCalls(
                    FindMethod(module, "ResearchAndTradeOptimization.Patches.ResearchRefreshPatch", "Prefix"),
                    "ResearchAndTradeOptimization.Runtime.ResearchReservationRuntime",
                    "CanRefresh");
                AssertCalls(
                    FindMethod(module, "ResearchAndTradeOptimization.Patches.ResearchRefundPatch", "Transpiler"),
                    "ResearchAndTradeOptimization.Patching.ResearchRefundTranspiler",
                    "Rewrite");
                AssertCalls(
                    FindMethod(module, "ResearchAndTradeOptimization.Patches.ResearchRefundPatch", "Prefix"),
                    "ResearchAndTradeOptimization.Runtime.ResearchReservationRuntime",
                    "BeginRefundOperation");
                AssertCalls(
                    FindMethod(module, "ResearchAndTradeOptimization.Patches.ResearchRefundPatch", "Finalizer"),
                    "ResearchAndTradeOptimization.Runtime.ResearchReservationRuntime",
                    "EndRefundOperation");
                AssertCalls(
                    FindMethod(module, "ResearchAndTradeOptimization.Patches.ResearchQueueViewPatch", "Prefix"),
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueRuntime",
                    "EnsureCurrentQueueVisible");
                AssertCalls(
                    FindMethod(module, "ResearchAndTradeOptimization.Patches.TradeLayoutPatch", "Transpiler"),
                    "ResearchAndTradeOptimization.Patching.TradeLayoutTranspiler",
                    "Rewrite");
                AssertCalls(
                    FindMethod(module, "ResearchAndTradeOptimization.Patches.TradeLayoutPatch", "Postfix"),
                    "ResearchAndTradeOptimization.Runtime.TradeQueueRuntime",
                    "UpdateLayoutLabel");
                AssertCalls(
                    FindMethod(module, "ResearchAndTradeOptimization.Patches.TradeWorldDetailPatch", "Prefix"),
                    "ResearchAndTradeOptimization.Runtime.TradeResourcePreviewRuntime",
                    "ApplyCompactDetailLayout");
                AssertCalls(
                    FindMethod(module, "ResearchAndTradeOptimization.Patches.TradeWorldDetailPatch", "Postfix"),
                    "ResearchAndTradeOptimization.Runtime.TradeQueueRuntime",
                    "UpdateWorldDetailLabel");
                AssertCalls(
                    FindMethod(module, "ResearchAndTradeOptimization.Patches.TradeResourcePreviewPatch", "Prefix"),
                    "ResearchAndTradeOptimization.Runtime.TradeResourcePreviewRuntime",
                    "LimitVisibleItems");
                Assert.DoesNotContain(
                    FindType(module, "ResearchAndTradeOptimization.Patches.TradeResourcePreviewPatch").Methods,
                    method => method.Name == "Postfix");
                AssertCalls(
                    FindMethod(module, "ResearchAndTradeOptimization.Patches.FullTradeResourceSetPatch", "Prefix"),
                    "ResearchAndTradeOptimization.Runtime.FullTradeResourceRuntime",
                    "TryApplyBothDirections");
                AssertCalls(
                    FindMethod(module, "ResearchAndTradeOptimization.Patches.TradeResourceLoadPatch", "Postfix"),
                    "ResearchAndTradeOptimization.Runtime.FullTradeResourceRuntime",
                    "RefreshAfterLoad");
                AssertCalls(
                    FindMethod(module, "ResearchAndTradeOptimization.Patches.TradeModifyEventPatch", "Prefix"),
                    "ResearchAndTradeOptimization.Runtime.TradeAgreementEditRuntime",
                    "OpenEditor");
                AssertCalls(
                    FindMethod(module, "ResearchAndTradeOptimization.Patches.TradeSheetSubmitEditPatch", "Prefix"),
                    "ResearchAndTradeOptimization.Runtime.TradeAgreementEditRuntime",
                    "HandleSubmittedData");
                AssertCalls(
                    FindMethod(module, "ResearchAndTradeOptimization.Patches.QuarterlyTradePricePatch", "Postfix"),
                    "ResearchAndTradeOptimization.Runtime.QuarterlyTradePriceRuntime",
                    "RefreshPrices");
            }
        }


        [Fact]
        public void OrdinaryPeriodMinimumIsAppliedBeforeEditSessionUnlocking()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var configure = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.TradeAgreementEditRuntime",
                    "ConfigureDetailSlot");
                var calls = configure.Body.Instructions
                    .Where(instruction => instruction.Operand is MethodReference)
                    .Select(instruction => (MethodReference)instruction.Operand)
                    .ToArray();
                var minimumIndex = Array.FindIndex(calls, call =>
                    call.DeclaringType.FullName ==
                        "ResearchAndTradeOptimization.Core.TradeAgreementRules" &&
                    call.Name == "GetPeriodMinimum");
                var sessionIndex = Array.FindIndex(calls, call =>
                    call.DeclaringType.FullName ==
                        "ResearchAndTradeOptimization.Runtime.TradeAgreementEditRuntime" &&
                    call.Name == "IsActiveSession");

                Assert.True(minimumIndex >= 0);
                Assert.True(sessionIndex >= 0);
                Assert.True(minimumIndex < sessionIndex);
            }
        }

        [Fact]
        public void FullPoolRuntimeGatesBothNewAndLoadedCountriesByGlobalGroups()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                AssertCalls(
                    FindMethod(
                        module,
                        "ResearchAndTradeOptimization.Runtime.FullTradeResourceRuntime",
                        "TryApplyBothDirections"),
                    "ResearchAndTradeOptimization.Core.FullTradeResourceRules",
                    "CanExpandAll");
                AssertCalls(
                    FindMethod(
                        module,
                        "ResearchAndTradeOptimization.Runtime.FullTradeResourceRuntime",
                        "RefreshAfterLoad"),
                    "ResearchAndTradeOptimization.Core.FullTradeResourceRules",
                    "CanExpandAll");
                AssertCalls(
                    FindMethod(
                        module,
                        "ResearchAndTradeOptimization.Runtime.FullTradeResourceRuntime",
                        "BuildBuckets"),
                    "CasselGames.Diplomatic.Asset.DiplomaticTradeResourceGroupData",
                    "get_IsGlobal");

                var buildBuckets = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.FullTradeResourceRuntime",
                    "BuildBuckets");
                Assert.Contains(buildBuckets.Body.Instructions, instruction =>
                    instruction.Operand is string text &&
                    text.Contains("找不到原版贸易资源组"));
            }
        }

        [Fact]
        public void OversizedLoadedGlobalPoolIsRepairedThroughVanillaReroll()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var refresh = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.FullTradeResourceRuntime",
                    "RefreshAfterLoad");
                AssertCalls(
                    refresh,
                    "ResearchAndTradeOptimization.Core.FullTradeResourceRules",
                    "NeedsVanillaRepair");
                AssertCalls(
                    refresh,
                    "ResearchAndTradeOptimization.Runtime.FullTradeResourceRuntime",
                    "RepairLegacyGlobalPool");
            }
        }

        [Fact]
        public void VanillaPoolRepairRestoresItsSnapshotWhenRerollFails()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var repair = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.FullTradeResourceRuntime",
                    "RepairLegacyGlobalPool");
                Assert.NotEmpty(repair.Body.ExceptionHandlers);
                AssertCalls(
                    repair,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "RemakeTradeData");
                AssertCalls(
                    repair,
                    "ResearchAndTradeOptimization.Runtime.FullTradeResourceRuntime",
                    "RestoreSnapshot");
            }
        }

        [Fact]
        public void VanillaPoolRepairPreservesAnUninitializedGlobalUsageList()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var capture = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.FullTradeResourceRuntime",
                    "CaptureSnapshot");
                AssertCalls(
                    capture,
                    "ResearchAndTradeOptimization.Runtime.FullTradeResourceRuntime",
                    "CopyOptionalList");

                var restore = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.FullTradeResourceRuntime",
                    "RestoreSnapshot");
                AssertCalls(
                    restore,
                    "ResearchAndTradeOptimization.Runtime.FullTradeResourceRuntime",
                    "RestoreOptionalGlobalUsageList");
            }
        }

        [Fact]
        public void TradeResourceLayoutCompactsBothNativeGridsWithoutMovingTheDetailPanel()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var runtime = FindType(
                    module,
                    "ResearchAndTradeOptimization.Runtime.TradeResourcePreviewRuntime");
                Assert.DoesNotContain(
                    runtime.Methods,
                    method => method.Name == "ApplyDetailPanelHeight");

                var apply = FindMethod(
                    module,
                    runtime.FullName,
                    "ApplyCompactDetailLayout");
                AssertCalls(
                    apply,
                    "ResearchAndTradeOptimization.Core.TradeResourcePreviewRules",
                    "CreateDetailPlan");
                AssertCalls(
                    apply,
                    "ResearchAndTradeOptimization.Runtime.TradeResourcePreviewRuntime",
                    "ApplyResourceLayout");
                Assert.DoesNotContain(apply.Body.Instructions, instruction =>
                    instruction.Operand is MethodReference called &&
                    called.DeclaringType.FullName == "UnityEngine.RectTransform" &&
                    called.Name == "set_anchoredPosition");

                var applyResource = FindMethod(
                    module,
                    runtime.FullName,
                    "ApplyResourceLayout");
                AssertCalls(
                    applyResource,
                    "UnityEngine.UI.GridLayoutGroup",
                    "set_cellSize");
                AssertCalls(
                    applyResource,
                    "UnityEngine.UI.GridLayoutGroup",
                    "set_spacing");
                AssertCalls(
                    applyResource,
                    "UnityEngine.UI.GridLayoutGroup",
                    "set_constraint");
                AssertCalls(
                    applyResource,
                    "UnityEngine.UI.GridLayoutGroup",
                    "set_constraintCount");
                AssertCalls(
                    applyResource,
                    "UnityEngine.RectTransform",
                    "set_sizeDelta");
                AssertCalls(
                    applyResource,
                    "UnityEngine.UI.LayoutRebuilder",
                    "ForceRebuildLayoutImmediate");
            }
        }

        [Fact]
        public void RuntimeExpandsNativeNodesAndUsesInfinityLabelsWithoutSaveOrConfigTypes()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var ensure = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueRuntime",
                    "EnsureVisibleCapacity");
                AssertCalls(ensure, "UnityEngine.Object", "Instantiate");
                AssertCalls(
                    ensure,
                    "ResearchAndTradeOptimization.Core.ResearchQueueLayoutRules",
                    "GetRowPosition");
                Assert.NotEmpty(ensure.Body.ExceptionHandlers);

                var layout = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.TradeQueueRuntime",
                    "UpdateLayoutLabel");
                var world = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.TradeQueueRuntime",
                    "UpdateWorldDetailLabel");
                AssertCalls(layout, "ResearchAndTradeOptimization.Core.QueueRules", "GetUnlimitedCountLabel");
                AssertCalls(world, "ResearchAndTradeOptimization.Core.QueueRules", "GetUnlimitedCountLabel");

                Assert.DoesNotContain(module.AssemblyReferences, reference =>
                    reference.Name.IndexOf("Configuration", StringComparison.OrdinalIgnoreCase) >= 0);
                Assert.DoesNotContain(module.Types, type =>
                    type.FullName.IndexOf("Save", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    type.FullName.IndexOf("Config", StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        [Fact]
        public void ResearchCapacityUsesViewportMetricsBeforeNativeNodeGrowth()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var ensure = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueRuntime",
                    "EnsureVisibleCapacity");
                AssertCalls(
                    ensure,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueLayoutRuntime",
                    "TryGetMetrics");
                AssertCalls(ensure, "UnityEngine.Object", "Instantiate");
                AssertCalls(
                    ensure,
                    "ResearchAndTradeOptimization.Core.ResearchQueueLayoutRules",
                    "GetRowPosition");
                Assert.NotEmpty(ensure.Body.ExceptionHandlers);

                var metrics = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueLayoutRuntime",
                    "TryGetMetrics");
                AssertCalls(metrics, "UnityEngine.RectTransform", "GetWorldCorners");
                AssertCalls(
                    metrics,
                    "ResearchAndTradeOptimization.Core.ResearchQueueLayoutRules",
                    "GetSlotCapacity");
            }
        }

        [Fact]
        public void ResearchViewPostfixAppliesSingleRowSummary()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                AssertCalls(
                    FindMethod(
                        module,
                        "ResearchAndTradeOptimization.Patches.ResearchQueueViewPatch",
                        "Postfix"),
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueLayoutRuntime",
                    "ApplySingleRowSummary");
            }
        }

        [Fact]
        public void OverflowIndicatorIsSeparateAndCannotReceiveClicks()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var apply = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueLayoutRuntime",
                    "ApplySingleRowSummary");
                AssertCalls(
                    apply,
                    "ResearchAndTradeOptimization.Core.ResearchQueueLayoutRules",
                    "CreateDisplayPlan");
                Assert.DoesNotContain(apply.Body.Instructions, instruction =>
                    instruction.OpCode.Code == Mono.Cecil.Cil.Code.Stind_Ref);

                var configure = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueLayoutRuntime",
                    "ConfigureOverflowIndicator");
                AssertCalls(
                    configure,
                    "UnityEngine.CanvasGroup",
                    "set_interactable");
                AssertCalls(
                    configure,
                    "UnityEngine.CanvasGroup",
                    "set_blocksRaycasts");
                Assert.Contains(configure.Body.Instructions, instruction =>
                    instruction.Operand is string text && text == "...");
            }
        }

        [Fact]
        public void ResearchSummaryLogsItsFirstRuntimeLayoutSnapshot()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var apply = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueLayoutRuntime",
                    "ApplySingleRowSummary");
                AssertCalls(
                    apply,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueLayoutRuntime",
                    "BuildLayoutDiagnostic");
                AssertCalls(
                    apply,
                    "ResearchAndTradeOptimization.Plugin",
                    "LogRuntimeInfo");

                var diagnostic = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueLayoutRuntime",
                    "BuildLayoutDiagnostic");
                AssertCalls(
                    diagnostic,
                    "UnityEngine.Component",
                    "GetComponents");
                Assert.Contains(diagnostic.Body.Instructions, instruction =>
                    instruction.Operand is string text &&
                    text.Contains("研究队列单行摘要首次应用"));
            }
        }

        [Fact]
        public void ResearchSummaryUsesTheFixedFiveItemDisplayPlan()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var apply = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueLayoutRuntime",
                    "ApplySingleRowSummary");
                var planCall = apply.Body.Instructions.Single(instruction =>
                    instruction.Operand is MethodReference called &&
                    called.DeclaringType.FullName ==
                        "ResearchAndTradeOptimization.Core.ResearchQueueLayoutRules" &&
                    called.Name == "CreateDisplayPlan");
                var planMethod = (MethodReference)planCall.Operand;
                Assert.Single(planMethod.Parameters);
            }
        }

        [Fact]
        public void ResearchSummaryDefersItsSnapshotUntilTheQueueExceedsFive()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var apply = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueLayoutRuntime",
                    "ApplySingleRowSummary");
                var instructions = apply.Body.Instructions;
                Assert.Contains(Enumerable.Range(0, instructions.Count - 1), index =>
                    instructions[index].OpCode.Code ==
                        Mono.Cecil.Cil.Code.Ldc_I4_5 &&
                    (instructions[index + 1].OpCode.Code ==
                         Mono.Cecil.Cil.Code.Ble ||
                     instructions[index + 1].OpCode.Code ==
                         Mono.Cecil.Cil.Code.Ble_S));
            }
        }

        [Fact]
        public void ResearchSummaryForcesTheNativeGridToOneRow()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var apply = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueLayoutRuntime",
                    "ApplySingleRowSummary");
                AssertCalls(
                    apply,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueLayoutRuntime",
                    "ConfigureSingleRowGrid");

                var configure = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueLayoutRuntime",
                    "ConfigureSingleRowGrid");
                AssertCalls(
                    configure,
                    "UnityEngine.UI.GridLayoutGroup",
                    "set_startAxis");
                AssertCalls(
                    configure,
                    "UnityEngine.UI.GridLayoutGroup",
                    "set_constraint");
                AssertCalls(
                    configure,
                    "UnityEngine.UI.GridLayoutGroup",
                    "set_constraintCount");
            }
        }

        [Fact]
        public void ResearchSummaryAlignsTheAreaAfterUpdatingItsWidth()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var apply = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueLayoutRuntime",
                    "ApplySingleRowSummary");
                var instructions = apply.Body.Instructions;
                var widthUpdate = -1;
                var alignment = -1;
                for (var index = 0; index < instructions.Count; index++)
                {
                    if (!(instructions[index].Operand is MethodReference called))
                    {
                        continue;
                    }

                    if (called.DeclaringType.FullName ==
                            "UnityEngine.RectTransform" &&
                        called.Name == "set_sizeDelta")
                    {
                        widthUpdate = index;
                    }
                    else if (called.DeclaringType.FullName ==
                                 "ResearchAndTradeOptimization.Runtime.ResearchQueueLayoutRuntime" &&
                             called.Name == "AlignAreaToCanvasLeft")
                    {
                        alignment = index;
                    }
                }

                Assert.True(widthUpdate >= 0);
                Assert.True(alignment > widthUpdate);

                var helper = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueLayoutRuntime",
                    "AlignAreaToCanvasLeft");
                Assert.Equal(2, helper.Body.Instructions.Count(instruction =>
                    instruction.Operand is MethodReference called &&
                    called.DeclaringType.FullName == "UnityEngine.RectTransform" &&
                    called.Name == "GetWorldCorners"));
                AssertCalls(
                    helper,
                    "ResearchAndTradeOptimization.Core.ResearchQueueLayoutRules",
                    "GetHorizontalAlignmentShift");
                AssertCalls(
                    helper,
                    "UnityEngine.RectTransform",
                    "set_anchoredPosition");
            }
        }

        [Fact]
        public void ResearchCapacityPrefersTheRightDetailPanelBoundary()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var metrics = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueLayoutRuntime",
                    "TryGetMetrics");
                AssertCalls(
                    metrics,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueLayoutRuntime",
                    "TryGetRightDetailBoundary");

                var boundary = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueLayoutRuntime",
                    "TryGetRightDetailBoundary");
                Assert.Contains(boundary.Body.Instructions, instruction =>
                    instruction.Operand is FieldReference field &&
                    field.FullName == "Tech_RPInfo ResearchUI::m_Tech_RPInfo");
                AssertCalls(
                    boundary,
                    "UnityEngine.RectTransform",
                    "GetWorldCorners");
            }
        }

        [Fact]
        public void ResearchExpansionUsesComponentTransformInsteadOfLazilyInitializedTechNodeField()
        {
            using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
            {
                var ensure = FindMethod(
                    module,
                    "ResearchAndTradeOptimization.Runtime.ResearchQueueRuntime",
                    "EnsureVisibleCapacity");

                AssertCalls(ensure, "UnityEngine.Component", "get_transform");
                Assert.DoesNotContain(ensure.Body.Instructions, instruction =>
                    instruction.Operand is FieldReference field &&
                    field.FullName == "UnityEngine.RectTransform TechNode::Tf");
            }
        }

        private static string GetPluginAssemblyPath()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "ResearchAndTradeOptimization.dll");
            Assert.True(File.Exists(path), $"Plugin assembly not found: {path}");
            return path;
        }

        private static TypeDefinition FindType(ModuleDefinition module, string fullName)
        {
            var type = module.Types.SingleOrDefault(item => item.FullName == fullName);
            Assert.NotNull(type);
            return type;
        }

        private static MethodDefinition FindMethod(ModuleDefinition module, string typeName, string methodName)
        {
            return FindType(module, typeName).Methods.Single(method => method.Name == methodName);
        }

        private static void AssertCalls(MethodDefinition method, string typeName, string methodName)
        {
            Assert.Contains(method.Body.Instructions, instruction =>
                instruction.Operand is MethodReference called &&
                called.DeclaringType.FullName == typeName &&
                called.Name == methodName);
        }
    }
}
