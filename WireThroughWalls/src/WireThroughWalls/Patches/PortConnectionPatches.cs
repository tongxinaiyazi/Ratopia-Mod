using System;
using System.Collections.Generic;
using HarmonyLib;
using WireThroughWalls.Runtime;

namespace WireThroughWalls.Patches
{
    [HarmonyPatch(typeof(BuildingMgr), nameof(BuildingMgr.NewConnectCheck),
        new[] { typeof(ElecPort), typeof(float) })]
    internal static class NewConnectCheckPatch
    {
        private static void Postfix(BuildingMgr __instance, ElecPort __0)
        {
            try
            {
                Plugin.LogFirstInvocation(nameof(NewConnectCheckPatch));
                Plugin.ObservePortRegistration(__instance, __0, nameof(NewConnectCheckPatch));
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("登记同格电力端口", error);
            }
        }
    }

    [HarmonyPatch(typeof(BuildingMgr), nameof(BuildingMgr.DeleteConnectCheck),
        new[] { typeof(ElecPort) })]
    internal static class DeleteSingleConnectCheckPatch
    {
        private static void Prefix(BuildingMgr __instance, ElecPort __0, out PortRemovalState __state)
        {
            __state = null;
            try
            {
                Plugin.LogFirstInvocation(nameof(DeleteSingleConnectCheckPatch));
                __state = Plugin.CapturePortRemoval(__instance, __0);
                __state?.SetStage(nameof(DeleteSingleConnectCheckPatch));
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("记录待删除单端口", error);
            }
        }

        private static void Postfix(PortRemovalState __state)
        {
            DisposeSafely(__state, "单端口删除后提升同格幸存端口");
        }

        private static Exception Finalizer(Exception __exception, PortRemovalState __state)
        {
            if (__exception != null)
            {
                CancelSafely(__state, "异常路径保留未确认删除的单端口");
            }
            else
            {
                DisposeSafely(__state, "完成单端口删除后的幸存端口提升");
            }

            return __exception;
        }

        internal static void DisposeSafely(IDisposable state, string operation)
        {
            try
            {
                state?.Dispose();
            }
            catch (Exception error)
            {
                Plugin.LogPatchError(operation, error);
            }
        }

        internal static void CancelSafely(PortRemovalState state, string operation)
        {
            try
            {
                state?.Cancel();
            }
            catch (Exception error)
            {
                Plugin.LogPatchError(operation, error);
            }
        }
    }

    [HarmonyPatch(typeof(BuildingMgr), nameof(BuildingMgr.DeleteConnectCheck),
        new[] { typeof(int), typeof(List<ElecPort>) })]
    internal static class DeleteManyConnectCheckPatch
    {
        private static void Prefix(BuildingMgr __instance, List<ElecPort> __1, out PortRemovalState __state)
        {
            __state = null;
            try
            {
                Plugin.LogFirstInvocation(nameof(DeleteManyConnectCheckPatch));
                __state = Plugin.CapturePortRemoval(__instance, __1);
                __state?.SetStage(nameof(DeleteManyConnectCheckPatch));
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("记录待删除多端口", error);
            }
        }

        private static void Postfix(PortRemovalState __state)
        {
            DeleteSingleConnectCheckPatch.DisposeSafely(__state, "多端口删除后提升同格幸存端口");
        }

        private static Exception Finalizer(Exception __exception, PortRemovalState __state)
        {
            if (__exception != null)
            {
                DeleteSingleConnectCheckPatch.CancelSafely(
                    __state,
                    "异常路径保留未确认删除的多端口");
            }
            else
            {
                DeleteSingleConnectCheckPatch.DisposeSafely(
                    __state,
                    "完成多端口删除后的幸存端口提升");
            }

            return __exception;
        }
    }
}
