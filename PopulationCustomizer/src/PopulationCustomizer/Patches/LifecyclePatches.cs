using HarmonyLib;
using CasselGames.UI;

namespace PopulationCustomizer.Patches
{
    [HarmonyPatch(typeof(PlayDataMgr), nameof(PlayDataMgr.LoadData))]
    internal static class GameDataLoadPatch
    {
        private static void Postfix()
        {
            Plugin.BeginGameSession();
        }
    }

    [HarmonyPatch(typeof(PlayDataMgr), nameof(PlayDataMgr.BeforeLoad))]
    internal static class GameDataResetPatch
    {
        private static void Prefix()
        {
            Plugin.ResetGameSession();
        }
    }

    [HarmonyPatch(typeof(StatisticsCitizenListUI), "Initialize")]
    internal static class StatisticsCitizenListUiPatch
    {
        private static void Postfix(StatisticsCitizenListUI __instance)
        {
            Plugin.AttachStatisticsCitizenListUi(__instance);
        }
    }
}
