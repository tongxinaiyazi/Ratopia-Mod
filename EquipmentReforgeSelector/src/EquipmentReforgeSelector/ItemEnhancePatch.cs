using System;
using HarmonyLib;

namespace EquipmentReforgeSelector
{
    [HarmonyPatch(typeof(T_Queen), "ItemEnhance", new[] { typeof(ItemInfo), typeof(int), typeof(Res_Ability) })]
    internal static class ItemEnhancePatch
    {
        [HarmonyPriority(Priority.Last)]
        private static void Prefix(T_Queen __instance, ItemInfo _info, int _level, Res_Ability _before_res, ref OverrideState __state)
        {
            try
            {
                RuntimeController.TryCreateOverride(__instance, _info, _level, _before_res, out __state);
            }
            catch (Exception exception)
            {
                __state?.Dispose();
                __state = null;
                RuntimeController.ReportRuntimeException("应用重铸选择", exception);
                RuntimeController.WarnVanillaFallback("应用选择时发生异常");
            }
        }

        private static void Postfix(OverrideState __state)
        {
            if (__state != null && __state.IsApplied)
            {
                __state.UiDirty = true;
            }
        }

        private static Exception Finalizer(Exception __exception, OverrideState __state)
        {
            try
            {
                if (__state != null)
                {
                    var refresh = __state.UiDirty && __exception == null;
                    __state.Dispose();
                    RuntimeController.LogRestoration(__state, __exception);
                    if (refresh)
                    {
                        RuntimeController.RefreshAfterReforge();
                    }
                }
            }
            catch (Exception restoreException)
            {
                RuntimeController.ReportRuntimeException("恢复原始重铸候选列表", restoreException);
            }

            return __exception;
        }
    }
}
