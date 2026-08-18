using System;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace EquipmentReforgeSelector
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class EquipmentReforgeSelectorPlugin : BaseUnityPlugin
    {
        public const string PluginName = "装备重铸自选属性";
        public const string PluginGuid = "cn.ratopia.equipmentreforgeselector";
        public const string PluginVersion = "0.1.2";

        private Harmony _harmony;

        private void Awake()
        {
            gameObject.hideFlags |= HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);
            Logger.LogInfo($"发现插件：{PluginName} {PluginVersion}");

            RuntimeController.Initialize(Logger);
            _harmony = new Harmony(PluginGuid);
            var patchTypes = new[] { typeof(ItemDetailOpenPatch), typeof(ItemEnhancePatch), typeof(SimpleToolTipPatch) };

            try
            {
                foreach (var patchType in patchTypes.OrderBy(type => type.FullName, StringComparer.Ordinal))
                {
                    Logger.LogInfo($"安装补丁：{patchType.FullName}");
                    _harmony.CreateClassProcessor(patchType).Patch();
                    Logger.LogInfo($"补丁已安装：{patchType.FullName}");
                }
            }
            catch (Exception exception)
            {
                Logger.LogError($"补丁安装失败，已停用运行时功能：{exception}");
                _harmony.UnpatchSelf();
                RuntimeController.Disable("Harmony 补丁未能完整安装");
            }
        }

        private void OnDestroy()
        {
            Logger.LogWarning("插件对象已销毁，正在清理重铸选择会话。");
            RuntimeController.Shutdown();
            _harmony?.UnpatchSelf();
        }
    }
}
