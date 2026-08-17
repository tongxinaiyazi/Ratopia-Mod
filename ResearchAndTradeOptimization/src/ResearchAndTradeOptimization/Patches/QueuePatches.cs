using System.Collections.Generic;
using CasselGames.Diplomatic.Asset;
using CasselGames.Diplomatic.Data;
using CasselGames.Diplomatic.UI;
using HarmonyLib;
using ResearchAndTradeOptimization.Patching;
using ResearchAndTradeOptimization.Runtime;

namespace ResearchAndTradeOptimization.Patches
{
    [HarmonyPatch(typeof(Tech_RPInfo), "UpgradBtn")]
    internal static class ResearchQueueLimitPatch
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return ResearchReservationEnqueueTranspiler.Rewrite(
                ResearchQueueTranspiler.Rewrite(instructions));
        }
    }

    [HarmonyPatch(typeof(ResearchingGroup), "ResearchingGroupSet")]
    internal static class ResearchQueueViewPatch
    {
        internal static void Prefix(ResearchingGroup __instance)
        {
            ResearchQueueRuntime.EnsureCurrentQueueVisible(__instance);
        }

        internal static void Postfix(ResearchingGroup __instance)
        {
            ResearchQueueLayoutRuntime.ApplySingleRowSummary(__instance);
        }
    }

    [HarmonyPatch(typeof(ResearchUI), "UpdateUpgradeNode")]
    internal static class ResearchProgressPatch
    {
        internal static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return ResearchProgressTranspiler.Rewrite(instructions);
        }
    }

    [HarmonyPatch(typeof(UpgradeNode), "Refresh")]
    internal static class ResearchRefreshPatch
    {
        internal static bool Prefix(UpgradeNode __instance)
        {
            return ResearchReservationRuntime.CanRefresh(__instance);
        }
    }

    [HarmonyPatch(typeof(Tech_RPInfo), "RemoveUpgradeNode")]
    internal static class ResearchRefundPatch
    {
        internal static void Prefix()
        {
            ResearchReservationRuntime.BeginRefundOperation();
        }

        internal static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return ResearchRefundTranspiler.Rewrite(instructions);
        }

        internal static System.Exception Finalizer(System.Exception __exception)
        {
            ResearchReservationRuntime.EndRefundOperation();
            return __exception;
        }
    }

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
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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
        internal static void Prefix(DiplomaticWorldDetailUI __instance)
        {
            TradeResourcePreviewRuntime.ApplyCompactDetailLayout(__instance);
        }

        internal static void Postfix(DiplomaticWorldDetailUI __instance)
        {
            TradeQueueRuntime.UpdateWorldDetailLabel(__instance);
        }
    }

    [HarmonyPatch(typeof(DiplomaticWorldDetailResourceLayoutUI), "SetData")]
    internal static class TradeResourcePreviewPatch
    {
        internal static void Prefix(
            ref KeyValuePair<int, TileType>[] arr)
        {
            TradeResourcePreviewRuntime.LimitVisibleItems(ref arr);
        }
    }

    [HarmonyPatch(typeof(DiplomaticCountryData), "SetTradeResource")]
    internal static class FullTradeResourceSetPatch
    {
        internal static bool Prefix(
            DiplomaticCountryData __instance,
            DiplomaticAsset asset)
        {
            return !FullTradeResourceRuntime.TryApplyBothDirections(
                __instance,
                asset);
        }
    }

    [HarmonyPatch(typeof(DiplomaticCountryData), "SetSavableData")]
    internal static class TradeResourceLoadPatch
    {
        internal static void Postfix(
            DiplomaticCountryData __instance,
            DiplomaticAsset __0)
        {
            FullTradeResourceRuntime.RefreshAfterLoad(__instance, __0);
        }
    }

    [HarmonyPatch(typeof(DiplomaticTradeDetailUI), "Refresh")]
    internal static class TradeDetailModifySlotPatch
    {
        internal static void Postfix(DiplomaticTradeDetailUI __instance)
        {
            TradeAgreementEditRuntime.UpdateDetailSlot(__instance);
        }
    }

    [HarmonyPatch(typeof(DiplomaticUI), "OnTradeDetailEvent")]
    internal static class TradeModifyEventPatch
    {
        internal static bool Prefix(
            DiplomaticUI __instance,
            DiplomaticCountryData cData,
            DiplomaticCountryTradeSheetData sData,
            TypeTradeOrder typeTradeOrder)
        {
            if (typeTradeOrder != TypeTradeOrder.Modify)
            {
                return true;
            }

            TradeAgreementEditRuntime.OpenEditor(__instance, cData, sData);
            return false;
        }
    }

    [HarmonyPatch(
        typeof(DiplomaticTradeSheetLayoutUI),
        "SetData",
        new[]
        {
            typeof(DiplomaticCountryData),
            typeof(DiplomaticCountryTradeSheetData),
            typeof(TypeTradeSheetCategory),
            typeof(bool)
        })]
    internal static class TradeSheetLayoutEditPatch
    {
        internal static void Postfix(DiplomaticTradeSheetLayoutUI __instance)
        {
            TradeAgreementEditRuntime.ConfigureSheetLayout(__instance);
        }
    }

    [HarmonyPatch(
        typeof(DiplomaticTradeSheetDetailSlotUI),
        "SetData",
        new[]
        {
            typeof(DiplomaticCountryData),
            typeof(DiplomaticCountryTradeSheetData),
            typeof(TypeTradeSheet),
            typeof(bool)
        })]
    internal static class TradeSheetDetailSlotEditPatch
    {
        internal static void Postfix(
            DiplomaticTradeSheetDetailSlotUI __instance,
            TypeTradeSheet typeTradeSheet)
        {
            TradeAgreementEditRuntime.ConfigureDetailSlot(
                __instance,
                typeTradeSheet);
        }
    }

    [HarmonyPatch(typeof(DiplomaticTradeSheetUI), "OnSubmitedEvent")]
    internal static class TradeSheetSubmitEditPatch
    {
        internal static bool Prefix(
            DiplomaticTradeSheetUI __instance,
            DiplomaticCountryTradeSheetData sData)
        {
            return !TradeAgreementEditRuntime.HandleSubmittedData(
                __instance,
                sData);
        }
    }

    [HarmonyPatch(typeof(DiplomaticTradeSheetUI), "Hide")]
    internal static class TradeSheetHideEditPatch
    {
        internal static void Postfix(DiplomaticTradeSheetUI __instance)
        {
            TradeAgreementEditRuntime.ClearSession(__instance);
        }
    }

    [HarmonyPatch(typeof(DiplomaticTradeSheetUI), "CleanUp")]
    internal static class TradeSheetCleanUpEditPatch
    {
        internal static void Postfix(DiplomaticTradeSheetUI __instance)
        {
            TradeAgreementEditRuntime.ClearSession(__instance);
        }
    }

    [HarmonyPatch(typeof(DiplomaticCountryPackage), "RunProcessDaily")]
    internal static class QuarterlyTradePricePatch
    {
        internal static void Postfix(
            DiplomaticCountryPackage __instance,
            System.TimeSpan __0)
        {
            QuarterlyTradePriceRuntime.RefreshPrices(__instance, __0);
        }
    }
}
