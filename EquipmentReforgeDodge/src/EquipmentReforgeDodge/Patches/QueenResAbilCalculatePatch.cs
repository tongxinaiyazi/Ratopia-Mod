using System;
using HarmonyLib;

namespace EquipmentReforgeDodge.Patches
{
    /// <summary>
    /// 原版 T_Queen.ResAbil_Value_Calculate 计算 Dodge 时只统计 Buff（生活用品），
    /// 不包含装备重铸值。此补丁把已装备物品的重铸闪避（GetEnhancValue）补进总闪避。
    /// 由于只有饰品的重铸候选里存在 Dodge，武器与装甲不会带来任何闪避加成。
    /// </summary>
    [HarmonyPatch(typeof(T_Queen), "ResAbil_Value_Calculate")]
    internal static class QueenResAbilCalculatePatch
    {
        private static void Postfix(T_Queen __instance, Res_Ability _res_name, ref float value)
        {
            try
            {
                if (_res_name != Res_Ability.Dodge || __instance == null)
                {
                    return;
                }

                var enhanceValue = __instance.GetEnhancValue(Res_Ability.Dodge);
                if (enhanceValue != 0f)
                {
                    value += enhanceValue;
                    Plugin.RuntimeLog?.LogDebug($"装备重铸提供闪避 +{enhanceValue:0}%。");
                }
            }
            catch (Exception exception)
            {
                Plugin.RuntimeLog?.LogError($"合并重铸闪避值失败：{exception}");
            }
        }
    }
}
