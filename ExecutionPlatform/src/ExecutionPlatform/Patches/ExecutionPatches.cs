using System;
using ExecutionPlatform.Runtime;
using HarmonyLib;
using UnityEngine;

namespace ExecutionPlatform.Patches
{
    [HarmonyPatch(typeof(DB_Mgr), "Build_DB_Setting")]
    internal static class BuildDatabasePatch
    {
        private static void Postfix(DB_Mgr __instance)
        {
            ExecutionRuntime.RegisterDatabase(__instance);
        }
    }

    [HarmonyPatch(typeof(DB_Mgr), "IsLockBuilding", typeof(int))]
    internal static class UnlockBuildingPatch
    {
        private static bool Prefix(int _index, ref bool __result)
        {
            if (ExecutionRuntime.IsEnabled && _index == ExecutionCatalog.RuntimeBuildingValue)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Func), "LoadSprite", typeof(string))]
    [HarmonyPriority(Priority.First)]
    internal static class SpriteLookupPatch
    {
        private static void Prefix(ref string name)
        {
            name = ExecutionVisuals.ResolveSpritePath(name);
        }
    }

    [HarmonyPatch(typeof(Helpers), "IsMagicianBuilding", typeof(BuildInfo))]
    [HarmonyPriority(Priority.First)]
    internal static class MagicianBuildingPatch
    {
        private static bool Prefix(BuildInfo _info, ref bool __result)
        {
            if (!ExecutionVisuals.RequiresOrdinaryFrame(_info))
            {
                return true;
            }

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(BuildingMgr), "BuildSet",
        new[] { typeof(BuildingName), typeof(Vector2), typeof(int) })]
    internal static class BuildSetPatch
    {
        private static bool Prefix(
            BuildingMgr __instance,
            BuildingName name,
            Vector2 pos,
            int _att_num,
            ref Building __result)
        {
            if (!ExecutionPoolBridge.TryBuild(__instance, name, pos, _att_num, out var building))
            {
                return true;
            }

            __result = building;
            return false;
        }
    }

    [HarmonyPatch(typeof(BuildingMgr), "AddToPool", typeof(Building))]
    internal static class AddToPoolPatch
    {
        private static void Prefix(Building _building, out BuildInfo __state)
        {
            __state = ExecutionPoolBridge.PrepareRecycle(_building);
        }

        private static Exception Finalizer(Exception __exception, Building _building, BuildInfo __state)
        {
            ExecutionPoolBridge.RestoreAfterRecycle(_building, __state);
            return __exception;
        }
    }

    [HarmonyPatch(typeof(T_Citizen), "JobSet", typeof(Building))]
    internal static class CitizenJobPatch
    {
        private static void Prefix(T_Citizen __instance)
        {
            ExecutionRuntime.OnJobChanging(__instance);
        }

        private static void Postfix(T_Citizen __instance, Building _building)
        {
            ExecutionRuntime.OnJobSet(__instance, _building);
        }
    }

    [HarmonyPatch(typeof(T_Citizen), "JobFire", typeof(bool))]
    internal static class CitizenJobFirePatch
    {
        private static void Prefix(T_Citizen __instance)
        {
            ExecutionRuntime.OnJobChanging(__instance);
        }
    }

    [HarmonyPatch(typeof(T_Citizen), "UpdateFunction")]
    [HarmonyPriority(Priority.First)]
    internal static class CitizenUpdatePatch
    {
        private static bool Prefix(T_Citizen __instance)
        {
            return !ExecutionRuntime.TryHandleUpdate(__instance);
        }
    }

    [HarmonyPatch(typeof(PlayDataMgr), "BeforeLoad")]
    internal static class BeforeLoadPatch
    {
        private static void Prefix()
        {
            ExecutionRuntime.ClearTransient("切换或读取存档");
        }
    }
}
