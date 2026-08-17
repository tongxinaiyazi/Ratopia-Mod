using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using RestroomBathFun.Patches;
using RestroomBathFun.Runtime;

namespace RestroomBathFun
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "cn.ratopia.restroombathfun";
        public const string PluginName = "卫生间澡堂加乐趣";
        public const string PluginVersion = "1.0.0";

        private static readonly IReadOnlyList<Type> PatchTypes = new[]
        {
            typeof(ServiceCompletionPatch)
        };

        private Harmony _harmony;

        private void Awake()
        {
            var acceptableRange = new AcceptableValueRange<float>(0f, 100f);
            var toiletFunReward = Config.Bind(
                "Rewards", "ToiletFunReward", 25f,
                new ConfigDescription(
                    "普通卫生间正常使用完成后增加的乐趣。修改后请重启游戏。",
                    acceptableRange));
            var bathsFunReward = Config.Bind(
                "Rewards", "BathsFunReward", 30f,
                new ConfigDescription(
                    "澡堂正常使用完成后增加的乐趣。修改后请重启游戏。",
                    acceptableRange));

            FunRewardRuntime.Configure(Logger, toiletFunReward, bathsFunReward);
            Logger.LogInfo(
                $"已发现 {PluginName} {PluginVersion}。普通卫生间奖励 {toiletFunReward.Value}，澡堂奖励 {bathsFunReward.Value}。准备安装 {PatchTypes.Count} 个补丁。");

            _harmony = new Harmony(PluginGuid);
            try
            {
                foreach (var patchType in PatchTypes)
                {
                    Logger.LogInfo($"正在安装补丁：{patchType.FullName}");
                    _harmony.CreateClassProcessor(patchType).Patch();
                    Logger.LogInfo($"补丁安装成功：{patchType.FullName}");
                }

                Logger.LogInfo("卫生设施乐趣奖励功能已启用。");
            }
            catch (Exception exception)
            {
                Logger.LogError($"补丁安装失败，正在撤销本 Mod 的全部补丁并停用功能：{exception}");
                _harmony.UnpatchSelf();
                _harmony = null;
            }
        }

        private void OnDestroy()
        {
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
    }
}
