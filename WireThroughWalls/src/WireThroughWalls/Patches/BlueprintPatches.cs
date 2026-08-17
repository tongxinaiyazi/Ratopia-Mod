using System;
using WireThroughWalls.Core;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using WireThroughWalls.Runtime;

namespace WireThroughWalls.Patches
{
    [HarmonyPatch(typeof(BP_Building), nameof(BP_Building.BluePrintSet),
        new[] { typeof(BuildInfo), typeof(Vector2), typeof(int), typeof(int) })]
    internal static class BlueprintSetPatch
    {
        private static void Prefix(BuildInfo info, Vector2 pos, out LifecyclePatchState __state)
        {
            __state = null;
            try
            {
                Plugin.LogFirstInvocation(nameof(BlueprintSetPatch));
                if (Plugin.TryGetCoordinator(out var coordinator))
                {
                    var positions = coordinator.GetBuildPositions(info, pos);
                    if (coordinator.RequiresCoordination(info, positions))
                    {
                        __state = new LifecyclePatchState(coordinator, positions);
                        if (WireOverlayCoordinator.IsWire(info))
                        {
                            __state.AddScope(NodeStateSnapshot.Capture(coordinator.TileManager, positions));
                            __state.AddScope(WireActionScope.ProtectTiles(positions));
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("蓝图创建前状态捕获", error);
            }
        }

        private static void Postfix(BP_Building __result, LifecyclePatchState __state)
        {
            try
            {
                __state?.AddPositions(__result != null ? __result.List_BuildPos : null);
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("蓝图创建后位置采集", error);
            }
            finally
            {
                DisposeSafely(__state, "蓝图创建后前景格协调");
            }
        }

        private static Exception Finalizer(Exception __exception, LifecyclePatchState __state)
        {
            DisposeSafely(__state, "异常路径蓝图创建协调");
            return __exception;
        }

        internal static void DisposeSafely(IDisposable scope, string operation)
        {
            try
            {
                scope?.Dispose();
            }
            catch (Exception error)
            {
                Plugin.LogPatchError(operation, error);
            }
        }
    }

    [HarmonyPatch(typeof(BP_Building), nameof(BP_Building.EnableCheck), new Type[0])]
    internal static class BlueprintEnableCheckPatch
    {
        private static void Prefix(BP_Building __instance, out LifecyclePatchState __state)
        {
            __state = CreateTransparencyState(__instance, nameof(BlueprintEnableCheckPatch));
        }

        private static void Postfix(LifecyclePatchState __state)
        {
            BlueprintSetPatch.DisposeSafely(__state, "蓝图启用检查后前景格协调");
        }

        private static Exception Finalizer(Exception __exception, LifecyclePatchState __state)
        {
            BlueprintSetPatch.DisposeSafely(__state, "异常路径恢复蓝图启用检查");
            return __exception;
        }

        internal static LifecyclePatchState CreateTransparencyState(BP_Building blueprint, string operation)
        {
            try
            {
                Plugin.LogFirstInvocation(operation);
                if (WireActionScope.IsTransparencyActive)
                {
                    return null;
                }

                if (!Plugin.TryGetCoordinator(out var coordinator) || blueprint == null)
                {
                    return null;
                }

                if (!coordinator.RequiresCoordination(blueprint.m_Info, blueprint.List_BuildPos))
                {
                    return null;
                }

                var state = new LifecyclePatchState(coordinator, blueprint.List_BuildPos);
                state.AddScope(TransparencyScope.Create(coordinator, blueprint.m_Info, state.Positions));
                return state;
            }
            catch (Exception error)
            {
                Plugin.LogPatchError(operation, error);
                return null;
            }
        }
    }

    [HarmonyPatch(typeof(BP_Building), nameof(BP_Building.MakeEnableList), new Type[0])]
    internal static class BlueprintMakeEnableListPatch
    {
        private static void Prefix(BP_Building __instance, out LifecyclePatchState __state)
        {
            __state = BlueprintEnableCheckPatch.CreateTransparencyState(
                __instance,
                nameof(BlueprintMakeEnableListPatch));
        }

        private static void Postfix(LifecyclePatchState __state)
        {
            BlueprintSetPatch.DisposeSafely(__state, "蓝图工作标记更新后前景格协调");
        }

        private static Exception Finalizer(Exception __exception, LifecyclePatchState __state)
        {
            BlueprintSetPatch.DisposeSafely(__state, "异常路径恢复蓝图工作标记视图");
            return __exception;
        }
    }

    [HarmonyPatch(typeof(BP_Building), nameof(BP_Building.BuildingUpdate_Call), new[] { typeof(GameUnit) })]
    internal static class BlueprintBuildingUpdatePatch
    {
        private static void Prefix(BP_Building __instance, out LifecyclePatchState __state)
        {
            __state = null;
            try
            {
                Plugin.LogFirstInvocation(nameof(BlueprintBuildingUpdatePatch));
                if (!Plugin.TryGetCoordinator(out var coordinator) || __instance == null)
                {
                    return;
                }

                if (!coordinator.RequiresCoordination(__instance.m_Info, __instance.List_BuildPos))
                {
                    return;
                }

                __state = new LifecyclePatchState(coordinator, __instance.List_BuildPos);
                if (WireOverlayCoordinator.IsWire(__instance.m_Info))
                {
                    __state.AddScope(NodeStateSnapshot.Capture(
                        coordinator.TileManager,
                        __state.Positions));
                }

                __state.AddScope(TransparencyScope.Create(coordinator, __instance.m_Info, __state.Positions));

                if (WireOverlayCoordinator.IsWire(__instance.m_Info))
                {
                    __state.AddScope(WireActionScope.ProtectTiles(__state.Positions));
                }
                else if (OverlayRules.ShouldMaskCompletedWiresDuringCompletion(
                             candidateIsWire: false,
                             candidateAbility: (int)__instance.m_Info.Ability))
                {
                    var targets = new HashSet<Vector2Int>(__state.Positions);
                    __state.AddScope(ScopedListMask<Building_HeavyWire>.RemoveWhere(
                        coordinator.BuildingManager.List_HeavyWire,
                        wire => wire != null && WireOverlayCoordinator.Overlaps(wire.List_BuildPos, targets)));
                }
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("蓝图完工透明视图", error);
                BlueprintSetPatch.DisposeSafely(__state, "恢复未完成的蓝图完工透明视图");
                __state = null;
            }
        }

        private static void Postfix(LifecyclePatchState __state)
        {
            BlueprintSetPatch.DisposeSafely(__state, "蓝图完工后前景格协调");
        }

        private static Exception Finalizer(Exception __exception, LifecyclePatchState __state)
        {
            BlueprintSetPatch.DisposeSafely(__state, "异常路径恢复蓝图完工视图");
            return __exception;
        }
    }

    [HarmonyPatch(typeof(BP_Building), nameof(BP_Building.CancelBP), new Type[0])]
    internal static class BlueprintCancelPatch
    {
        private static void Prefix(BP_Building __instance, out LifecyclePatchState __state)
        {
            __state = null;
            try
            {
                Plugin.LogFirstInvocation(nameof(BlueprintCancelPatch));
                if (Plugin.TryGetCoordinator(out var coordinator) && __instance != null)
                {
                    if (coordinator.RequiresCoordination(__instance.m_Info, __instance.List_BuildPos))
                    {
                        __state = new LifecyclePatchState(coordinator, __instance.List_BuildPos);
                        if (WireOverlayCoordinator.IsWire(__instance.m_Info))
                        {
                            __state.AddScope(NodeStateSnapshot.Capture(
                                coordinator.TileManager,
                                __state.Positions));
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("取消蓝图前状态捕获", error);
            }
        }

        private static void Postfix(LifecyclePatchState __state)
        {
            BlueprintSetPatch.DisposeSafely(__state, "取消蓝图后前景格协调");
        }

        private static Exception Finalizer(Exception __exception, LifecyclePatchState __state)
        {
            BlueprintSetPatch.DisposeSafely(__state, "异常路径取消蓝图协调");
            return __exception;
        }
    }

    [HarmonyPatch(typeof(C_Tile), nameof(C_Tile.DestroyTile),
        new[] { typeof(bool), typeof(bool), typeof(GameUnit) })]
    internal static class TileDestroyProtectionPatch
    {
        private static bool Prefix(C_Tile __instance)
        {
            try
            {
                Plugin.LogFirstInvocation(nameof(TileDestroyProtectionPatch));
                return __instance == null || !WireActionScope.IsTileProtected(__instance.m_X, __instance.m_Y);
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("电线完工时保护前景地块", error);
                return true;
            }
        }
    }
}
