using System;
using HarmonyLib;

namespace EquipmentReforgeDodge.Patches
{
    /// <summary>
    /// 原版 Helpers.GetToolTipString2 对 Dodge 走默认分支，重铸槽位上只显示“+20”。
    /// 此补丁让闪避显示为“+20%”，与重铸候选列表（GetToolTipString）的百分比风格一致。
    /// 仅改写 Dodge 的结果，其他属性文本不受影响。
    /// </summary>
    [HarmonyPatch(
        typeof(Helpers),
        "GetToolTipString2",
        new[] { typeof(Res_Ability), typeof(float) })]
    internal static class DodgeTooltipFormatPatch
    {
        private static void Postfix(Res_Ability _ability, float _value, ref string __result)
        {
            try
            {
                if (_ability != Res_Ability.Dodge)
                {
                    return;
                }

                var sign = _value >= 0f ? "+" : string.Empty;
                __result = $"{sign}{_value:0}%";
            }
            catch (Exception exception)
            {
                Plugin.RuntimeLog?.LogError($"格式化闪避文本失败：{exception}");
            }
        }
    }
}
