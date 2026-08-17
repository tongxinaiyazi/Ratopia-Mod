using System;
using BepInEx.Logging;
using SleepAcceleration.Core;

namespace SleepAcceleration.Runtime
{
    internal sealed class RatopiaGameSpeedGateway : IGameSpeedGateway
    {
        private readonly SystemMgr _systemManager;
        private readonly ManualLogSource _logger;
        private bool _temporaryAccelerationApplied;

        public RatopiaGameSpeedGateway(SystemMgr systemManager, ManualLogSource logger)
        {
            _systemManager = systemManager ?? throw new ArgumentNullException(nameof(systemManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public float UserSelectedSpeed
        {
            get
            {
                var playData = PlayDataMgr.Instance;
                if (playData == null)
                {
                    throw new InvalidOperationException("PlayDataMgr 尚未就绪，无法读取玩家速度。");
                }

                var speed = playData.m_UserGameSpeed;
                if (speed <= 0f || float.IsNaN(speed) || float.IsInfinity(speed))
                {
                    _logger.LogWarning($"玩家速度值无效（{speed}），恢复速度将使用 1 倍速。");
                    return 1f;
                }

                return speed;
            }
        }

        public void SetTemporarySpeed(float speed)
        {
            if (_systemManager == null)
            {
                throw new InvalidOperationException("SystemMgr 已销毁，无法设置临时时间流速。");
            }

            _systemManager.SetTimeScale(speed);
            if (!_temporaryAccelerationApplied)
            {
                _temporaryAccelerationApplied = true;
                _logger.LogInfo($"女王已在女王床上连续睡眠 3 秒，时间流速已临时切换为 {speed:0.###} 倍。");
                return;
            }

            _temporaryAccelerationApplied = false;
            _logger.LogInfo($"女王已离开床，时间流速已恢复为玩家选择的 {speed:0.###} 倍。");
        }

        public void MarkUserOverride(float speed)
        {
            if (!_temporaryAccelerationApplied)
            {
                return;
            }

            _temporaryAccelerationApplied = false;
            _logger.LogInfo($"玩家主动选择了 {speed:0.###} 倍速，本次睡眠加速已取消，离床时不会覆盖该速度。");
        }
    }
}
