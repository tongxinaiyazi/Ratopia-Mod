using System;
using HarmonyLib;
using SuperBow.Runtime;

namespace SuperBow.Patches
{
    [HarmonyPatch(typeof(DB_Mgr), "Item_DB_Setting", new Type[0])]
    internal static class ItemDatabasePatch
    {
        private static void Postfix(DB_Mgr __instance)
        {
            RuntimeCatalog.TryApplySafely(__instance);
        }
    }

    [HarmonyPatch(typeof(DB_Mgr), "ItemEnhance_DB_Setting", new Type[0])]
    internal static class ItemEnhanceDatabasePatch
    {
        private static void Postfix(DB_Mgr __instance)
        {
            RuntimeCatalog.TryApplySafely(__instance);
        }
    }
}
