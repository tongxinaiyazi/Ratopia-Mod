using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace RatopiaMod.YunQing.All
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "RatopiaMod.YunQing.YunQingAll";
        public const string PluginName = "YunQingAll";
        public const string PluginVersion = "2.2.0";

        internal const string CommonConfigSection = "Common";
        internal const string GuiConfigSection = "GUI";
        internal const string FishConfigKey = "IsActiveFishDrownInTheWater";
        internal const string ExchangeModeConfigKey = "CustomExchangeRateMode";
        internal const string BankMultiplierConfigKey = "BankExchangeMultiplier";
        internal const string GuiToggleKeyConfigKey = "GuiToggleKey";
        internal const bool DefaultFishFeatureEnabled = true;
        internal const ExchangeRateMode DefaultExchangeRateMode = ExchangeRateMode.COMMON;
        internal const BankExchangeMultiplier DefaultBankExchangeMultiplier = BankExchangeMultiplier.X1;
        internal const int DefaultGuiToggleKeyCode = 290;

        private readonly HashSet<string> _reportedErrors = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _reportedInvocations = new HashSet<string>(StringComparer.Ordinal);
        private ConfigEntry<bool> _fishFeatureEnabled;
        private ConfigEntry<ExchangeRateMode> _exchangeRateMode;
        private ConfigEntry<BankExchangeMultiplier> _bankExchangeMultiplier;
        private ConfigEntry<KeyboardShortcut> _guiToggleKey;
        private Harmony _harmony;
        private bool _patchesActive;
        private bool _showGui;
        private Rect _windowRect = new Rect(20f, 20f, 420f, 360f);

        internal static Plugin Instance { get; private set; }

        internal static bool FishFeatureEnabled =>
            Instance != null
            && Instance._patchesActive
            && Instance._fishFeatureEnabled != null
            && Instance._fishFeatureEnabled.Value;

        internal static ExchangeRateMode CurrentExchangeRateMode =>
            Instance?._exchangeRateMode?.Value ?? DefaultExchangeRateMode;

        internal static BankExchangeMultiplier CurrentBankExchangeMultiplier =>
            Instance?._bankExchangeMultiplier?.Value ?? DefaultBankExchangeMultiplier;

        private void Awake()
        {
            Instance = this;
            BindConfiguration();

            try
            {
                _harmony = new Harmony(PluginGuid);
                PatchAllWithDiagnostics();
                _patchesActive = true;
                Logger.LogInfo(
                    $"{PluginName} v{PluginVersion} 已加载；BepInEx 5 迁移版不包含未提供源码的 CheatPanelLocalizer。按 F9 打开控制面板。");
            }
            catch (Exception error)
            {
                _patchesActive = false;
                _harmony?.UnpatchSelf();
                Logger.LogError($"Harmony 补丁安装失败，已撤销本插件全部补丁并停用：{error}");
            }
        }

        private void Update()
        {
            if (!_patchesActive || _guiToggleKey == null)
            {
                return;
            }

            try
            {
                if (_guiToggleKey.Value.IsDown())
                {
                    _showGui = !_showGui;
                }
            }
            catch (Exception error)
            {
                LogErrorOnce("gui-toggle", "读取控制面板快捷键失败", error);
            }
        }

        private void OnGUI()
        {
            if (!_patchesActive || !_showGui)
            {
                return;
            }

            try
            {
                _windowRect = GUILayout.Window(
                    0,
                    _windowRect,
                    DrawGuiWindow,
                    $"YunQing Mod 控制面板 (v{PluginVersion})");
            }
            catch (Exception error)
            {
                _showGui = false;
                LogErrorOnce("gui-render", "绘制控制面板失败，已关闭面板", error);
            }
        }

        private void OnDestroy()
        {
            _patchesActive = false;
            _harmony?.UnpatchSelf();
            if (ReferenceEquals(Instance, this))
            {
                Instance = null;
            }
        }

        internal static void LogPatchErrorOnce(string key, string operation, Exception error)
        {
            Instance?.LogErrorOnce(key, operation, error);
        }

        internal static void LogPatchInvocationOnce(string key, string target)
        {
            var plugin = Instance;
            if (plugin == null)
            {
                return;
            }

            try
            {
                lock (plugin._reportedInvocations)
                {
                    if (!plugin._reportedInvocations.Add(key))
                    {
                        return;
                    }
                }

                plugin.Logger.LogInfo($"Harmony 补丁首次执行：{target}");
            }
            catch
            {
                // Logging must never change game behavior.
            }
        }

        internal static void LogExchangeRateChange(float before, float after)
        {
            Instance?.Logger.LogDebug($"汇率券修改：{before} -> {after}");
        }

        private void BindConfiguration()
        {
            _fishFeatureEnabled = Config.Bind(
                CommonConfigSection,
                FishConfigKey,
                DefaultFishFeatureEnabled,
                "鱼会自行淹死在水里功能：默认开启");
            _exchangeRateMode = Config.Bind(
                CommonConfigSection,
                ExchangeModeConfigKey,
                DefaultExchangeRateMode,
                "自定义汇率券功能：【正汇率|正汇率最大值|官方正常值|负汇率|负汇率最大值】，默认官方正常值");
            _bankExchangeMultiplier = Config.Bind(
                CommonConfigSection,
                BankMultiplierConfigKey,
                DefaultBankExchangeMultiplier,
                "银行兑换倍数：x1(默认) | x10 | x100 | x500");
            _guiToggleKey = Config.Bind(
                GuiConfigSection,
                GuiToggleKeyConfigKey,
                new KeyboardShortcut((KeyCode)DefaultGuiToggleKeyCode),
                "控制面板开关快捷键（默认F9）");
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
                Logger.LogInfo($"正在安装 Harmony 补丁：{patchType.FullName}");
                _harmony.CreateClassProcessor(patchType).Patch();
                Logger.LogInfo($"Harmony 补丁安装完成：{patchType.FullName}");
            }
        }

        private void LogErrorOnce(string key, string operation, Exception error)
        {
            lock (_reportedErrors)
            {
                if (!_reportedErrors.Add(key))
                {
                    return;
                }
            }

            Logger.LogError($"{operation}；已回退到原版行为：{error}");
        }

        private void DrawGuiWindow(int windowId)
        {
            GUILayout.BeginVertical();
            GUILayout.Space(10f);
            GUILayout.Label("━━━ 鱼淹死在水里功能 ━━━");

            GUILayout.BeginHorizontal();
            GUILayout.Label("开关控制：");
            if (GUILayout.Button(
                _fishFeatureEnabled.Value ? "◉ 关闭" : "○ 打开",
                GUILayout.Width(100f)))
            {
                _fishFeatureEnabled.Value = !_fishFeatureEnabled.Value;
            }

            GUILayout.EndHorizontal();
            GUILayout.Label($"   配置项当前状态：{(_fishFeatureEnabled.Value ? "打开" : "关闭")}");

            GUILayout.Space(15f);
            GUILayout.Label("━━━ 手动控制汇率券 ━━━");
            GUILayout.BeginHorizontal();
            DrawExchangeRateButton("正汇率", ExchangeRateMode.POSITIVE);
            DrawExchangeRateButton("正汇率最大", ExchangeRateMode.POSITIVE_MAX);
            DrawExchangeRateButton("官方正常值", ExchangeRateMode.COMMON);
            DrawExchangeRateButton("负汇率", ExchangeRateMode.NEGATIVE);
            DrawExchangeRateButton("负汇率最大", ExchangeRateMode.NEGATIVE_MAX);
            GUILayout.EndHorizontal();
            GUILayout.Label($"   配置项当前状态：{GetExchangeRateModeLabel(_exchangeRateMode.Value)}");

            GUILayout.Space(15f);
            GUILayout.Label("━━━ 银行兑换倍数 ━━━");
            GUILayout.BeginHorizontal();
            DrawMultiplierButton("x1", BankExchangeMultiplier.X1);
            DrawMultiplierButton("x10", BankExchangeMultiplier.X10);
            DrawMultiplierButton("x100", BankExchangeMultiplier.X100);
            DrawMultiplierButton("x500", BankExchangeMultiplier.X500);
            GUILayout.EndHorizontal();
            GUILayout.Label($"   配置项当前状态：x{(int)_bankExchangeMultiplier.Value}");

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void DrawExchangeRateButton(string label, ExchangeRateMode mode)
        {
            if (GUILayout.Button(label, GetExchangeRateButtonStyle(mode)))
            {
                _exchangeRateMode.Value = mode;
            }
        }

        private void DrawMultiplierButton(string label, BankExchangeMultiplier multiplier)
        {
            if (GUILayout.Button(label, GetMultiplierButtonStyle(multiplier)))
            {
                _bankExchangeMultiplier.Value = multiplier;
            }
        }

        private GUIStyle GetExchangeRateButtonStyle(ExchangeRateMode mode)
        {
            var style = new GUIStyle(GUI.skin.button);
            if (_exchangeRateMode.Value == mode)
            {
                style.normal.textColor = Color.green;
            }

            return style;
        }

        private GUIStyle GetMultiplierButtonStyle(BankExchangeMultiplier multiplier)
        {
            var style = new GUIStyle(GUI.skin.button);
            if (_bankExchangeMultiplier.Value == multiplier)
            {
                style.normal.textColor = Color.green;
            }

            return style;
        }

        private static string GetExchangeRateModeLabel(ExchangeRateMode mode)
        {
            switch (mode)
            {
                case ExchangeRateMode.POSITIVE:
                    return "正汇率";
                case ExchangeRateMode.POSITIVE_MAX:
                    return "正汇率最大值";
                case ExchangeRateMode.COMMON:
                    return "官方正常值";
                case ExchangeRateMode.NEGATIVE:
                    return "负汇率";
                case ExchangeRateMode.NEGATIVE_MAX:
                    return "负汇率最大值";
                default:
                    return "未知";
            }
        }
    }
}
