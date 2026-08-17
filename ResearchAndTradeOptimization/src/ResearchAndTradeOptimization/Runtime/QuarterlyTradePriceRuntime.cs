using System;
using CasselGames.Diplomatic.Data;
using HarmonyLib;
using ResearchAndTradeOptimization.Core;

namespace ResearchAndTradeOptimization.Runtime
{
    internal static class QuarterlyTradePriceRuntime
    {
        private static bool _loggedFirstRefresh;

        internal static void RefreshPrices(
            DiplomaticCountryPackage package,
            TimeSpan now)
        {
            try
            {
                var totalDays = (int)now.TotalDays;
                if (!TradeAgreementRules.IsQuarterBoundary(
                        totalDays,
                        GetDayOfQuarter()))
                {
                    return;
                }

                var updated = 0;
                var countries = package?.CountryArray;
                if (countries == null)
                {
                    return;
                }

                for (var countryIndex = 0; countryIndex < countries.Length; countryIndex++)
                {
                    var country = countries[countryIndex];
                    if (country == null)
                    {
                        continue;
                    }

                    for (var sheetIndex = 0; sheetIndex < country.Sheets.Count; sheetIndex++)
                    {
                        var sheet = country.Sheets[sheetIndex];
                        if (sheet == null ||
                            !TradeAgreementRules.IsEditableAgreement(
                                (int)sheet.Resource,
                                (int)sheet.State))
                        {
                            continue;
                        }

                        var resource = country.GetResourceDataOrNull(sheet.Resource);
                        if (resource == null)
                        {
                            continue;
                        }

                        sheet.SetTradeValue(resource.NowValue);
                        updated++;
                    }
                }

                if (!_loggedFirstRefresh)
                {
                    _loggedFirstRefresh = true;
                    Plugin.LogRuntimeInfo(
                        $"季度市场价首次刷新完成：第 {totalDays} 天，更新 {updated} 份普通商品协议。只影响后续交易。");
                }
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError("季度市场价刷新失败；本次保留原协议价格。", exception);
            }
        }

        private static int GetDayOfQuarter()
        {
            var definesType = AccessTools.TypeByName("Defines");
            var field = definesType == null
                ? null
                : AccessTools.Field(definesType, "DayOfQuarter");
            return field == null ? 0 : (int)field.GetValue(null);
        }
    }
}
