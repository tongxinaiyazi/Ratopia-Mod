using System.Collections.Generic;
using BroadcastStationGlobalCoverage.Runtime;
using HarmonyLib;

namespace BroadcastStationGlobalCoverage.Patches
{
    [HarmonyPatch(typeof(UI_StorageSelect), nameof(UI_StorageSelect.TelevisionSelectSet), new[]
    {
        typeof(Building)
    })]
    internal static class TelevisionSelectionPanelPatch
    {
        private static void Postfix(List<Building> ___List_Storage)
        {
            BroadcastSignalRuntime.EnsureManualCandidates(___List_Storage);
        }
    }
}
