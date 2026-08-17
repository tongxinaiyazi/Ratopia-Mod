using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using WireThroughWalls.Core;
using Xunit;

namespace WireThroughWalls.Tests
{
    public sealed class PluginContractTests
    {
        private static readonly string[] RequiredPatchTypes =
        {
            "WireThroughWalls.Patches.MiningBoxBuildEnableCheckPatch",
            "WireThroughWalls.Patches.BlueprintSetPatch",
            "WireThroughWalls.Patches.BlueprintEnableCheckPatch",
            "WireThroughWalls.Patches.BlueprintMakeEnableListPatch",
            "WireThroughWalls.Patches.BlueprintBuildingUpdatePatch",
            "WireThroughWalls.Patches.BlueprintCancelPatch",
            "WireThroughWalls.Patches.TileDestroyProtectionPatch",
            "WireThroughWalls.Patches.HeavyWireBuildingSetPatch",
            "WireThroughWalls.Patches.HeavyWireDemolitionPatch",
            "WireThroughWalls.Patches.HeavyWireLoadPatch",
            "WireThroughWalls.Patches.HeavyWireWorkStopPatch",
            "WireThroughWalls.Patches.HeavyWireWorkResumePatch",
            "WireThroughWalls.Patches.NewConnectCheckPatch",
            "WireThroughWalls.Patches.DeleteSingleConnectCheckPatch",
            "WireThroughWalls.Patches.DeleteManyConnectCheckPatch",
            "WireThroughWalls.Patches.MiningBoxDemolitionScopePatch",
            "WireThroughWalls.Patches.WireFirstBuildingLookupPatch",
            "WireThroughWalls.Patches.MiniInfoSelectionPatch",
            "WireThroughWalls.Patches.QueenCheckBoxTriggerEnterPatch"
        };

        [Fact]
        public void PluginMetadataIsStable()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(OverlayRules).Assembly.Location))
            {
                var plugin = module.Types.Single(type => type.FullName == "WireThroughWalls.Plugin");
                var attribute = plugin.CustomAttributes.Single(item =>
                    item.AttributeType.FullName == "BepInEx.BepInPlugin");

                Assert.Equal("cn.ratopia.wirethroughwalls", attribute.ConstructorArguments[0].Value);
                Assert.Equal("电线可穿墙", attribute.ConstructorArguments[1].Value);
                Assert.Equal("0.1.3", attribute.ConstructorArguments[2].Value);
            }
        }

        [Fact]
        public void EveryRequiredPatchTypeIsInstalledByAttributeDiscovery()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(OverlayRules).Assembly.Location))
            {
                foreach (var name in RequiredPatchTypes)
                {
                    var patch = module.Types.Single(type => type.FullName == name);
                    Assert.Contains(patch.CustomAttributes,
                        attribute => attribute.AttributeType.FullName == "HarmonyLib.HarmonyPatch");
                }
            }
        }

        [Theory]
        [InlineData("WireThroughWalls.Patches.MiningBoxBuildEnableCheckPatch")]
        [InlineData("WireThroughWalls.Patches.BlueprintSetPatch")]
        [InlineData("WireThroughWalls.Patches.BlueprintEnableCheckPatch")]
        [InlineData("WireThroughWalls.Patches.BlueprintMakeEnableListPatch")]
        [InlineData("WireThroughWalls.Patches.BlueprintBuildingUpdatePatch")]
        [InlineData("WireThroughWalls.Patches.BlueprintCancelPatch")]
        [InlineData("WireThroughWalls.Patches.HeavyWireBuildingSetPatch")]
        [InlineData("WireThroughWalls.Patches.HeavyWireDemolitionPatch")]
        [InlineData("WireThroughWalls.Patches.HeavyWireLoadPatch")]
        [InlineData("WireThroughWalls.Patches.HeavyWireWorkStopPatch")]
        [InlineData("WireThroughWalls.Patches.HeavyWireWorkResumePatch")]
        [InlineData("WireThroughWalls.Patches.DeleteSingleConnectCheckPatch")]
        [InlineData("WireThroughWalls.Patches.DeleteManyConnectCheckPatch")]
        [InlineData("WireThroughWalls.Patches.MiningBoxDemolitionScopePatch")]
        public void StatefulPatchesHaveAFinalizer(string patchTypeName)
        {
            using (var module = ModuleDefinition.ReadModule(typeof(OverlayRules).Assembly.Location))
            {
                var patch = module.Types.Single(type => type.FullName == patchTypeName);
                Assert.Contains(patch.Methods, method => method.Name == "Finalizer");
            }
        }

        [Fact]
        public void PluginDoesNotReferenceRedistributedRuntimeHelpers()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(OverlayRules).Assembly.Location))
            {
                var forbidden = new[] { "Mono.Cecil", "xunit.core", "Microsoft.VisualStudio.TestPlatform.TestFramework" };
                Assert.DoesNotContain(module.AssemblyReferences,
                    reference => forbidden.Contains(reference.Name, StringComparer.OrdinalIgnoreCase));
            }
        }

        [Fact]
        public void EndingASessionRearmsInitializationForAReusedBuildingManager()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(OverlayRules).Assembly.Location))
            {
                var plugin = module.Types.Single(type => type.FullName == "WireThroughWalls.Plugin");
                var method = plugin.Methods.Single(item => item.Name == "EndCurrentSession");

                Assert.Contains(method.Body.Instructions,
                    instruction => instruction.Operand is MethodReference called &&
                                   called.DeclaringType.FullName.StartsWith("WireThroughWalls.Core.SessionTracker`1", StringComparison.Ordinal) &&
                                   called.Name == "MarkInitializationFailed");
            }
        }

        [Fact]
        public void RuntimeNeverCallsNodeTypeCheckToRepairForegroundNodes()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(OverlayRules).Assembly.Location))
            {
                Assert.DoesNotContain(
                    module.Types.SelectMany(type => type.Methods).SelectMany(method =>
                        method.HasBody ? method.Body.Instructions : Enumerable.Empty<Instruction>()),
                    instruction => instruction.Operand is MethodReference called &&
                                   called.DeclaringType.FullName == "TileMgr" &&
                                   called.Name == "NodeTypeCheck");
            }
        }

        [Fact]
        public void NodeSnapshotPreservesEverySpecialForegroundField()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(OverlayRules).Assembly.Location))
            {
                var snapshot = module.Types.Single(type =>
                    type.FullName == "WireThroughWalls.Runtime.NodeStateSnapshot");
                var entry = snapshot.NestedTypes.Single(type => type.Name == "Entry");
                var fields = entry.Fields.Select(field => field.Name).ToArray();

                Assert.Contains("<TileType>k__BackingField", fields);
                Assert.Contains("<NodeType>k__BackingField", fields);
                Assert.Contains("<BuildType>k__BackingField", fields);
                Assert.Contains("<RailSlope>k__BackingField", fields);
                Assert.Contains("<WorldObject>k__BackingField", fields);
            }
        }

        [Fact]
        public void CoordinatorDoesNotContainLegacyForegroundTypeInferenceOrNodeRepair()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(OverlayRules).Assembly.Location))
            {
                var coordinator = module.Types.Single(type =>
                    type.FullName == "WireThroughWalls.Runtime.WireOverlayCoordinator");
                var methods = coordinator.Methods.Select(method => method.Name).ToArray();

                Assert.DoesNotContain("ResolveForegroundTileType", methods);
                Assert.DoesNotContain("ReconcilePosition", methods);
                Assert.DoesNotContain("ReconcilePositions", methods);
                Assert.DoesNotContain("ReconcileAll", methods);
            }
        }

        [Fact]
        public void LegacyPartialNodeSnapshotTypesAreGone()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(OverlayRules).Assembly.Location))
            {
                Assert.DoesNotContain(module.Types,
                    type => type.FullName == "WireThroughWalls.Runtime.NodeTileSnapshot");
                Assert.DoesNotContain(module.Types,
                    type => type.FullName == "WireThroughWalls.Runtime.NodeOccupancySnapshot");
            }
        }

        [Fact]
        public void PortRegistryRestoresMissingLinesMergesAndPublishesARepresentative()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(OverlayRules).Assembly.Location))
            {
                var registry = module.Types.Single(type =>
                    type.FullName == "WireThroughWalls.Runtime.PortOverlayRegistry");
                var reconcile = registry.Methods.Single(method => method.Name == "Reconcile");
                var calls = reconcile.Body.Instructions
                    .Select(instruction => instruction.Operand)
                    .OfType<MethodReference>()
                    .Select(method => method.DeclaringType.FullName + "::" + method.Name)
                    .ToArray();

                Assert.Contains("BuildingMgr::NewConnectCheck", calls);
                Assert.Contains("BuildingMgr::MergeTwoElecLine", calls);
                Assert.Contains("BuildingMgr::RefreshWire", calls);
                Assert.Contains(reconcile.Body.Instructions,
                    instruction => instruction.Operand is FieldReference field &&
                                   field.DeclaringType.FullName == "BuildingMgr" &&
                                   field.Name == "Dic_PortTileMap");
            }
        }

        [Fact]
        public void PortRegistryDoesNotReevaluateEveryPowerBuilding()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(OverlayRules).Assembly.Location))
            {
                var registry = module.Types.Single(type =>
                    type.FullName == "WireThroughWalls.Runtime.PortOverlayRegistry");
                var reconcile = registry.Methods.Single(method => method.Name == "Reconcile");
                var calls = reconcile.Body.Instructions
                    .Select(instruction => instruction.Operand)
                    .OfType<MethodReference>()
                    .Select(method => method.DeclaringType.FullName + "::" + method.Name)
                    .ToArray();

                Assert.DoesNotContain("BuildingMgr::RefreshElecUseBuilding", calls);
                Assert.DoesNotContain("BuildingMgr::RefreshElecMakeBuilding", calls);
            }
        }

        [Fact]
        public void PortRegistryRefreshesWireOnlyAfterAStateChange()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(OverlayRules).Assembly.Location))
            {
                var registry = module.Types.Single(type =>
                    type.FullName == "WireThroughWalls.Runtime.PortOverlayRegistry");
                var reconcile = registry.Methods.Single(method => method.Name == "Reconcile");
                var instructions = reconcile.Body.Instructions.ToArray();
                var refreshIndex = Array.FindIndex(instructions, instruction =>
                    instruction.Operand is MethodReference called &&
                    called.DeclaringType.FullName == "BuildingMgr" &&
                    called.Name == "RefreshWire");

                Assert.True(refreshIndex >= 0, "Reconcile 必须保留局部电线显示刷新。\n");
                Assert.Contains(
                    instructions.Skip(Math.Max(0, refreshIndex - 8)).Take(8),
                    instruction => instruction.OpCode.FlowControl == FlowControl.Cond_Branch);
            }
        }

        [Fact]
        public void DemolitionSelectionChecksBothAltKeysInsideTheDemolitionScope()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(OverlayRules).Assembly.Location))
            {
                var patch = module.Types.Single(type =>
                    type.FullName == "WireThroughWalls.Patches.WireFirstBuildingLookupPatch");
                var prefix = patch.Methods.Single(method => method.Name == "Prefix");
                var getKeyCalls = prefix.Body.Instructions.Count(instruction =>
                    instruction.Operand is MethodReference called &&
                    called.DeclaringType.FullName == "UnityEngine.Input" &&
                    called.Name == "GetKey");

                Assert.Equal(2, getKeyCalls);
            }
        }

        [Fact]
        public void HighlightSelectionSynchronizesCompletedAndBlueprintInteractionTargets()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(OverlayRules).Assembly.Location))
            {
                var patch = module.Types.Single(type =>
                    type.FullName == "WireThroughWalls.Patches.MiniInfoSelectionPatch");
                var synchronize = patch.Methods.Single(method => method.Name == "Synchronize");
                var targetWrites = synchronize.Body.Instructions
                    .Select(instruction => instruction.Operand)
                    .OfType<FieldReference>()
                    .Where(field => field.DeclaringType.FullName == "QueenCheckBox")
                    .Select(field => field.Name)
                    .ToArray();

                Assert.Contains("m_Building", targetWrites);
                Assert.Contains("m_BP_Building", targetWrites);
                Assert.Equal(2, synchronize.Body.Instructions.Count(instruction =>
                    instruction.Operand is MethodReference called &&
                    called.DeclaringType.FullName ==
                        "WireThroughWalls.Core.InteractionSelectionRules" &&
                    called.Name == "PreferSelectedTarget"));
            }
        }

        [Fact]
        public void LaterColliderEntriesRestoreTheAlreadyHighlightedInteractionTarget()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(OverlayRules).Assembly.Location))
            {
                var patch = module.Types.Single(type =>
                    type.FullName == "WireThroughWalls.Patches.QueenCheckBoxTriggerEnterPatch");
                var postfix = patch.Methods.Single(method => method.Name == "Postfix");

                Assert.Contains(postfix.Body.Instructions,
                    instruction => instruction.Operand is MethodReference called &&
                                   called.DeclaringType.FullName ==
                                       "WireThroughWalls.Patches.MiniInfoSelectionPatch" &&
                                   called.Name == "Synchronize");
            }
        }

        [Theory]
        [InlineData("WireThroughWalls.Patches.DeleteSingleConnectCheckPatch")]
        [InlineData("WireThroughWalls.Patches.DeleteManyConnectCheckPatch")]
        public void FailedOriginalPortDeletionKeepsTheCapturedOwnerRegistered(string patchTypeName)
        {
            using (var module = ModuleDefinition.ReadModule(typeof(OverlayRules).Assembly.Location))
            {
                var patch = module.Types.Single(type => type.FullName == patchTypeName);
                var finalizer = patch.Methods.Single(method => method.Name == "Finalizer");

                Assert.Contains(finalizer.Body.Instructions,
                    instruction => instruction.Operand is MethodReference called &&
                                   called.Name == "CancelSafely");
            }
        }

        [Fact]
        public void NestedBlueprintChecksReuseTheOutermostTransparencyView()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(OverlayRules).Assembly.Location))
            {
                var patch = module.Types.Single(type =>
                    type.FullName == "WireThroughWalls.Patches.BlueprintEnableCheckPatch");
                var create = patch.Methods.Single(method => method.Name == "CreateTransparencyState");

                Assert.Contains(create.Body.Instructions,
                    instruction => instruction.Operand is MethodReference called &&
                                   called.DeclaringType.FullName == "WireThroughWalls.Runtime.WireActionScope" &&
                                   called.Name == "get_IsTransparencyActive");
            }
        }

        [Fact]
        public void SessionInitializationReevaluatesWireRelatedBlueprintsAfterLoading()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(OverlayRules).Assembly.Location))
            {
                var plugin = module.Types.Single(type => type.FullName == "WireThroughWalls.Plugin");
                var poll = plugin.Methods.Single(method => method.Name == "PollGameSession");

                Assert.Contains(poll.Body.Instructions,
                    instruction => instruction.Operand is MethodReference called &&
                                   called.DeclaringType.FullName == "WireThroughWalls.Runtime.WireOverlayCoordinator" &&
                                   called.Name == "ReevaluateBlueprints");
            }
        }

        [Fact]
        public void WireBlueprintCreationProtectsTilesBeforeInstantCompletionCanDestroyThem()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(OverlayRules).Assembly.Location))
            {
                var patch = module.Types.Single(type =>
                    type.FullName == "WireThroughWalls.Patches.BlueprintSetPatch");
                var prefix = patch.Methods.Single(method => method.Name == "Prefix");

                Assert.Contains(prefix.Body.Instructions,
                    instruction => instruction.Operand is MethodReference called &&
                                   called.DeclaringType.FullName == "WireThroughWalls.Runtime.WireActionScope" &&
                                   called.Name == "ProtectTiles");
            }
        }
    }
}
