using System;
using HarmonyLib;
using UnityEngine;
using WireThroughWalls.Runtime;

namespace WireThroughWalls.Patches
{
    [HarmonyPatch(typeof(MiningBox), nameof(MiningBox.BuildEnableCheck), new Type[0])]
    internal static class MiningBoxBuildEnableCheckPatch
    {
        private static void Prefix(
            BuildInfo ___m_BuildInfo,
            Transform ___Tf,
            out TransparencyScope __state)
        {
            __state = null;
            try
            {
                Plugin.LogFirstInvocation(nameof(MiningBoxBuildEnableCheckPatch));
                if (!Plugin.TryGetCoordinator(out var coordinator))
                {
                    return;
                }

                var positions = coordinator.GetPlacementPositions(___m_BuildInfo, ___Tf);
                if (!coordinator.RequiresCoordination(___m_BuildInfo, positions))
                {
                    return;
                }

                __state = TransparencyScope.Create(coordinator, ___m_BuildInfo, positions);
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("建造可用性透明视图", error);
                DisposeSafely(__state, "恢复建造可用性透明视图");
                __state = null;
            }
        }

        private static void Postfix(MiningBox __instance, TransparencyScope __state)
        {
            try
            {
                if (__state != null && __state.DuplicateCompletedWire)
                {
                    __instance.m_BuildEnable = false;
                }
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("重复电线检查", error);
            }
            finally
            {
                DisposeSafely(__state, "恢复建造可用性透明视图");
            }
        }

        private static Exception Finalizer(Exception __exception, TransparencyScope __state)
        {
            DisposeSafely(__state, "异常路径恢复建造可用性透明视图");
            return __exception;
        }

        private static void DisposeSafely(IDisposable scope, string operation)
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
}
