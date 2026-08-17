using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BroadcastStationGlobalCoverage.Patches;
using BroadcastStationGlobalCoverage.Runtime;
using HarmonyLib;

namespace BroadcastStationGlobalCoverage
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "cn.ratopia.broadcaststationglobalcoverage";
        public const string PluginName = "广播站信号覆盖全图";
        public const string PluginVersion = "0.1.1";

        private static readonly IReadOnlyList<Type> PatchTypes = new[]
        {
            typeof(TelevisionSelectionPanelPatch),
            typeof(TelevisionAutomaticSignalPatch)
        };

        private Harmony _harmony;

        private void Awake()
        {
            BroadcastSignalRuntime.Configure(Logger);
            Logger.LogInfo($"已发现 {PluginName} {PluginVersion}，准备安装 {PatchTypes.Count} 个 Harmony 补丁。");

            _harmony = new Harmony(PluginGuid);
            try
            {
                foreach (var patchType in PatchTypes)
                {
                    Logger.LogInfo($"正在安装补丁：{patchType.Name}");
                    _harmony.CreateClassProcessor(patchType).Patch();
                    Logger.LogInfo($"补丁安装成功：{patchType.Name}");
                }

                Logger.LogInfo($"全部 {PatchTypes.Count} 个 Harmony 补丁安装完成，功能已启用。");
            }
            catch (Exception exception)
            {
                Logger.LogError($"补丁安装失败，正在撤销本 Mod 的全部补丁并停用功能：{exception}");
                _harmony.UnpatchSelf();
                _harmony = null;
            }
        }

        private void OnDestroy()
        {
            try
            {
                _harmony?.UnpatchSelf();
            }
            catch (Exception exception)
            {
                Logger.LogError($"卸载 Harmony 补丁时发生异常：{exception}");
            }
            finally
            {
                _harmony = null;
            }
        }
    }
}
