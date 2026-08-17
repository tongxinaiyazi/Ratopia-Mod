using System.Collections.Generic;
using CasselGames.Diplomatic.Data;
using CasselGames.Diplomatic.UI;
using HarmonyLib;
using UnlimitedTradeAgreements.Patching;
using UnlimitedTradeAgreements.Runtime;

namespace UnlimitedTradeAgreements.Patches
{
    [HarmonyPatch(typeof(DiplomaticCountryData), "IsFullTradeAgreement")]
    internal static class TradeAgreementLimitPatch
    {
        internal static void Postfix(ref bool __result)
        {
            __result = false;
        }
    }

    [HarmonyPatch(typeof(DiplomaticTradeLayoutUI), "UpdateSlot")]
    internal static class TradeLayoutPatch
    {
        internal static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return TradeLayoutTranspiler.Rewrite(instructions);
        }

        internal static void Postfix(
            DiplomaticTradeLayoutUI __instance,
            DiplomaticCountryData __0)
        {
            TradeQueueRuntime.UpdateLayoutLabel(__instance, __0);
        }
    }

    [HarmonyPatch(typeof(DiplomaticWorldDetailUI), "Refresh")]
    internal static class TradeWorldDetailPatch
    {
        internal static void Postfix(DiplomaticWorldDetailUI __instance)
        {
            TradeQueueRuntime.UpdateWorldDetailLabel(__instance);
        }
    }
}
