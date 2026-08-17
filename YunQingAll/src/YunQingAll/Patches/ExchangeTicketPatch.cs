using System;
using System.Collections.Generic;
using CasselGames.Diplomatic.Data;
using Extensions;
using HarmonyLib;
using RatopiaMod.YunQing.All.Core;

namespace RatopiaMod.YunQing.All.Patches
{
    [HarmonyPatch(typeof(DiplomaticExchangeData), "GetRandomTicket")]
    internal static class ExchangeTicketPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(
            DiplomaticExchangeData __instance,
            ref DiplomaticExchangeTicketData __result)
        {
            var originalResult = __result;
            try
            {
                Plugin.LogPatchInvocationOnce(
                    "exchange-ticket-invoked",
                    "DiplomaticExchangeData.GetRandomTicket");
                var tickets = Traverse.Create(__instance)
                    .Field("_exchangeTicketList")
                    .GetValue<List<DiplomaticExchangeTicketData>>();

                __result = ExchangeTicketSelector.SelectOrOriginal(
                    originalResult,
                    tickets,
                    Plugin.CurrentExchangeRateMode,
                    ticket => ticket.ExchangeRate,
                    values => values.Shuffle(),
                    error => Plugin.LogPatchErrorOnce(
                        "exchange-ticket-selection",
                        "选择自定义汇率券失败",
                        error));

                Plugin.LogExchangeRateChange(originalResult.ExchangeRate, __result.ExchangeRate);
            }
            catch (Exception error)
            {
                __result = originalResult;
                Plugin.LogPatchErrorOnce(
                    "exchange-ticket-patch",
                    "DiplomaticExchangeData.GetRandomTicket 补丁执行失败",
                    error);
            }
        }
    }
}
