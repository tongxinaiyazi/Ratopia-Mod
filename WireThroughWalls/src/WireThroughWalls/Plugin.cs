using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using WireThroughWalls.Core;
using WireThroughWalls.Runtime;

namespace WireThroughWalls
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "cn.ratopia.wirethroughwalls";
        public const string PluginName = "电线可穿墙";
        public const string PluginVersion = "0.1.3";

        private const float PollIntervalSeconds = 0.5f;
        private const float PortValidationIntervalSeconds = 2f;

        private readonly SessionTracker<BuildingMgr> _sessionTracker = new SessionTracker<BuildingMgr>();
        private readonly HashSet<string> _firstInvocations = new HashSet<string>(StringComparer.Ordinal);
        private Harmony _harmony;
        private WireOverlayCoordinator _coordinator;
        private PortOverlayRegistry _portRegistry;
        private BuildingMgr _portRegistryManager;
        private BuildingMgr _runtimeBuildingManager;
        private TileMgr _activeTileManager;
        private bool _patchingSucceeded;
        private bool _initializationFailureLogged;
        private float _nextPollTime;
        private float _nextPortValidationTime;

        internal static Plugin Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            try
            {
                _harmony = new Harmony(PluginGuid);
                PatchAllWithDiagnostics();
                _patchingSucceeded = true;
                Logger.LogInfo($"{PluginName} v{PluginVersion} 已加载，等待存档完成载入。");
            }
            catch (Exception error)
            {
                _patchingSucceeded = false;
                _harmony?.UnpatchSelf();
                Logger.LogError($"Harmony 补丁安装失败，Mod 已完全停用：{error}");
            }
        }

        private void Update()
        {
            if (!_patchingSucceeded || Time.unscaledTime < _nextPollTime)
            {
                return;
            }

            _nextPollTime = Time.unscaledTime + PollIntervalSeconds;
            try
            {
                PollGameSession();
            }
            catch (Exception error)
            {
                _sessionTracker.MarkInitializationFailed();
                if (!_initializationFailureLogged)
                {
                    Logger.LogError($"运行时协调失败，将在后续轮询重试：{error}");
                    _initializationFailureLogged = true;
                }
            }
        }

        private void OnDestroy()
        {
            _coordinator?.Reset();
            _portRegistry?.Reset();
            _harmony?.UnpatchSelf();
            if (ReferenceEquals(Instance, this))
            {
                Instance = null;
            }
        }

        internal static bool TryGetCoordinator(out WireOverlayCoordinator coordinator)
        {
            var plugin = Instance;
            if (plugin != null && plugin._patchingSucceeded)
            {
                var game = GameMgr.Instance;
                var manager = game?._BuildingMgr;
                var tileManager = game?._TileMgr;
                if (manager != null && tileManager != null)
                {
                    coordinator = plugin.EnsureCoordinator(manager, tileManager);
                    return coordinator != null && coordinator.IsReady;
                }
            }

            coordinator = null;
            return false;
        }

        internal static void LogPatchError(string operation, Exception error)
        {
            Instance?.Logger.LogError($"{operation}发生异常；Mod 已隔离该异常，详情：{error}");
        }

        internal static void LogFirstInvocation(string operation)
        {
            var plugin = Instance;
            if (plugin == null || !plugin._firstInvocations.Add(operation))
            {
                return;
            }

            plugin.Logger.LogDebug($"首次执行补丁：{operation}");
        }

        internal static void ObservePortRegistration(
            BuildingMgr manager,
            ElecPort port,
            string stage)
        {
            var registry = Instance?.EnsurePortRegistry(manager, GameMgr.Instance?._TileMgr);
            registry?.Register(port, stage);
        }

        internal static PortRemovalState CapturePortRemoval(BuildingMgr manager, ElecPort port)
        {
            return Instance?
                .EnsurePortRegistry(manager, GameMgr.Instance?._TileMgr)?
                .CaptureRemoval(port);
        }

        internal static PortRemovalState CapturePortRemoval(
            BuildingMgr manager,
            IEnumerable<ElecPort> ports)
        {
            return Instance?
                .EnsurePortRegistry(manager, GameMgr.Instance?._TileMgr)?
                .CaptureRemoval(ports);
        }

        internal static void LogPortCoordinationError(
            Vector2Int position,
            string owners,
            string stage,
            Exception error)
        {
            Instance?.Logger.LogError(
                $"同格端口协调失败；坐标=({position.x},{position.y})，所有者=[{owners}]，阶段={stage}。" +
                $"对象已保留，将在后续检查重试：{error}");
        }

        private void PollGameSession()
        {
            var game = GameMgr.Instance;
            var manager = game?._BuildingMgr;
            var tileManager = game?._TileMgr;

            if (manager != null && tileManager != null)
            {
                PrepareForSession(manager, tileManager);
            }

            var action = _sessionTracker.Observe(manager, tileManager == null || tileManager.m_GameLoading);
            if (action == SessionAction.Reset)
            {
                if (manager == null || tileManager == null)
                {
                    EndCurrentSession();
                }

                return;
            }

            if (action == SessionAction.Initialize)
            {
                _coordinator = EnsureCoordinator(manager, tileManager);
                _portRegistry = EnsurePortRegistry(manager, tileManager);
                _activeTileManager = tileManager;
                var validatedPorts = _portRegistry?.ValidateAllRegistered("SessionInitialize") ?? 0;
                var reevaluated = _coordinator.ReevaluateBlueprints();
                _sessionTracker.MarkInitialized();
                _initializationFailureLogged = false;
                _nextPortValidationTime = Time.unscaledTime + PortValidationIntervalSeconds;
                Logger.LogInfo(
                    $"电线背景与同格端口协调器已初始化；验证 {validatedPorts} 个端口格，" +
                    $"重评 {reevaluated} 个相关蓝图。");
                return;
            }

            if (_portRegistry != null && _portRegistry.IsReady &&
                Time.unscaledTime >= _nextPortValidationTime)
            {
                _nextPortValidationTime = Time.unscaledTime + PortValidationIntervalSeconds;
                _portRegistry.ValidateOverlaps("PeriodicValidation");
            }
        }

        private void EndCurrentSession()
        {
            _coordinator?.Reset();
            _coordinator = null;
            _portRegistry?.Reset();
            _portRegistry = null;
            _portRegistryManager = null;
            _runtimeBuildingManager = null;
            _activeTileManager = null;
            _sessionTracker.MarkInitializationFailed();
            _initializationFailureLogged = false;
        }

        private WireOverlayCoordinator EnsureCoordinator(BuildingMgr manager, TileMgr tileManager)
        {
            if (manager == null || tileManager == null)
            {
                return null;
            }

            PrepareForSession(manager, tileManager);
            if (_coordinator == null ||
                !ReferenceEquals(_coordinator.BuildingManager, manager) ||
                !ReferenceEquals(_coordinator.TileManager, tileManager))
            {
                _coordinator?.Reset();
                _coordinator = new WireOverlayCoordinator();
                _coordinator.Initialize(manager, tileManager);
            }

            return _coordinator;
        }

        private PortOverlayRegistry EnsurePortRegistry(BuildingMgr manager, TileMgr tileManager)
        {
            if (manager == null || tileManager == null)
            {
                return null;
            }

            PrepareForSession(manager, tileManager);
            if (_portRegistry == null || !ReferenceEquals(_portRegistryManager, manager))
            {
                _portRegistry?.Reset();
                _portRegistry = new PortOverlayRegistry();
                _portRegistryManager = manager;
            }

            _portRegistry.Initialize(manager, tileManager);
            return _portRegistry;
        }

        private void PrepareForSession(BuildingMgr manager, TileMgr tileManager)
        {
            if (manager == null || tileManager == null)
            {
                return;
            }

            var changed =
                (_runtimeBuildingManager != null && !ReferenceEquals(_runtimeBuildingManager, manager)) ||
                (_activeTileManager != null && !ReferenceEquals(_activeTileManager, tileManager));
            if (changed)
            {
                _coordinator?.Reset();
                _coordinator = null;
                _portRegistry?.Reset();
                _portRegistry = null;
                _portRegistryManager = null;
                _sessionTracker.MarkInitializationFailed();
                _initializationFailureLogged = false;
            }

            _runtimeBuildingManager = manager;
            _activeTileManager = tileManager;
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
