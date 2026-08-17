using BroadcastStationGlobalCoverage.Runtime;
using HarmonyLib;

namespace BroadcastStationGlobalCoverage.Patches
{
    [HarmonyPatch(typeof(Building_ElecBandstand), nameof(Building_ElecBandstand.Building_Update2), new[]
    {
        typeof(float)
    })]
    internal static class TelevisionAutomaticSignalPatch
    {
        private static void Postfix(Building_ElecBandstand __instance)
        {
            BroadcastSignalRuntime.EnsureAutomaticSource(__instance);
        }
    }
}
