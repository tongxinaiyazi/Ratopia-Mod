using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Utility.Data;

namespace SharedWarehouse.Patches
{
    [HarmonyPatch(typeof(PlayDataMgr), nameof(PlayDataMgr.Save))]
    internal static class SaveSingleViewPatch
    {
        private static void Prefix(out IDisposable __state)
        {
            __state = SingleViewPatchSupport.Enter();
        }

        private static Exception Finalizer(IDisposable __state, Exception __exception)
        {
            return SingleViewPatchSupport.Exit("保存存档后恢复共享库存", __state, __exception);
        }
    }

    [HarmonyPatch]
    internal static class InventoryAggregateSingleViewPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return RequiredMethod(typeof(BuildingMgr), nameof(BuildingMgr.GetStorageMatNum), typeof(TileType));
            yield return RequiredMethod(typeof(BuildingMgr), nameof(BuildingMgr.GetAllFood));
            yield return RequiredMethod(typeof(BuildingMgr), nameof(BuildingMgr.IsMatEnough), typeof(TileType), typeof(int));
            yield return RequiredMethod(typeof(BuildingMgr), nameof(BuildingMgr.IsMatEnoughByNoBP_Check), typeof(TileType), typeof(int));
            yield return RequiredMethod(
                typeof(LedgerDataPackage),
                nameof(LedgerDataPackage.SetSavableData),
                typeof(uint),
                typeof(int),
                typeof(LedgerSavableData));
        }

        private static void Prefix(out IDisposable __state)
        {
            __state = SingleViewPatchSupport.Enter();
        }

        private static Exception Finalizer(MethodBase __originalMethod, IDisposable __state, Exception __exception)
        {
            return SingleViewPatchSupport.Exit(
                $"库存汇总 {__originalMethod?.DeclaringType?.Name}.{__originalMethod?.Name} 后恢复共享库存",
                __state,
                __exception);
        }

        private static MethodInfo RequiredMethod(Type type, string name, params Type[] parameterTypes)
        {
            return AccessTools.Method(type, name, parameterTypes)
                ?? throw new MissingMethodException(type.FullName, name);
        }
    }

    [HarmonyPatch(typeof(BuildingMgr), nameof(BuildingMgr.Find_ResourceNum), typeof(TileType))]
    internal static class ActiveResourceCountSingleViewPatch
    {
        private static void Prefix(BuildingMgr __instance, out IDisposable __state)
        {
            __state = SingleViewPatchSupport.EnterActiveResourceView(__instance);
        }

        private static Exception Finalizer(IDisposable __state, Exception __exception)
        {
            return SingleViewPatchSupport.Exit(
                "活动仓库资源统计后恢复共享库存",
                __state,
                __exception);
        }
    }

    internal static class SingleViewPatchSupport
    {
        public static IDisposable Enter()
        {
            if (!Plugin.TryGetReadyCoordinator(out var coordinator))
            {
                return null;
            }

            try
            {
                return coordinator.EnterSingleView();
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("进入单仓统计视图", error);
                throw;
            }
        }

        public static IDisposable EnterActiveResourceView(BuildingMgr manager)
        {
            if (!Plugin.TryGetReadyCoordinator(out var coordinator))
            {
                return null;
            }

            try
            {
                return coordinator.EnterActiveResourceView(manager);
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("进入活动仓库资源统计视图", error);
                throw;
            }
        }

        public static Exception Exit(string operation, IDisposable scope, Exception originalException)
        {
            if (scope == null)
            {
                return originalException;
            }

            try
            {
                scope.Dispose();
                return originalException;
            }
            catch (Exception restoreError)
            {
                Plugin.LogPatchError(operation, restoreError);
                return originalException ?? restoreError;
            }
        }
    }
}
