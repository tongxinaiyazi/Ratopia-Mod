using HarmonyLib;
using SuperBow.Core;

namespace SuperBow.Patches
{
    [HarmonyPatch(
        typeof(Helpers),
        "GetToolTipString",
        new[] { typeof(Res_Ability), typeof(float), typeof(bool) })]
    internal static class TooltipPatch
    {
        private static bool Prefix(Res_Ability __0, float __1, ref string __result)
        {
            if (!TooltipRules.IsBleedMarker((int)__0, __1))
            {
                return true;
            }

            __result = TooltipRules.BleedText;
            return false;
        }
    }

    [HarmonyPatch(
        typeof(Helpers),
        "GetToolTipString2",
        new[] { typeof(Res_Ability), typeof(float) })]
    internal static class Tooltip2Patch
    {
        private static bool Prefix(Res_Ability __0, float __1, ref string __result)
        {
            if (!TooltipRules.IsBleedMarker((int)__0, __1))
            {
                return true;
            }

            __result = TooltipRules.BleedText;
            return false;
        }
    }
}
