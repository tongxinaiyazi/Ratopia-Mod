using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using SleepAcceleration.Patches;
using SleepAcceleration.Runtime;
using UnityEngine;

namespace SleepAcceleration
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "cn.ratopia.sleepacceleration";
        public const string PluginName = "睡觉加速";
        public const string PluginVersion = "0.1.0";

        private static readonly IReadOnlyList<Type> PatchTypes = new[]
        {
            typeof(QueenUpdatePatch),
            typeof(UserSpeedChangePatch)
        };

        private Harmony _harmony;

        private void Awake()
        {
            gameObject.hideFlags |= HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);
            SleepAccelerationRuntime.Configure(Logger);
            Logger.LogInfo($"已发现 {PluginName} {PluginVersion}，准备安装 {PatchTypes.Count} 个 Harmony 补丁。");

            _harmony = new Harmony(PluginGuid);
            try
            {
                foreach (var patchType in PatchTypes)
                {
                    Logger.LogInfo($"正在安装补丁：{patchType.Name}");
                    _harmony.CreateClassProcessor(patchType).Patch();
                    Logger.LogInfo($"补丁安装成功：{patchType.Name}");
                }

                Logger.LogInfo($"全部 {PatchTypes.Count} 个补丁安装完成，功能已启用。");
            }
            catch (Exception exception)
            {
                Logger.LogError($"补丁安装失败，正在恢复速度、撤销本 Mod 的补丁并停用功能：{exception}");
                TryShutdownRuntime();
                _harmony.UnpatchSelf();
                _harmony = null;
            }
        }

        private void OnDestroy()
        {
            TryShutdownRuntime();
            try
            {
                _harmony?.UnpatchSelf();
            }
            catch (Exception exception)
            {
                Logger.LogError($"卸载 Harmony 补丁时发生异常：{exception}");
            }
            finally
            {
                _harmony = null;
            }
        }

        private void TryShutdownRuntime()
        {
            try
            {
                SleepAccelerationRuntime.Shutdown();
            }
            catch (Exception exception)
            {
                Logger.LogError($"恢复临时时间流速或清理运行时状态时发生异常：{exception}");
            }
        }
    }
}
