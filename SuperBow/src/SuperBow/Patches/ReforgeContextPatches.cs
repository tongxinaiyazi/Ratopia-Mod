using System;
using HarmonyLib;
using SuperBow.Runtime;

namespace SuperBow.Patches
{
    [HarmonyPatch(
        typeof(BuildMidUI),
        "ItemDetail_Open",
        new[] { typeof(ItemInfo), typeof(bool), typeof(bool), typeof(int) })]
    internal static class ItemDetailReforgeContextPatch
    {
        private static void Prefix(ItemInfo __0)
        {
            RuntimeCatalog.SetReforgeContextSafely(__0);
        }
    }

    [HarmonyPatch(
        typeof(T_Queen),
        "ItemEnhance",
        new[] { typeof(ItemInfo), typeof(int), typeof(Res_Ability) })]
    internal static class ItemEnhanceReforgeContextPatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(ItemInfo __0)
        {
            RuntimeCatalog.SetReforgeContextSafely(__0);
        }
    }
}
