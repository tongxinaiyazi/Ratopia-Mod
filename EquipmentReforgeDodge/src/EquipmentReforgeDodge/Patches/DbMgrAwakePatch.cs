using System;
using EquipmentReforgeDodge.Core;
using HarmonyLib;

namespace EquipmentReforgeDodge.Patches
{
    /// <summary>
    /// DB_Mgr.Awake 完成所有数据库装载后，把闪避追加进饰品重铸候选。
    /// 注入具备幂等性：重复调用不会产生重复候选。
    /// </summary>
    [HarmonyPatch(typeof(DB_Mgr), "Awake")]
    internal static class DbMgrAwakePatch
    {
        private static bool _firstApplicationLogged;

        private static void Postfix(DB_Mgr __instance)
        {
            try
            {
                AccessoryEnhanceInjector.Apply(__instance, Plugin.RuntimeLog);
                if (!_firstApplicationLogged)
                {
                    Plugin.RuntimeLog?.LogInfo("饰品闪避重铸候选项已就绪。");
                    _firstApplicationLogged = true;
                }
            }
            catch (Exception exception)
            {
                Plugin.RuntimeLog?.LogError($"注入闪避重铸候选失败，本会话保持原版重铸列表：{exception}");
            }
        }
    }
}
