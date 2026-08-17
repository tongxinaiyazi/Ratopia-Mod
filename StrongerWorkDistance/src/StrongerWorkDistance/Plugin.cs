using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using StrongerWorkDistance.Patches;

namespace StrongerWorkDistance
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "cn.ratopia.strongerworkdistance";
        public const string PluginName = "更强大的工作距离";
        public const string PluginVersion = "0.1.0";

        private Harmony _harmony;

        internal static ManualLogSource RuntimeLog { get; private set; }

        private void Awake()
        {
            RuntimeLog = Logger;
            _harmony = new Harmony(PluginGuid);

            try
            {
                var patchType = typeof(SystemMgrAwakePatch);
                Logger.LogDebug($"Patching: {patchType.FullName}");
                _harmony.CreateClassProcessor(patchType).Patch();
                Logger.LogInfo($"{PluginName} {PluginVersion} 补丁安装完成。");
            }
            catch (Exception exception)
            {
                Logger.LogError($"{PluginName} 补丁安装失败，功能已停用：{exception}");
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

            RuntimeLog = null;
        }

        internal static void LogRuntimeInfo(string message)
        {
            RuntimeLog?.LogInfo(message);
        }

        internal static void LogRuntimeError(string message)
        {
            RuntimeLog?.LogError(message);
        }
    }
}
