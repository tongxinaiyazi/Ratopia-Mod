using System;
using System.Linq;
using BepInEx;
using CasselGames.UI;
using HarmonyLib;
using PopulationCustomizer.Core;
using PopulationCustomizer.Runtime;

namespace PopulationCustomizer
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "cn.ratopia.populationcustomizer";
        public const string PluginName = "人口自定义";
        public const string PluginVersion = "0.1.3";

        private Harmony _harmony;
        private PopulationUiController _uiController;
        private bool _patchingSucceeded;
        private bool _malformedWarningLogged;

        internal static Plugin Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            LimitRuntime.Reset();
            _uiController = new PopulationUiController(Logger, ApplySettings, RestoreVanilla);

            try
            {
                _harmony = new Harmony(PluginGuid);
                PatchAllWithDiagnostics();
                _patchingSucceeded = true;
                Logger.LogInfo($"{PluginName} v{PluginVersion} 已加载；各存档默认沿用原版上限。");
            }
            catch (Exception error)
            {
                _patchingSucceeded = false;
                _harmony?.UnpatchSelf();
                _uiController?.Dispose();
                _uiController = null;
                LimitRuntime.Reset();
                Logger.LogError($"Harmony 补丁安装失败，已撤销本插件全部补丁并保持原版行为：{error}");
            }
        }

        private void OnDestroy()
        {
            _patchingSucceeded = false;
            try
            {
                _uiController?.Dispose();
            }
            catch (Exception error)
            {
                Logger.LogWarning($"清理人口设置界面时出现异常：{error.Message}");
            }

            _uiController = null;
            LimitRuntime.Reset();
            _harmony?.UnpatchSelf();
            if (ReferenceEquals(Instance, this))
            {
                Instance = null;
            }
        }

        internal static void BeginGameSession()
        {
            var plugin = Instance;
            if (plugin == null || !plugin._patchingSucceeded)
            {
                return;
            }

            try
            {
                plugin._uiController?.ResetSession();
                var settings = SaveSettingsStore.LoadCurrent(out var malformed);
                LimitRuntime.Apply(settings);
                if (malformed && !plugin._malformedWarningLogged)
                {
                    plugin.Logger.LogWarning("当前存档的人口自定义数据无效，已安全回退原版上限；数据不会被自动覆盖。");
                    plugin._malformedWarningLogged = true;
                }

                if (!malformed)
                {
                    plugin._malformedWarningLogged = false;
                }

                plugin.Logger.LogInfo(
                    $"已从当前存档载入人口上限：鼠民={(settings.CitizenEnabled ? settings.CitizenLimit.ToString() : "原版")}，" +
                    $"机器鼠={(settings.RatronEnabled ? settings.RatronLimit.ToString() : "原版")}。");
            }
            catch (Exception error)
            {
                LimitRuntime.Reset();
                plugin.Logger.LogError($"读取当前存档人口设置失败，已回退原版上限：{error}");
            }
        }

        internal static void ResetGameSession()
        {
            var plugin = Instance;
            if (plugin == null)
            {
                return;
            }

            try
            {
                plugin._uiController?.ResetSession();
            }
            catch (Exception error)
            {
                plugin.Logger.LogWarning($"切换存档时清理人口设置界面失败：{error.Message}");
            }

            LimitRuntime.Reset();
            plugin._malformedWarningLogged = false;
        }

        internal static void AttachStatisticsCitizenListUi(StatisticsCitizenListUI listUi)
        {
            var plugin = Instance;
            if (plugin == null || !plugin._patchingSucceeded || listUi == null)
            {
                return;
            }

            try
            {
                plugin._uiController?.Attach(listUi);
            }
            catch (Exception error)
            {
                plugin.Logger.LogError($"在鼠民名单创建人口上限入口失败；上限补丁仍保持可用：{error}");
            }
        }

        private bool ApplySettings(LimitSettings settings, out string message)
        {
            try
            {
                if (!SaveSettingsStore.TrySaveCurrent(settings))
                {
                    message = "当前存档尚未就绪，设置没有应用。";
                    return false;
                }

                LimitRuntime.Apply(settings);
                RefreshOriginalPopulationText();
                Logger.LogInfo($"已应用当前存档人口上限：鼠民={(settings.CitizenEnabled ? settings.CitizenLimit.ToString() : "原版")}，机器鼠={(settings.RatronEnabled ? settings.RatronLimit.ToString() : "原版")}。等待玩家正常保存游戏后落盘。");
                message = "已应用到当前存档；请正常保存游戏以写入磁盘。";
                return true;
            }
            catch (Exception error)
            {
                message = "应用失败，已保留原来的设置。";
                Logger.LogError($"写入当前存档人口设置失败：{error}");
                return false;
            }
        }

        private bool RestoreVanilla(out string message)
        {
            try
            {
                if (!SaveSettingsStore.TryRemoveCurrent())
                {
                    message = "当前存档尚未就绪，无法恢复。";
                    return false;
                }

                LimitRuntime.Reset();
                RefreshOriginalPopulationText();
                Logger.LogInfo("已恢复当前存档的原版人口上限；请正常保存游戏以写入磁盘。");
                message = "已恢复原版上限；请正常保存游戏以写入磁盘。";
                return true;
            }
            catch (Exception error)
            {
                message = "恢复失败，已保留原来的设置。";
                Logger.LogError($"移除当前存档人口设置失败：{error}");
                return false;
            }
        }

        private static void RefreshOriginalPopulationText()
        {
            var game = GameMgr.Instance;
            game?._EcoMgr?.m_CitizenUI?.CitizenTxtUpdate();
            game?._SysMgr?.GetGBotMaxCount();
        }

        private void PatchAllWithDiagnostics()
        {
            var patchTypes = typeof(Plugin).Assembly
                .GetTypes()
                .Where(type => type.GetCustomAttributes(typeof(HarmonyPatch), false).Length > 0)
                .OrderBy(type => type.FullName)
                .ToArray();

            foreach (var patchType in patchTypes)
            {
                Logger.LogDebug($"正在安装 Harmony 补丁：{patchType.FullName}");
                _harmony.CreateClassProcessor(patchType).Patch();
                Logger.LogDebug($"Harmony 补丁安装完成：{patchType.FullName}");
            }
        }
    }
}
