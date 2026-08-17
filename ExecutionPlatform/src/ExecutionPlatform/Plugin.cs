using System;
using System.Collections.Generic;
using BepInEx;
using ExecutionPlatform.Patches;
using ExecutionPlatform.Runtime;
using HarmonyLib;
using UnityEngine;

namespace ExecutionPlatform
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "cn.ratopia.executionplatform";
        public const string PluginName = "处刑台";
        public const string PluginVersion = "0.1.1";

        private static readonly IReadOnlyList<Type> PatchTypes = new[]
        {
            typeof(BuildDatabasePatch),
            typeof(UnlockBuildingPatch),
            typeof(SpriteLookupPatch),
            typeof(MagicianBuildingPatch),
            typeof(BuildSetPatch),
            typeof(AddToPoolPatch),
            typeof(CitizenJobPatch),
            typeof(CitizenJobFirePatch),
            typeof(CitizenUpdatePatch),
            typeof(BeforeLoadPatch)
        };

        private Harmony _harmony;

        private void Awake()
        {
            gameObject.hideFlags |= HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);
            ExecutionRuntime.Configure(Logger);
            Logger.LogInfo($"已发现 {PluginName} {PluginVersion}，准备安装 {PatchTypes.Count} 组 Harmony 补丁。");

            _harmony = new Harmony(PluginGuid);
            try
            {
                foreach (var patchType in PatchTypes)
                {
                    _harmony.CreateClassProcessor(patchType).Patch();
                    Logger.LogInfo($"补丁安装成功：{patchType.Name}");
                }

                Logger.LogInfo("处刑台补丁安装完成，等待建筑数据库注册。");
            }
            catch (Exception exception)
            {
                Logger.LogError($"补丁安装失败，正在撤销本 Mod 的补丁并停用功能：{exception}");
                ExecutionRuntime.Shutdown();
                _harmony.UnpatchSelf();
                _harmony = null;
            }
        }

        private void OnDestroy()
        {
            ExecutionRuntime.Shutdown();
            try
            {
                _harmony?.UnpatchSelf();
            }
            catch (Exception exception)
            {
                Logger.LogError($"卸载处刑台 Harmony 补丁时发生异常：{exception}");
            }
            finally
            {
                _harmony = null;
            }
        }
    }
}
