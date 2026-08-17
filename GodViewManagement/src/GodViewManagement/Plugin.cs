using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using CasselGames.Input;
using GodViewManagement.Runtime;
using HarmonyLib;
using UnityEngine.InputSystem;

namespace GodViewManagement
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "cn.ratopia.godviewmanagement";
        public const string PluginName = "上帝视角管理";
        public const string PluginVersion = "0.1.3";

        private Harmony _harmony;
        private GodViewRuntime _runtime;
        private readonly QueenInputUpdateScope _queenInputUpdateScope = new QueenInputUpdateScope();
        private readonly RuntimeTickGate _runtimeTickGate = new RuntimeTickGate();
        private bool _patchingSucceeded;
        private bool _runtimeErrorLogged;
        private bool _runtimeDriverLogged;

        internal static Plugin Instance { get; private set; }

        private void Awake()
        {
            gameObject.hideFlags |= UnityEngine.HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            Instance = this;
            var toggleKey = Config.Bind(
                "Input",
                "ToggleKey",
                Key.M,
                "切换上帝视角管理的按键；也可在游戏内设置面板重新绑定。");
            _runtime = new GodViewRuntime(Logger, toggleKey, Config);

            try
            {
                _harmony = new Harmony(PluginGuid);
                PatchAllWithDiagnostics();
                _patchingSucceeded = true;
                Logger.LogInfo($"{PluginName} v{PluginVersion} 已加载；每次进入或切换存档时默认关闭。");
            }
            catch (Exception error)
            {
                _patchingSucceeded = false;
                _harmony?.UnpatchSelf();
                _runtime.FailSafeReset();
                Logger.LogError($"Harmony 补丁安装失败，已撤销本插件全部补丁并停用：{error}");
            }
        }

        private void OnDestroy()
        {
            _patchingSucceeded = false;
            if (ReferenceEquals(Instance, this))
            {
                Instance = null;
            }

            try
            {
                _runtime?.Dispose();
            }
            catch (Exception error)
            {
                Logger.LogError($"卸载上帝视角管理时清理状态失败：{error}");
            }

            _harmony?.UnpatchSelf();
        }

        private void Update()
        {
            var game = GameMgr.Instance;
            DriveRuntime(game?._TileMgr, "Plugin.Update");
        }

        internal static void TickFromTileManager(TileMgr tileManager)
        {
            var plugin = Instance;
            plugin?.DriveRuntime(tileManager, "TileMgr.Update Postfix");
        }

        private void DriveRuntime(TileMgr tileManager, string source)
        {
            if (!_patchingSucceeded
                || _runtime == null
                || !_runtimeTickGate.TryEnter(UnityEngine.Time.frameCount))
            {
                return;
            }

            if (!_runtimeDriverLogged)
            {
                Logger.LogInfo($"独立运行时驱动已启动：{source}；不依赖其他 Mod。");
                _runtimeDriverLogged = true;
            }

            try
            {
                _runtime.Tick(tileManager);
                _runtimeErrorLogged = false;
            }
            catch (Exception error)
            {
                _runtime.FailSafeReset();
                if (!_runtimeErrorLogged)
                {
                    Logger.LogError($"运行时协调发生异常；远程上下文已清除且模式已关闭：{error}");
                    _runtimeErrorLogged = true;
                }
            }
        }

        internal static bool ShouldBlockQueenAction(BuildMidUI panel)
        {
            var plugin = Instance;
            return plugin != null
                && plugin._patchingSucceeded
                && plugin._runtime != null
                && plugin._runtime.ShouldBlockQueenAction(panel);
        }

        internal static IDisposable EnterQueenInputUpdateScope()
        {
            var plugin = Instance;
            if (plugin == null
                || !plugin._patchingSucceeded
                || plugin._runtime == null
                || !plugin._runtime.IsModeEnabled)
            {
                return null;
            }

            return plugin._queenInputUpdateScope.Enter();
        }

        internal static bool ShouldSuppressQueenDirection(HotKeyName hotKey)
        {
            var plugin = Instance;
            return plugin != null
                && plugin._patchingSucceeded
                && plugin._runtime != null
                && QueenInputIsolationRules.ShouldSuppress(
                    plugin._runtime.IsModeEnabled,
                    plugin._queenInputUpdateScope.IsActive,
                    (int)hotKey);
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
