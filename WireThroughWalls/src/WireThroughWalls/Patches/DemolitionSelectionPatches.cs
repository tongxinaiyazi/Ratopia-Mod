using System;
using HarmonyLib;
using UnityEngine;
using WireThroughWalls.Core;
using WireThroughWalls.Runtime;

namespace WireThroughWalls.Patches
{
    [HarmonyPatch(typeof(MiningBox), "Update", new Type[0])]
    internal static class MiningBoxDemolitionScopePatch
    {
        private static void Prefix(MiningBox __instance, out IDisposable __state)
        {
            __state = null;
            try
            {
                Plugin.LogFirstInvocation(nameof(MiningBoxDemolitionScopePatch));
                if (__instance != null && __instance.m_Mode == MiningBoxMode.Demolition)
                {
                    __state = WireActionScope.EnterDemolitionSelection();
                }
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("进入拆除选择作用域", error);
            }
        }

        private static void Postfix(IDisposable __state)
        {
            DisposeSafely(__state, "退出拆除选择作用域");
        }

        private static Exception Finalizer(Exception __exception, IDisposable __state)
        {
            DisposeSafely(__state, "异常路径退出拆除选择作用域");
            return __exception;
        }

        private static void DisposeSafely(IDisposable state, string operation)
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
    }

    [HarmonyPatch(typeof(BuildingMgr), nameof(BuildingMgr.FindBuildingByBpos),
        new[] { typeof(UnityEngine.Vector2Int) })]
    internal static class WireFirstBuildingLookupPatch
    {
        private static bool Prefix(BuildingMgr __instance, UnityEngine.Vector2Int _pos, ref Building __result)
        {
            if (!WireActionScope.IsDemolitionSelectionActive)
            {
                return true;
            }

            try
            {
                Plugin.LogFirstInvocation(nameof(WireFirstBuildingLookupPatch));
                if (__instance == null)
                {
                    return true;
                }

                var foreground = __instance.List_Building?.Find(building =>
                    building != null &&
                    !WireOverlayCoordinator.IsWire(building.m_Info) &&
                    building.List_BuildPos != null &&
                    building.List_BuildPos.Contains(_pos));
                var wire = __instance.List_HeavyWire?.Find(candidate =>
                    candidate != null &&
                    candidate.List_BuildPos != null &&
                    candidate.List_BuildPos.Contains(_pos));
                var altPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

                switch (DemolitionSelectionRules.GetPreference(
                            foreground != null,
                            wire != null,
                            altPressed))
                {
                    case DemolitionTargetPreference.Foreground:
                        __result = foreground;
                        return false;
                    case DemolitionTargetPreference.Wire:
                        __result = wire;
                        return false;
                    default:
                        return true;
                }
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("拆除模式选择重叠对象", error);
            }

            return true;
        }
    }
}
