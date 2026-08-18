using HarmonyLib;

namespace GodViewManagement.Patches
{
    [HarmonyPatch(typeof(BuildMidUI), nameof(BuildMidUI.QueenBtn))]
    internal static class QueenBtnPatch
    {
        private static bool Prefix(BuildMidUI __instance) => !Plugin.ShouldBlockQueenAction(__instance);
    }

    [HarmonyPatch(typeof(BuildMidUI), nameof(BuildMidUI.QueenBtn2))]
    internal static class QueenBtn2Patch
    {
        private static bool Prefix(BuildMidUI __instance) => !Plugin.ShouldBlockQueenAction(__instance);
    }

    [HarmonyPatch(typeof(BuildMidUI), nameof(BuildMidUI.QueenBtn3))]
    internal static class QueenBtn3Patch
    {
        private static bool Prefix(BuildMidUI __instance) => !Plugin.ShouldBlockQueenAction(__instance);
    }

    [HarmonyPatch(typeof(BuildMidUI), nameof(BuildMidUI.QueenBtn4))]
    internal static class QueenBtn4Patch
    {
        private static bool Prefix(BuildMidUI __instance) => !Plugin.ShouldBlockQueenAction(__instance);
    }

    [HarmonyPatch(typeof(BuildMidUI), nameof(BuildMidUI.QueenBtn5))]
    internal static class QueenBtn5Patch
    {
        private static bool Prefix(BuildMidUI __instance) => !Plugin.ShouldBlockQueenAction(__instance);
    }
}
