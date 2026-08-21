using System;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using EquipmentReforgeDodge.Patches;
using HarmonyLib;

namespace EquipmentReforgeDodge
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "cn.ratopia.equipmentreforgedodge";
        public const string PluginName = "装备重铸闪避属性";
        public const string PluginVersion = "0.1.0";

        private Harmony _harmony;

        internal static ManualLogSource RuntimeLog { get; private set; }

        private void Awake()
        {
            RuntimeLog = Logger;
            _harmony = new Harmony(PluginGuid);

            try
            {
                var patchTypes = new[]
                {
                    typeof(DbMgrAwakePatch),
                    typeof(QueenResAbilCalculatePatch),
                    typeof(DodgeTooltipFormatPatch)
                };

                foreach (var patchType in patchTypes.OrderBy(type => type.FullName, StringComparer.Ordinal))
                {
                    Logger.LogInfo($"安装补丁：{patchType.FullName}");
                    _harmony.CreateClassProcessor(patchType).Patch();
                }

                Logger.LogInfo($"{PluginName} {PluginVersion} 补丁安装完成。");
            }
            catch (Exception exception)
            {
                Logger.LogError($"{PluginName} 补丁安装失败，功能已停用：{exception}");
                _harmony.UnpatchSelf();
                _harmony = null;
                enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (_harmony != null)
            {
                _harmony.UnpatchSelf();
                _harmony = null;
            }

            RuntimeLog = null;
        }
    }
}
