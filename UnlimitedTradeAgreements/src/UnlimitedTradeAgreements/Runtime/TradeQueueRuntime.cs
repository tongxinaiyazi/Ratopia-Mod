using System;
using CasselGames.Diplomatic.Data;
using CasselGames.Diplomatic.UI;
using HarmonyLib;
using TMPro;
using UnlimitedTradeAgreements.Core;

namespace UnlimitedTradeAgreements.Runtime
{
    internal static class TradeQueueRuntime
    {
        private static readonly AccessTools.FieldRef<DiplomaticTradeLayoutUI, DiplomaticTradeSlotUI> NewSlot =
            AccessTools.FieldRefAccess<DiplomaticTradeLayoutUI, DiplomaticTradeSlotUI>("_newSlotUI");

        private static readonly AccessTools.FieldRef<DiplomaticWorldDetailUI, DiplomaticCountryData> WorldCountry =
            AccessTools.FieldRefAccess<DiplomaticWorldDetailUI, DiplomaticCountryData>("_country");

        private static readonly AccessTools.FieldRef<DiplomaticWorldDetailUI, TextMeshProUGUI> WorldTradeValue =
            AccessTools.FieldRefAccess<DiplomaticWorldDetailUI, TextMeshProUGUI>("_tradeAgreementValueText");

        private static bool _loggedExpandedDisplay;

        internal static int GetVisibleSlotCount(DiplomaticCountryData country)
        {
            try
            {
                var count = country?.GetGoodsTradeCount() ?? 0;
                var visible = TradeQueueRules.GetVisibleSlotCount(count);
                if (visible > TradeQueueRules.VanillaVisibleSlotCount && !_loggedExpandedDisplay)
                {
                    _loggedExpandedDisplay = true;
                    Plugin.LogRuntimeInfo($"贸易界面首次显示超过原版容量：{visible} 个槽位。");
                }

                return visible;
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError("计算贸易槽位数量失败，已回退为原版 7 个槽位。", exception);
                return TradeQueueRules.VanillaVisibleSlotCount;
            }
        }

        internal static void UpdateLayoutLabel(
            DiplomaticTradeLayoutUI layout,
            DiplomaticCountryData country)
        {
            try
            {
                var slot = NewSlot(layout);
                if (slot != null && country != null)
                {
                    slot.SetSlotText(TradeQueueRules.GetUnlimitedCountLabel(
                        country.NowTradeAgreementCount));
                }
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError("更新贸易列表无限数量文本失败。", exception);
            }
        }

        internal static void UpdateWorldDetailLabel(DiplomaticWorldDetailUI detail)
        {
            try
            {
                var country = WorldCountry(detail);
                var valueText = WorldTradeValue(detail);
                if (country != null && valueText != null)
                {
                    valueText.text = TradeQueueRules.GetUnlimitedCountLabel(
                        country.NowTradeAgreementCount);
                }
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError("更新国家详情无限数量文本失败。", exception);
            }
        }
    }
}
