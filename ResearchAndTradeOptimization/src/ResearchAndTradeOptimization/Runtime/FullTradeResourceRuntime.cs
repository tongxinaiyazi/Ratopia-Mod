using System;
using System.Collections.Generic;
using System.Linq;
using CasselGames.Diplomatic.Asset;
using CasselGames.Diplomatic.Data;
using HarmonyLib;
using ResearchAndTradeOptimization.Core;

namespace ResearchAndTradeOptimization.Runtime
{
    internal static class FullTradeResourceRuntime
    {
        private static readonly AccessTools.FieldRef<
            DiplomaticCountryData,
            DiplomaticCountryRawData> Raw =
                AccessTools.FieldRefAccess<
                    DiplomaticCountryData,
                    DiplomaticCountryRawData>("_raw");

        private static readonly AccessTools.FieldRef<
            DiplomaticCountryData,
            KeyValuePair<int, TileType>[]> CountryToHometownArray =
                AccessTools.FieldRefAccess<
                    DiplomaticCountryData,
                    KeyValuePair<int, TileType>[]>("_countryToHometownArray");

        private static readonly AccessTools.FieldRef<
            DiplomaticCountryData,
            KeyValuePair<int, TileType>[]> HometownToCountryArray =
                AccessTools.FieldRefAccess<
                    DiplomaticCountryData,
                    KeyValuePair<int, TileType>[]>("_hometownToCountryArray");

        private static readonly AccessTools.FieldRef<
            DiplomaticCountryData,
            List<TileType>> AllCountryToHometownList =
                AccessTools.FieldRefAccess<
                    DiplomaticCountryData,
                    List<TileType>>("_allCountryToHometownList");

        private static readonly AccessTools.FieldRef<
            DiplomaticCountryData,
            List<TileType>> AllHometownToCountryList =
                AccessTools.FieldRefAccess<
                    DiplomaticCountryData,
                    List<TileType>>("_allHometownToCountryList");

        private static readonly AccessTools.FieldRef<
            DiplomaticCountryData,
            List<TileType>> TradableCountryToHometownList =
                AccessTools.FieldRefAccess<
                    DiplomaticCountryData,
                    List<TileType>>("_countryToHometownList");

        private static readonly AccessTools.FieldRef<
            DiplomaticCountryData,
            List<TileType>> TradableHometownToCountryList =
                AccessTools.FieldRefAccess<
                    DiplomaticCountryData,
                    List<TileType>>("_hometownToCountryList");

        private static readonly AccessTools.FieldRef<
            DiplomaticCountryData,
            List<TileType>> UsedResources =
                AccessTools.FieldRefAccess<
                    DiplomaticCountryData,
                    List<TileType>>("_useResources");

        private static readonly AccessTools.FieldRef<
            DiplomaticTradeResourceGroupAsset,
            List<TileType>> GloballyUsedResources =
                AccessTools.FieldRefAccess<
                    DiplomaticTradeResourceGroupAsset,
                    List<TileType>>("_usedResourceList");

        private static bool _loggedFirstBuild;

        internal static bool TryApplyBothDirections(
            DiplomaticCountryData country,
            DiplomaticAsset asset)
        {
            try
            {
                if (country == null || asset?.TradeResGroupAsset == null)
                {
                    return false;
                }

                var raw = Raw(country);
                if (raw == null)
                {
                    return false;
                }

                var countryBuckets = BuildBuckets(
                    asset.TradeResGroupAsset,
                    raw.CountryToHometownArray);
                var hometownBuckets = BuildBuckets(
                    asset.TradeResGroupAsset,
                    raw.HometownToCountryArray);
                if (!FullTradeResourceRules.CanExpandAll(
                        countryBuckets,
                        hometownBuckets))
                {
                    return false;
                }

                var result = FullTradeResourceRules.BuildBothDirections(
                    countryBuckets,
                    hometownBuckets,
                    asset.TradeResGroupAsset.IgnoreResourceList
                        .Select(resource => (int)resource)
                        .ToArray());

                var countryArray = ToGameArray(result.CountryToHometown);
                var hometownArray = ToGameArray(result.HometownToCountry);
                var countryResources = result.CountryToHometown
                    .Select(item => (TileType)item.Value)
                    .ToArray();
                var hometownResources = result.HometownToCountry
                    .Select(item => (TileType)item.Value)
                    .ToArray();
                if (countryResources.Intersect(hometownResources).Any())
                {
                    throw new InvalidOperationException(
                        $"{country.Key} 的进口与出口完整商品池存在重复资源。");
                }

                // 所有临时结果都构造并验证成功后才原子写回原版字段。
                CountryToHometownArray(country) = countryArray;
                HometownToCountryArray(country) = hometownArray;
                ReplaceContents(AllCountryToHometownList(country), countryResources);
                ReplaceContents(AllHometownToCountryList(country), hometownResources);
                country.CalculateTradableCountryToHometownResources();
                country.CalculateTradableHometownToCountryResources();

                if (!_loggedFirstBuild &&
                    countryResources.Length + hometownResources.Length > 0)
                {
                    _loggedFirstBuild = true;
                    Plugin.LogRuntimeInfo(
                        $"首次生成城市完整贸易商品池：{country.Key}，进口 {countryResources.Length} 项，出口 {hometownResources.Length} 项；两方向无重复。");
                }

                return true;
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError(
                    "联合生成城市进口/出口完整商品池失败，已回退原版随机抽选。",
                    exception);
                return false;
            }
        }

        internal static void RefreshAfterLoad(
            DiplomaticCountryData country,
            DiplomaticAsset asset)
        {
            try
            {
                if (country != null && asset != null)
                {
                    var raw = Raw(country);
                    if (raw == null)
                    {
                        return;
                    }

                    var countryBuckets = BuildBuckets(
                        asset.TradeResGroupAsset,
                        raw.CountryToHometownArray);
                    var hometownBuckets = BuildBuckets(
                        asset.TradeResGroupAsset,
                        raw.HometownToCountryArray);
                    if (FullTradeResourceRules.CanExpandAll(
                            countryBuckets,
                            hometownBuckets))
                    {
                        country.SetTradeResource(asset);
                    }
                    else if (FullTradeResourceRules.NeedsVanillaRepair(
                                 countryBuckets,
                                 CountryToHometownArray(country)?.Length ?? 0,
                                 hometownBuckets,
                                 HometownToCountryArray(country)?.Length ?? 0))
                    {
                        RepairLegacyGlobalPool(
                            country,
                            asset,
                            countryBuckets,
                            hometownBuckets);
                    }
                }
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError(
                    "读档后重建城市完整贸易商品池失败；当前城市暂时保留存档中的商品池。",
                    exception);
            }
        }

        private static void RepairLegacyGlobalPool(
            DiplomaticCountryData country,
            DiplomaticAsset asset,
            TradeResourceBucket[] countryBuckets,
            TradeResourceBucket[] hometownBuckets)
        {
            CountryTradeSnapshot snapshot = null;
            try
            {
                snapshot = CaptureSnapshot(country, asset.TradeResGroupAsset);
                var previousCountryCount = snapshot.CountryToHometownArray?.Length ?? 0;
                var previousHometownCount = snapshot.HometownToCountryArray?.Length ?? 0;

                country.RemakeTradeData(asset);

                var currentCountryCount = CountryToHometownArray(country)?.Length ?? 0;
                var currentHometownCount = HometownToCountryArray(country)?.Length ?? 0;
                if (FullTradeResourceRules.NeedsVanillaRepair(
                        countryBuckets,
                        currentCountryCount,
                        hometownBuckets,
                        currentHometownCount))
                {
                    throw new InvalidOperationException(
                        "原版重抽后贸易商品数量仍超过原版 PickCount 上限。");
                }

                Plugin.LogRuntimeInfo(
                    $"已修复旧存档中的超长全局贸易商品池：{country.Key}，" +
                    $"进口 {previousCountryCount}->{currentCountryCount} 项，" +
                    $"出口 {previousHometownCount}->{currentHometownCount} 项；后续继续使用原版周期抽选。");
            }
            catch (Exception exception)
            {
                if (snapshot != null)
                {
                    try
                    {
                        RestoreSnapshot(country, asset.TradeResGroupAsset, snapshot);
                    }
                    catch (Exception restoreException)
                    {
                        Plugin.LogRuntimeError(
                            $"恢复 {country?.Key ?? "未知城市"} 的贸易商品池快照失败。",
                            restoreException);
                    }
                }

                Plugin.LogRuntimeError(
                    $"修复 {country?.Key ?? "未知城市"} 的旧存档超长全局贸易商品池失败；已保留原存档数据。",
                    exception);
            }
        }

        private static CountryTradeSnapshot CaptureSnapshot(
            DiplomaticCountryData country,
            DiplomaticTradeResourceGroupAsset groupAsset)
        {
            return new CountryTradeSnapshot(
                CloneArray(CountryToHometownArray(country)),
                CloneArray(HometownToCountryArray(country)),
                CopyList(AllCountryToHometownList(country), "_allCountryToHometownList"),
                CopyList(AllHometownToCountryList(country), "_allHometownToCountryList"),
                CopyList(TradableCountryToHometownList(country), "_countryToHometownList"),
                CopyList(TradableHometownToCountryList(country), "_hometownToCountryList"),
                CopyList(UsedResources(country), "_useResources"),
                CopyOptionalList(GloballyUsedResources(groupAsset)));
        }

        private static void RestoreSnapshot(
            DiplomaticCountryData country,
            DiplomaticTradeResourceGroupAsset groupAsset,
            CountryTradeSnapshot snapshot)
        {
            CountryToHometownArray(country) = CloneArray(snapshot.CountryToHometownArray);
            HometownToCountryArray(country) = CloneArray(snapshot.HometownToCountryArray);
            ReplaceContents(AllCountryToHometownList(country), snapshot.AllCountryToHometown);
            ReplaceContents(AllHometownToCountryList(country), snapshot.AllHometownToCountry);
            ReplaceContents(TradableCountryToHometownList(country), snapshot.TradableCountryToHometown);
            ReplaceContents(TradableHometownToCountryList(country), snapshot.TradableHometownToCountry);
            ReplaceContents(UsedResources(country), snapshot.UsedResources);
            RestoreOptionalGlobalUsageList(groupAsset, snapshot.GloballyUsedResources);
        }

        private static KeyValuePair<int, TileType>[] CloneArray(
            KeyValuePair<int, TileType>[] source)
        {
            return source == null
                ? null
                : (KeyValuePair<int, TileType>[])source.Clone();
        }

        private static TileType[] CopyList(
            IEnumerable<TileType> source,
            string fieldName)
        {
            if (source == null)
            {
                throw new InvalidOperationException($"原版贸易字段 {fieldName} 尚未初始化。");
            }

            return source.ToArray();
        }

        private static TileType[] CopyOptionalList(IEnumerable<TileType> source)
        {
            return source?.ToArray();
        }

        private static void RestoreOptionalGlobalUsageList(
            DiplomaticTradeResourceGroupAsset groupAsset,
            TileType[] values)
        {
            if (values == null)
            {
                GloballyUsedResources(groupAsset) = null;
                return;
            }

            var target = GloballyUsedResources(groupAsset);
            if (target == null)
            {
                GloballyUsedResources(groupAsset) = new List<TileType>(values);
                return;
            }

            ReplaceContents(target, values);
        }

        private static TradeResourceBucket[] BuildBuckets(
            DiplomaticTradeResourceGroupAsset groupAsset,
            DiplomaticCountryTradeRawData[] rawBuckets)
        {
            if (rawBuckets == null)
            {
                return Array.Empty<TradeResourceBucket>();
            }

            var result = new List<TradeResourceBucket>();
            for (var index = 0; index < rawBuckets.Length; index++)
            {
                var raw = rawBuckets[index];
                if (!groupAsset.TryGetData(raw.TradeResGroupKey, out var group))
                {
                    throw new InvalidOperationException(
                        $"找不到原版贸易资源组：{raw.TradeResGroupKey}");
                }

                result.Add(new TradeResourceBucket(
                    raw.TradeResGroupKey,
                    raw.ProsperityValue,
                    raw.PickCount,
                    (group.Resources ?? Array.Empty<TileType>())
                        .Select(resource => (int)resource)
                        .ToArray(),
                    group.IsGlobal));
            }

            return result.ToArray();
        }

        private static KeyValuePair<int, TileType>[] ToGameArray(
            IEnumerable<TradeResourceSelection> selections)
        {
            return selections
                .Select(item => new KeyValuePair<int, TileType>(
                    item.Key,
                    (TileType)item.Value))
                .ToArray();
        }

        private static void ReplaceContents(
            ICollection<TileType> target,
            IEnumerable<TileType> values)
        {
            if (target == null)
            {
                throw new InvalidOperationException("原版贸易商品列表尚未初始化。");
            }

            target.Clear();
            foreach (var value in values)
            {
                target.Add(value);
            }
        }

        private sealed class CountryTradeSnapshot
        {
            internal CountryTradeSnapshot(
                KeyValuePair<int, TileType>[] countryToHometownArray,
                KeyValuePair<int, TileType>[] hometownToCountryArray,
                TileType[] allCountryToHometown,
                TileType[] allHometownToCountry,
                TileType[] tradableCountryToHometown,
                TileType[] tradableHometownToCountry,
                TileType[] usedResources,
                TileType[] globallyUsedResources)
            {
                CountryToHometownArray = countryToHometownArray;
                HometownToCountryArray = hometownToCountryArray;
                AllCountryToHometown = allCountryToHometown;
                AllHometownToCountry = allHometownToCountry;
                TradableCountryToHometown = tradableCountryToHometown;
                TradableHometownToCountry = tradableHometownToCountry;
                UsedResources = usedResources;
                GloballyUsedResources = globallyUsedResources;
            }

            internal KeyValuePair<int, TileType>[] CountryToHometownArray { get; }

            internal KeyValuePair<int, TileType>[] HometownToCountryArray { get; }

            internal TileType[] AllCountryToHometown { get; }

            internal TileType[] AllHometownToCountry { get; }

            internal TileType[] TradableCountryToHometown { get; }

            internal TileType[] TradableHometownToCountry { get; }

            internal TileType[] UsedResources { get; }

            internal TileType[] GloballyUsedResources { get; }
        }
    }
}
