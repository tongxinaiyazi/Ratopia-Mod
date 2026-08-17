using System;
using HarmonyLib;
using UnityEngine;
using WireThroughWalls.Runtime;

namespace WireThroughWalls.Patches
{
    [HarmonyPatch(typeof(Building_HeavyWire), nameof(Building_HeavyWire.BuildingSet),
        new[] { typeof(BuildInfo), typeof(Vector2), typeof(int) })]
    internal static class HeavyWireBuildingSetPatch
    {
        private static void Prefix(BuildInfo info, Vector2 pos, out LifecyclePatchState __state)
        {
            __state = null;
            try
            {
                Plugin.LogFirstInvocation(nameof(HeavyWireBuildingSetPatch));
                if (!Plugin.TryGetCoordinator(out var coordinator))
                {
                    return;
                }

                var positions = coordinator.GetBuildPositions(info, pos);
                __state = new LifecyclePatchState(coordinator, positions);
                __state.AddScope(NodeStateSnapshot.Capture(coordinator.TileManager, positions));
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("电线建成前节点状态捕获", error);
                BlueprintSetPatch.DisposeSafely(__state, "恢复电线建成前节点状态");
                __state = null;
            }
        }

        private static void Postfix(Building_HeavyWire __instance, LifecyclePatchState __state)
        {
            try
            {
                __state?.AddPositions(__instance != null ? __instance.List_BuildPos : null);
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("电线建成后位置采集", error);
            }
            finally
            {
                BlueprintSetPatch.DisposeSafely(__state, "电线建成后恢复前景节点");
            }
        }

        private static Exception Finalizer(Exception __exception, LifecyclePatchState __state)
        {
            BlueprintSetPatch.DisposeSafely(__state, "异常路径恢复电线建成节点");
            return __exception;
        }
    }

    [HarmonyPatch(typeof(Building_HeavyWire), nameof(Building_HeavyWire.BuildingDemolition),
        new[] { typeof(bool) })]
    internal static class HeavyWireDemolitionPatch
    {
        private static void Prefix(Building_HeavyWire __instance, out LifecyclePatchState __state)
        {
            __state = null;
            try
            {
                Plugin.LogFirstInvocation(nameof(HeavyWireDemolitionPatch));
                if (!Plugin.TryGetCoordinator(out var coordinator) || __instance == null)
                {
                    return;
                }

                __state = new LifecyclePatchState(coordinator, __instance.List_BuildPos);
                __state.AddScope(NodeStateSnapshot.Capture(coordinator.TileManager, __state.Positions));
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("拆除电线前节点状态捕获", error);
                BlueprintSetPatch.DisposeSafely(__state, "恢复拆除电线节点状态");
                __state = null;
            }
        }

        private static void Postfix(LifecyclePatchState __state)
        {
            BlueprintSetPatch.DisposeSafely(__state, "拆除电线后恢复前景节点");
        }

        private static Exception Finalizer(Exception __exception, LifecyclePatchState __state)
        {
            BlueprintSetPatch.DisposeSafely(__state, "异常路径恢复拆除电线节点");
            return __exception;
        }
    }

    internal static class HeavyWireNodeState
    {
        internal static LifecyclePatchState Capture(Building_HeavyWire wire, string operation)
        {
            try
            {
                Plugin.LogFirstInvocation(operation);
                if (!Plugin.TryGetCoordinator(out var coordinator) || wire == null)
                {
                    return null;
                }

                var state = new LifecyclePatchState(coordinator, wire.List_BuildPos);
                state.AddScope(NodeStateSnapshot.Capture(coordinator.TileManager, state.Positions));
                return state;
            }
            catch (Exception error)
            {
                Plugin.LogPatchError(operation, error);
                return null;
            }
        }
    }

    [HarmonyPatch(typeof(Building_HeavyWire), nameof(Building_HeavyWire.LoadSetting2),
        new[] { typeof(BuildingData) })]
    internal static class HeavyWireLoadPatch
    {
        private static void Prefix(Building_HeavyWire __instance, out LifecyclePatchState __state)
        {
            __state = HeavyWireNodeState.Capture(__instance, nameof(HeavyWireLoadPatch));
        }

        private static void Postfix(LifecyclePatchState __state)
        {
            BlueprintSetPatch.DisposeSafely(__state, "电线读档后恢复前景节点");
        }

        private static Exception Finalizer(Exception __exception, LifecyclePatchState __state)
        {
            BlueprintSetPatch.DisposeSafely(__state, "异常路径恢复电线读档节点");
            return __exception;
        }
    }

    [HarmonyPatch(typeof(Building_HeavyWire), nameof(Building_HeavyWire.BuildingWorkingStop),
        new[] { typeof(bool) })]
    internal static class HeavyWireWorkStopPatch
    {
        private static void Prefix(Building_HeavyWire __instance, out LifecyclePatchState __state)
        {
            __state = HeavyWireNodeState.Capture(__instance, nameof(HeavyWireWorkStopPatch));
        }

        private static void Postfix(LifecyclePatchState __state)
        {
            BlueprintSetPatch.DisposeSafely(__state, "电线停工后恢复前景节点");
        }

        private static Exception Finalizer(Exception __exception, LifecyclePatchState __state)
        {
            BlueprintSetPatch.DisposeSafely(__state, "异常路径恢复电线停工节点");
            return __exception;
        }
    }

    [HarmonyPatch(typeof(Building_HeavyWire), nameof(Building_HeavyWire.BuildWorkResume), new Type[0])]
    internal static class HeavyWireWorkResumePatch
    {
        private static void Prefix(Building_HeavyWire __instance, out LifecyclePatchState __state)
        {
            __state = HeavyWireNodeState.Capture(__instance, nameof(HeavyWireWorkResumePatch));
        }

        private static void Postfix(LifecyclePatchState __state)
        {
            BlueprintSetPatch.DisposeSafely(__state, "电线复工后恢复前景节点");
        }

        private static Exception Finalizer(Exception __exception, LifecyclePatchState __state)
        {
            BlueprintSetPatch.DisposeSafely(__state, "异常路径恢复电线复工节点");
            return __exception;
        }
    }
}
