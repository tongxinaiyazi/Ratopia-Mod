using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace SharedWarehouse
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "cn.ratopia.sharedwarehouse";
        public const string PluginName = "共享仓库";
        public const string PluginVersion = "0.1.0";

        private const float PollIntervalSeconds = 0.5f;
        private Harmony _harmony;
        private BuildingMgr _activeManager;
        private StorageInventoryCoordinator _coordinator;
        private bool _initializedForManager;
        private bool _patchingSucceeded;
        private bool _initializationFailureLogged;
        private float _nextPollTime;

        internal static Plugin Instance { get; private set; }

        internal bool RuntimeReady => _patchingSucceeded && _initializedForManager;

        internal StorageInventoryCoordinator Coordinator => _coordinator;

        private void Awake()
        {
            Instance = this;
            _coordinator = new StorageInventoryCoordinator(Logger);

            try
            {
                EnsureHarmonyEnvironmentCompatible();
                _harmony = new Harmony(PluginGuid);
                PatchAllWithDiagnostics();
                _patchingSucceeded = true;
                Logger.LogInfo($"{PluginName} v{PluginVersion} 已加载，等待游戏存档完成载入。");
            }
            catch (Exception error)
            {
                _patchingSucceeded = false;
                _harmony?.UnpatchSelf();
                Logger.LogError($"Harmony 补丁安装失败，Mod 已停用且不会修改库存：{error}");
            }
        }

        private void Update()
        {
            if (!_patchingSucceeded || Time.unscaledTime < _nextPollTime)
            {
                return;
            }

            _nextPollTime = Time.unscaledTime + PollIntervalSeconds;
            PollGameSession();
        }

        private void OnDestroy()
        {
            try
            {
                _coordinator?.RestoreCapacityOverrides();
            }
            catch (Exception error)
            {
                Logger.LogError($"恢复原版仓库容量时发生错误：{error}");
            }

            _harmony?.UnpatchSelf();
            if (ReferenceEquals(Instance, this))
            {
                Instance = null;
            }
        }

        internal void MarkSessionDirty(Exception error, string operation)
        {
            _initializedForManager = false;
            Logger.LogError($"{operation}失败；将在下一轮安全初始化时重试，当前操作未静默忽略：{error}");
        }

        internal static bool TryGetReadyCoordinator(out StorageInventoryCoordinator coordinator)
        {
            var plugin = Instance;
            if (plugin != null && plugin.RuntimeReady && plugin._coordinator != null)
            {
                coordinator = plugin._coordinator;
                return true;
            }

            coordinator = null;
            return false;
        }

        internal static void LogPatchError(string operation, Exception error)
        {
            var plugin = Instance;
            if (plugin == null)
            {
                return;
            }

            plugin.Logger.LogError($"{operation}失败：{error}");
        }

        private void PollGameSession()
        {
            try
            {
                var game = GameMgr.Instance;
                var manager = game?._BuildingMgr;
                var tileManager = game?._TileMgr;

                if (manager == null || tileManager == null)
                {
                    EndCurrentSession();
                    return;
                }

                if (!ReferenceEquals(manager, _activeManager))
                {
                    EndCurrentSession();
                    _activeManager = manager;
                }

                if (tileManager.m_GameLoading || _initializedForManager)
                {
                    return;
                }

                _coordinator.Initialize(manager);
                _initializedForManager = true;
                _initializationFailureLogged = false;
            }
            catch (Exception error)
            {
                _initializedForManager = false;
                if (!_initializationFailureLogged)
                {
                    Logger.LogError($"共享仓库初始化失败；库存绑定已回滚，Mod 将继续重试：{error}");
                    _initializationFailureLogged = true;
                }
            }
        }

        private void EnsureHarmonyEnvironmentCompatible()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    assembly.GetName();
                }
                catch (Exception error)
                {
                    var fullName = SafeAssemblyFullName(assembly);
                    if (fullName.StartsWith("UnityEngine.CoreModule,", StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            "检测到旧版 Mono 无法处理当前中文游戏路径。请退出游戏并使用游戏根目录中的 Launch_SharedWarehouse.cmd 启动；也可以把游戏移动到全英文路径。",
                            error);
                    }

                    Logger.LogWarning(
                        $"程序集名称诊断异常：FullName={fullName}，Location={SafeAssemblyLocation(assembly)}，错误={error.Message}");
                }
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

        private static string SafeAssemblyFullName(Assembly assembly)
        {
            try
            {
                return assembly.FullName;
            }
            catch
            {
                return "<unavailable>";
            }
        }

        private static string SafeAssemblyLocation(Assembly assembly)
        {
            try
            {
                return assembly.Location;
            }
            catch
            {
                return "<unavailable>";
            }
        }

        private void EndCurrentSession()
        {
            if (_activeManager == null && !_initializedForManager)
            {
                return;
            }

            try
            {
                _coordinator.ResetSession();
            }
            catch (Exception error)
            {
                Logger.LogError($"清理上一局共享库存状态失败：{error}");
            }

            _activeManager = null;
            _initializedForManager = false;
            _initializationFailureLogged = false;
        }
    }
}
