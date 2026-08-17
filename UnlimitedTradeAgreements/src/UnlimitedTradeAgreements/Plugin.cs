using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnlimitedTradeAgreements.Patches;

namespace UnlimitedTradeAgreements
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInIncompatibility(IncompatiblePluginGuid)]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal const string PluginGuid = "cn.ratopia.unlimitedtradeagreements";
        internal const string PluginName = "贸易站去除最大队列限制";
        internal const string PluginVersion = "0.1.0";
        internal const string IncompatiblePluginGuid =
            "cn.ratopia.unlimitedresearchandtradequeues";

        private static readonly IReadOnlyList<Type> PatchTypes = new[]
        {
            typeof(TradeAgreementLimitPatch),
            typeof(TradeLayoutPatch),
            typeof(TradeWorldDetailPatch)
        };

        private static ManualLogSource _runtimeLog;
        private Harmony _harmony;

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
    }
}
