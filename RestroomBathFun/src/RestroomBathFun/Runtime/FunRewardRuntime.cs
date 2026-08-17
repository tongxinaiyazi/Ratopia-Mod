using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using RestroomBathFun.Core;

namespace RestroomBathFun.Runtime
{
    internal static class FunRewardRuntime
    {
        private static ManualLogSource _logger;
        private static ConfigEntry<float> _toiletFunReward;
        private static ConfigEntry<float> _bathsFunReward;

        internal static void Configure(
            ManualLogSource logger,
            ConfigEntry<float> toiletFunReward,
            ConfigEntry<float> bathsFunReward)
        {
            _logger = logger;
            _toiletFunReward = toiletFunReward;
            _bathsFunReward = bathsFunReward;
        }

        internal static void ApplySafely(T_Citizen citizen, ServiceCompletionState state)
        {
            try
            {
                if (citizen == null)
                {
                    return;
                }

                var settings = new RewardSettings(
                    _toiletFunReward?.Value ?? RewardSettings.Default.ToiletFunReward,
                    _bathsFunReward?.Value ?? RewardSettings.Default.BathsFunReward);
                var reward = FunRewardPolicy.Resolve(
                    state.Facility,
                    state.ServiceAborted,
                    settings);
                if (reward <= 0f)
                {
                    return;
                }

                citizen.FunUpdate(reward);
            }
            catch (Exception exception)
            {
                _logger?.LogError($"应用卫生设施乐趣奖励时发生异常，已跳过本次奖励：{exception}");
            }
        }

        internal static void LogPatchException(Exception exception)
        {
            _logger?.LogError($"读取卫生设施服务完成状态时发生异常，已跳过本次奖励：{exception}");
        }
    }
}
