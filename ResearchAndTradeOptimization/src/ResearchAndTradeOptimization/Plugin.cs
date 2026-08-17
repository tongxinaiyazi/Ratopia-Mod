using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ResearchAndTradeOptimization.Core;
using ResearchAndTradeOptimization.Patches;
using UnityEngine;

namespace ResearchAndTradeOptimization
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal const string PluginGuid = "cn.ratopia.unlimitedresearchandtradequeues";
        internal const string PluginName = "研究与贸易优化";
        internal const string PluginVersion = "0.3.0";

        private static ManualLogSource _runtimeLog;
        private Harmony _harmony;
        private ConfigEntry<string> _activeTradeColorEntry;
        private ConfigEntry<string> _infiniteTradeColorEntry;

        private static readonly IReadOnlyList<Type> PatchTypes = new[]
        {
            typeof(ResearchQueueLimitPatch),
            typeof(ResearchQueueViewPatch),
            typeof(ResearchProgressPatch),
            typeof(ResearchRefreshPatch),
            typeof(ResearchRefundPatch),
            typeof(TradeAgreementLimitPatch),
            typeof(TradeLayoutPatch),
            typeof(TradeWorldDetailPatch),
            typeof(TradeResourcePreviewPatch),
            typeof(FullTradeResourceSetPatch),
            typeof(TradeResourceLoadPatch),
            typeof(TradeDetailModifySlotPatch),
            typeof(TradeModifyEventPatch),
            typeof(TradeSheetLayoutEditPatch),
            typeof(TradeSheetDetailSlotEditPatch),
            typeof(TradeSheetSubmitEditPatch),
            typeof(TradeSheetHideEditPatch),
            typeof(TradeSheetCleanUpEditPatch),
            typeof(QuarterlyTradePricePatch)
        };

        internal static Color ActiveTradeHighlightColor { get; private set; }

        internal static Color InfiniteTradeHighlightColor { get; private set; }

        private void Awake()
        {
            _runtimeLog = Logger;
            _harmony = new Harmony(PluginGuid);
            BindConfiguration();

            try
            {
                foreach (var patchType in PatchTypes)
                {
                    _harmony.CreateClassProcessor(patchType).Patch();
                    Logger.LogInfo($"已安装补丁：{patchType.Name}");
                }

                Logger.LogInfo($"{PluginName} v{PluginVersion} 已启用。");
            }
            catch (Exception exception)
            {
                Logger.LogError($"补丁安装失败，本 Mod 已停用：{exception}");
                _harmony.UnpatchSelf();
                _harmony = null;
                enabled = false;
            }
        }

        private void BindConfiguration()
        {
            try
            {
                _activeTradeColorEntry = Config.Bind(
                    TradeResourceStateRules.ConfigSection,
                    TradeResourceStateRules.ActiveTradeColorKey,
                    TradeResourceStateRules.ActiveTradeColorDefault,
                    "国家详情进出口列表中，有限期贸易商品的高亮背景色（RGB，格式 R,G,B，范围 0-255）。");

                _infiniteTradeColorEntry = Config.Bind(
                    TradeResourceStateRules.ConfigSection,
                    TradeResourceStateRules.InfiniteTradeColorKey,
                    TradeResourceStateRules.InfiniteTradeColorDefault,
                    "国家详情进出口列表中，无限期贸易商品的高亮背景色（RGB，格式 R,G,B，范围 0-255）。");

                ActiveTradeHighlightColor = ToUnityColor(
                    _activeTradeColorEntry.Value,
                    TradeResourceStateRules.DefaultHighlightColor);
                InfiniteTradeHighlightColor = ToUnityColor(
                    _infiniteTradeColorEntry.Value,
                    TradeResourceStateRules.DefaultInfiniteHighlightColor);
            }
            catch (Exception exception)
            {
                ActiveTradeHighlightColor = ToUnityColor(
                    null,
                    TradeResourceStateRules.DefaultHighlightColor);
                InfiniteTradeHighlightColor = ToUnityColor(
                    null,
                    TradeResourceStateRules.DefaultInfiniteHighlightColor);
                Logger.LogError($"读取贸易高亮颜色配置失败，已使用默认颜色：{exception}");
            }
        }

        private static Color ToUnityColor(
            string text,
            TradeHighlightColor fallback)
        {
            var parsed = TradeResourceStateRules.ParseColorOrDefault(text, fallback);
            return new Color(
                parsed.Red / 255f,
                parsed.Green / 255f,
                parsed.Blue / 255f,
                1f);
        }

        private void OnDestroy()
        {
            if (_harmony != null)
            {
                _harmony.UnpatchSelf();
                _harmony = null;
            }
        }

        internal static void LogRuntimeInfo(string message)
        {
            _runtimeLog?.LogInfo(message);
        }

        internal static void LogRuntimeError(string message, Exception exception)
        {
            _runtimeLog?.LogError($"{message} {exception}");
        }

        internal static void LogRuntimeError(string message)
        {
            _runtimeLog?.LogError(message);
        }
    }
}
