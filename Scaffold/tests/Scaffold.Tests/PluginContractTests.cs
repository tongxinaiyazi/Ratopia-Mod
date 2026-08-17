using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using ScaffoldMod.Core;
using Xunit;

namespace Scaffold.Tests
{
    public sealed class PluginContractTests
    {
        private static readonly string[] RequiredPatchTypes =
        {
            "ScaffoldMod.Patches.BuildDatabasePatch",
            "ScaffoldMod.Patches.BuildEnableCheckPatch",
            "ScaffoldMod.Patches.BlueprintSetPatch",
            "ScaffoldMod.Patches.SpriteLoadPatch",
            "ScaffoldMod.Patches.TileManagerUpdatePatch",
            "ScaffoldMod.Patches.MapDataMappingPatch",
            "ScaffoldMod.Patches.NodeTypeCheckPatch",
            "ScaffoldMod.Patches.MiningEnableTilePatch",
            "ScaffoldMod.Patches.DemolitionBuildingPriorityPatch",
            "ScaffoldMod.Patches.MiningBoxUpdatePatch",
            "ScaffoldMod.Patches.LoadDataPatch",
            "ScaffoldMod.Patches.BeforeLoadPatch",
            "ScaffoldMod.Patches.SelectionInfoPatch"
        };

        [Fact]
        public void PluginMetadataIsStable()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(ScaffoldClock).Assembly.Location))
            {
                var plugin = module.Types.Single(type => type.FullName == "ScaffoldMod.Plugin");
                var attribute = plugin.CustomAttributes.Single(item =>
                    item.AttributeType.FullName == "BepInEx.BepInPlugin");

                Assert.Equal("cn.ratopia.scaffold", attribute.ConstructorArguments[0].Value);
                Assert.Equal("脚手架", attribute.ConstructorArguments[1].Value);
                Assert.Equal("0.1.0", attribute.ConstructorArguments[2].Value);
            }
        }

        [Fact]
        public void EveryRequiredPatchIsInstalledByAttributeDiscovery()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(ScaffoldClock).Assembly.Location))
            {
                foreach (var name in RequiredPatchTypes)
                {
                    var patch = module.Types.Single(type => type.FullName == name);
                    Assert.Contains(patch.CustomAttributes,
                        attribute => attribute.AttributeType.FullName == "HarmonyLib.HarmonyPatch");
                }
            }
        }

        [Fact]
        public void PluginDoesNotRedistributeBuildTimeDependencies()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(ScaffoldClock).Assembly.Location))
            {
                var forbidden = new[] { "Mono.Cecil", "xunit.core", "Microsoft.NET.Test.Sdk" };
                Assert.DoesNotContain(module.AssemblyReferences,
                    reference => forbidden.Contains(reference.Name, StringComparer.OrdinalIgnoreCase));
            }
        }

        [Fact]
        public void RuntimeOwnsAnIndependentOverlayViewInsteadOfARealGameBuilding()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(ScaffoldClock).Assembly.Location))
            {
                Assert.Contains(module.Types, type => type.FullName == "ScaffoldMod.Runtime.ScaffoldView");
                Assert.DoesNotContain(module.Types, type =>
                    type.FullName == "ScaffoldMod.Runtime.ScaffoldView" && type.BaseType.FullName == "Building");
            }
        }

        [Fact]
        public void InstantPlacementCancelsTheTemporaryBlueprintBeforeDeductingLumber()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(ScaffoldClock).Assembly.Location))
            {
                var patch = module.Types.Single(type => type.FullName == "ScaffoldMod.Patches.BlueprintSetPatch");
                var postfix = patch.Methods.Single(method => method.Name == "Postfix");
                var calls = postfix.Body.Instructions
                    .Where(instruction => instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
                    .Select(instruction => instruction.Operand as MethodReference)
                    .Where(method => method != null)
                    .ToArray();

                var cancel = Array.FindIndex(calls, method =>
                    method.DeclaringType.FullName == "BP_Building" && method.Name == "CancelBP");
                var deduct = Array.FindIndex(calls, method =>
                    method.DeclaringType.FullName == "BuildingMgr" && method.Name == "UseStorageResource");
                var create = Array.FindIndex(calls, method =>
                    method.DeclaringType.FullName == "ScaffoldMod.Runtime.ScaffoldRuntime" && method.Name == "TryPlace");

                Assert.True(cancel >= 0 && deduct > cancel && create > deduct);
            }
        }

        [Fact]
        public void DemolitionConsumesScaffoldMarksBeforeVanillaUpdateHandlesBuildings()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(ScaffoldClock).Assembly.Location))
            {
                var patch = module.Types.Single(type => type.FullName == "ScaffoldMod.Patches.MiningBoxUpdatePatch");
                var prefix = patch.Methods.Single(method => method.Name == "Prefix");

                Assert.Contains(prefix.Body.Instructions,
                    instruction => instruction.Operand is MethodReference method &&
                                   method.DeclaringType.FullName == "ScaffoldMod.Runtime.ScaffoldRuntime" &&
                                   method.Name == "Remove");
            }
        }

        [Fact]
        public void SolidTileConflictIsRemovedFromTheNodeRebuildPath()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(ScaffoldClock).Assembly.Location))
            {
                var runtime = module.Types.Single(type => type.FullName == "ScaffoldMod.Runtime.ScaffoldRuntime");
                var postfix = runtime.Methods.Single(method => method.Name == "NodeTypeCheckPostfix");

                Assert.Contains(postfix.Body.Instructions,
                    instruction => instruction.Operand is MethodReference method &&
                                   method.DeclaringType.FullName == "ScaffoldMod.Runtime.ScaffoldRuntime" &&
                                   method.Name == "Remove");
            }
        }

        [Fact]
        public void PluginExposesNoPublicApiBeyondTheBepInExEntryPoint()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(ScaffoldClock).Assembly.Location))
            {
                var publicTypes = module.Types
                    .Where(type => type.IsPublic)
                    .Select(type => type.FullName)
                    .ToArray();

                Assert.Equal(new[] { "ScaffoldMod.Plugin" }, publicTypes);
            }
        }
    }
}
