using System;
using HarmonyLib;

namespace SharedWarehouse.Patches
{
    [HarmonyPatch(typeof(Building_Storage), nameof(Building_Storage.BuildingSet))]
    internal static class StorageBuiltPatch
    {
        private static void Postfix(Building_Storage __instance)
        {
            if (!Plugin.TryGetReadyCoordinator(out var coordinator)
                || !StorageInventoryCoordinator.IsTarget(__instance))
            {
                return;
            }

            try
            {
                coordinator.Attach(__instance);
            }
            catch (Exception error)
            {
                Plugin.Instance?.MarkSessionDirty(error, $"接入仓库 #{__instance.m_ID}");
            }
        }
    }

    [HarmonyPatch(typeof(Building_Storage), nameof(Building_Storage.BuildingDemolition), typeof(bool))]
    internal static class StorageDemolitionPatch
    {
        private static void Prefix(Building_Storage __instance, out DemolitionState __state)
        {
            __state = new DemolitionState
            {
                IsTarget = StorageInventoryCoordinator.IsTarget(__instance),
            };
            if (!__state.IsTarget || !Plugin.TryGetReadyCoordinator(out var coordinator))
            {
                return;
            }

            try
            {
                __state.Detached = coordinator.DetachForDemolition(__instance);
            }
            catch (Exception error)
            {
                Plugin.LogPatchError($"拆除前分离仓库 #{__instance.m_ID}", error);
                throw;
            }
        }

        private static Exception Finalizer(
            Building_Storage __instance,
            DemolitionState __state,
            Exception __exception)
        {
            if (!__state.IsTarget || !Plugin.TryGetReadyCoordinator(out var coordinator))
            {
                return __exception;
            }

            try
            {
                if (__exception == null)
                {
                    coordinator.Remove(__instance);
                }
                else if (__state.Detached)
                {
                    coordinator.Attach(__instance);
                }
            }
            catch (Exception error)
            {
                Plugin.Instance?.MarkSessionDirty(error, $"完成仓库 #{__instance.m_ID} 的拆除同步");
            }

            return __exception;
        }

        internal struct DemolitionState
        {
            public bool IsTarget;
            public bool Detached;
        }
    }
}
