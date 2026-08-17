using System;
using CasselGames.Diplomatic.Data;
using HarmonyLib;
using RatopiaMod.YunQing.All.Core;

namespace RatopiaMod.YunQing.All.Patches
{
    [HarmonyPatch(typeof(DiplomaticExchangeData), "get_DefaultDarValue")]
    internal static class BankExchangePatch
    {
        [HarmonyPostfix]
        internal static void Postfix(ref float __result)
        {
            var originalResult = __result;
            try
            {
                Plugin.LogPatchInvocationOnce(
                    "bank-exchange-invoked",
                    "DiplomaticExchangeData.get_DefaultDarValue");
                __result = BankExchangeRules.Apply(
                    originalResult,
                    Plugin.CurrentBankExchangeMultiplier);
            }
            catch (Exception error)
            {
                __result = originalResult;
                Plugin.LogPatchErrorOnce(
                    "bank-exchange",
                    "DiplomaticExchangeData.DefaultDarValue 补丁执行失败",
                    error);
            }
        }
    }
}
