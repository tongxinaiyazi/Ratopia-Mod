using System;
using System.Linq;
using BepInEx;
using HarmonyLib;
using TerrainEditor.Core;
using TerrainEditor.Runtime;
using UnityEngine;

namespace TerrainEditor
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal const string PluginGuid = "cn.ratopia.terraineditor";
        internal const string PluginName = "地形编辑器";
        internal const string PluginVersion = "0.1.0";

        private readonly FrameTickGate _tickGate = new FrameTickGate();
        private Harmony _harmony;
        private TerrainEditorController _controller;
        private bool _patchingSucceeded;
        private bool _runtimeErrorLogged;
        private bool _runtimeDriverLogged;

        internal static Plugin Instance { get; private set; }

        private void Awake()
        {
            gameObject.hideFlags |= HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);
            Instance = this;
            _controller = new TerrainEditorController(new RatopiaTerrainEditorGateway());

            try
            {
                _harmony = new Harmony(PluginGuid);
                PatchAllWithDiagnostics();
                _patchingSucceeded = true;
                Logger.LogInfo($"{PluginName} v{PluginVersion} 已加载；F4 切换，Esc 关闭。");
            }
            catch (Exception error)
            {
                _patchingSucceeded = false;
                _harmony?.UnpatchSelf();
                FailSafeReset("补丁安装失败", error);
            }
        }

        private void Update()
        {
            DriveRuntime("Plugin.Update");
        }

        private void OnDestroy()
        {
            _patchingSucceeded = false;
            FailSafeReset("插件卸载", null);
            _harmony?.UnpatchSelf();

            if (ReferenceEquals(Instance, this))
            {
                Instance = null;
            }
        }

        internal static void TickFromTileManager(TileMgr tileManager)
        {
            var plugin = Instance;
            if (plugin == null || tileManager == null)
            {
                return;
            }

            plugin.DriveRuntime("TileMgr.Update Postfix");
        }

        internal static void PrepareForSceneChange()
        {
            Instance?.FailSafeReset("切换场景", null);
        }

        private void DriveRuntime(string source)
        {
            if (!_patchingSucceeded || _controller == null || !_tickGate.TryEnter(Time.frameCount))
            {
                return;
            }

            if (!_runtimeDriverLogged)
            {
                Logger.LogInfo($"独立运行时驱动已启动：{source}。");
                _runtimeDriverLogged = true;
            }

            try
            {
                var transition = _controller.Tick(new EditorInput(
                    Input.GetKeyDown(KeyCode.F4),
                    Input.GetKeyDown(KeyCode.Escape)));

                if (transition == EditorTransition.Entered)
                {
                    Logger.LogInfo("地形编辑器已开启。");
                }
                else if (transition == EditorTransition.Exited)
                {
                    Logger.LogInfo("地形编辑器已关闭，进入前状态已恢复。");
                }

                _runtimeErrorLogged = false;
            }
            catch (Exception error)
            {
                FailSafeReset("运行时处理失败", error);
            }
        }

        private void FailSafeReset(string reason, Exception error)
        {
            try
            {
                _controller?.Exit();
            }
            catch (Exception cleanupError)
            {
                Logger.LogError($"{reason}后的状态清理失败：{cleanupError}");
            }

            if (error != null && !_runtimeErrorLogged)
            {
                Logger.LogError($"{reason}，本次编辑会话已关闭并尝试恢复状态：{error}");
                _runtimeErrorLogged = true;
            }
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
