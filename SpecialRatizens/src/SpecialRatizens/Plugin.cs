using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using RatopiaMod;
using SpecialRatizens.Core;
using SpecialRatizens.Patching;

namespace SpecialRatizens
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "cn.ratopia.specialratizens";
        public const string PluginName = "特殊鼠鼠";
        public const string PluginVersion = "0.1.4";

        private Harmony _harmony;
        private ConfigEntry<bool> _enabled;
        private bool _patchingSucceeded;

        internal static Plugin Instance { get; private set; }

        internal static bool Enabled => Instance != null && Instance._enabled != null && Instance._enabled.Value;

        private void Awake()
        {
            Instance = this;
            _enabled = Config.Bind(
                "General",
                "Enabled",
                true,
                "启用特殊鼠鼠的生成与特性效果。关闭时仍注册特性定义，以便读取已有存档。");

            try
            {
                var dataRoot = PluginDataPaths.ResolveDataRoot(typeof(Plugin).Assembly.Location);
                var catalog = SpecialDataCatalog.Load(
                    Path.Combine(dataRoot, "CustomSpecialUnit.csv"),
                    Path.Combine(dataRoot, "CustomCharInfo.csv"),
                    Path.Combine(dataRoot, "Icon"));

                CustomMOD.ConfigureSpecialRatizens(_enabled.Value, dataRoot);
                _harmony = new Harmony(PluginGuid);
                PatchRegistry.InstallAll(_harmony, Logger);
                _patchingSucceeded = true;
                Logger.LogInfo(
                    $"{PluginName} v{PluginVersion} 已加载：{catalog.Ratizens.Count} 名特殊鼠鼠、{catalog.Traits.Count} 个特性；" +
                    $"功能当前{(_enabled.Value ? "开启" : "关闭")}。");
            }
            catch (Exception error)
            {
                _patchingSucceeded = false;
                _harmony?.UnpatchSelf();
                CustomMOD.ResetSpecialRatizensSession();
                Logger.LogError($"特殊鼠鼠初始化失败，已回滚全部补丁并停用：{error}");
            }
        }

        private void OnDestroy()
        {
            try
            {
                CustomMOD.ResetSpecialRatizensSession();
            }
            catch (Exception error)
            {
                Logger.LogWarning($"清理特殊鼠鼠运行时状态失败：{error}");
            }

            _harmony?.UnpatchSelf();
            _patchingSucceeded = false;
            if (ReferenceEquals(Instance, this))
            {
                Instance = null;
            }
        }

        internal static void RunSafely(string operation, Action action)
        {
            var plugin = Instance;
            if (plugin == null || !plugin._patchingSucceeded)
            {
                return;
            }

            try
            {
                action();
            }
            catch (Exception error)
            {
                plugin.Logger.LogError($"特殊鼠鼠补丁 {operation} 执行失败，已隔离异常：{error}");
            }
        }

        internal static void LogPatchError(string operation, Exception error)
        {
            Instance?.Logger.LogError($"特殊鼠鼠补丁 {operation} 执行失败，已回退原版行为：{error}");
        }
    }
}
