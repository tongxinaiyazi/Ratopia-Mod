using System;
using HarmonyLib;

namespace EquipmentReforgeSelector
{
    [HarmonyPatch(
        typeof(SimpleToolTip),
        "SimpleToolTipSet",
        new[]
        {
            typeof(SimpleToolTip.SimpleToolTipList),
            typeof(float),
            typeof(float),
            typeof(float)
        })]
    internal static class SimpleToolTipPatch
    {
        private static void Postfix(
            SimpleToolTip __instance,
            SimpleToolTip.SimpleToolTipList _value,
            float _a_value,
            float _b_value,
            Batch_ResEffect ___m_EffectFrame)
        {
            if (_value != SimpleToolTip.SimpleToolTipList.EnhanceEffect)
            {
                RuntimeController.SuspendInlineSelector();
                return;
            }

            try
            {
                RuntimeController.OpenInlineSelector(
                    __instance,
                    ___m_EffectFrame,
                    (int)_a_value,
                    (int)_b_value);
            }
            catch (Exception exception)
            {
                RuntimeController.ReportRuntimeException("绑定原版重铸效果列表", exception);
                RuntimeController.SuspendInlineSelector();
                RuntimeController.WarnVanillaFallback("无法使用原版效果列表选择属性");
            }
        }
    }
}
