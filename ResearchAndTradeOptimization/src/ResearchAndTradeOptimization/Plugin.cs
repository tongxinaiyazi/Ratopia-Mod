using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ResearchAndTradeOptimization.Patches;

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

        private void Awake()
        {
            _runtimeLog = Logger;
            _harmony = new Harmony(PluginGuid);

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
