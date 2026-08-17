using HarmonyLib;
using PopulationCustomizer.Runtime;

namespace PopulationCustomizer.Patches
{
    [HarmonyPatch(typeof(ProsperityUI), "GetMaxCitizenCount")]
    internal static class CitizenLimitPatch
    {
        private static void Postfix(ref int __result)
        {
            __result = LimitRuntime.ResolveCitizen(__result);
        }
    }

    [HarmonyPatch(typeof(SystemMgr), "GetGBotMaxCount")]
    internal static class RatronLimitPatch
    {
        private static void Postfix(ref int __result)
        {
            __result = LimitRuntime.ResolveRatron(__result);
        }
    }
}
