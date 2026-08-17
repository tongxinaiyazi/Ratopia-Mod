using System;
using BepInEx.Logging;
using SleepAcceleration.Core;
using UnityEngine;

namespace SleepAcceleration.Runtime
{
    internal static class SleepAccelerationRuntime
    {
        private static SleepAccelerationController _controller = new SleepAccelerationController();
        private static ManualLogSource _logger;
        private static SystemMgr _systemManager;
        private static RatopiaGameSpeedGateway _gateway;
        private static bool _firstTickLogged;
        private static string _lastFailure;

        public static void Configure(ManualLogSource logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public static void TickSafely(T_Queen queen)
        {
            try
            {
                if (!_firstTickLogged)
                {
                    _firstTickLogged = true;
                    _logger?.LogInfo("女王运行时补丁已首次调用，睡眠检测开始工作。");
                }

                var gameManager = GameMgr.Instance;
                var currentSystemManager = gameManager != null ? gameManager._SysMgr : null;
                if (queen == null || currentSystemManager == null || PlayDataMgr.Instance == null)
                {
                    ResetSessionIfPresent();
                    ClearFailure();
                    return;
                }

                EnsureSession(currentSystemManager);
                var isSleepingInQueenBed =
                    queen.m_CharState == CharState.Queen_Action &&
                    queen.m_AniState == AniState.Sleep_bed;
                _controller.Tick(
                    isSleepingInQueenBed,
                    currentSystemManager.IsGamePause(),
                    Time.unscaledDeltaTime,
                    _gateway);
                ClearFailure();
            }
            catch (Exception exception)
            {
                ReportFailureOnce("睡眠检测", exception);
            }
        }

        public static void NotifyUserSpeedChangedSafely(SystemMgr systemManager, float speed)
        {
            try
            {
                if (!ReferenceEquals(systemManager, _systemManager) || _gateway == null)
                {
                    return;
                }

                _controller.NotifyUserSpeedChanged();
                _gateway.MarkUserOverride(speed);
                ClearFailure();
            }
            catch (Exception exception)
            {
                ReportFailureOnce("玩家调速处理", exception);
            }
        }

        public static void Shutdown()
        {
            try
            {
                ResetSessionIfPresent();
            }
            finally
            {
                _controller = new SleepAccelerationController();
                _systemManager = null;
                _gateway = null;
                _firstTickLogged = false;
                _lastFailure = null;
            }
        }

        private static void EnsureSession(SystemMgr currentSystemManager)
        {
            if (ReferenceEquals(currentSystemManager, _systemManager) && _gateway != null)
            {
                return;
            }

            ResetSessionIfPresent();
            _systemManager = currentSystemManager;
            _gateway = new RatopiaGameSpeedGateway(currentSystemManager, _logger);
        }

        private static void ResetSessionIfPresent()
        {
            if (ReferenceEquals(_systemManager, null) || _gateway == null)
            {
                return;
            }

            if (_systemManager == null)
            {
                _logger?.LogWarning("旧 SystemMgr 已销毁，放弃其临时状态并等待新会话。");
                _controller = new SleepAccelerationController();
            }
            else
            {
                _controller.Reset(_gateway);
            }

            _systemManager = null;
            _gateway = null;
        }

        private static void ReportFailureOnce(string phase, Exception exception)
        {
            var signature = $"{phase}|{exception.GetType().FullName}|{exception.Message}";
            if (signature == _lastFailure)
            {
                return;
            }

            _lastFailure = signature;
            _logger?.LogError($"{phase}发生异常；本帧已安全跳过，后续帧会重试：{exception}");
        }

        private static void ClearFailure()
        {
            _lastFailure = null;
        }
    }
}
