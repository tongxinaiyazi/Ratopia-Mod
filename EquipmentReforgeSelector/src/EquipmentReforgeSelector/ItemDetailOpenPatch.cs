using System;
using HarmonyLib;

namespace EquipmentReforgeSelector
{
    [HarmonyPatch(typeof(BuildMidUI), "ItemDetail_Open", new[] { typeof(ItemInfo), typeof(bool), typeof(bool), typeof(int) })]
    internal static class ItemDetailOpenPatch
    {
        private static void Prefix(BuildMidUI __instance, out bool __state)
        {
            __state = __instance != null && __instance.Obj_Main != null && __instance.Obj_Main.activeInHierarchy;
        }

        private static void Postfix(BuildMidUI __instance, ItemInfo _info, bool _isrobot, bool _upgrade, int _level, bool __state)
        {
            try
            {
                var gameManager = GameMgr.Instance;
                var buildUi = gameManager != null && gameManager._ConstructUI != null
                    ? gameManager._ConstructUI.m_BuildUI
                    : null;
                var buildType = buildUi != null ? buildUi.m_BuildType : -1;

                if (!RuntimeEligibility.ShouldShow(_isrobot, _upgrade, buildType, _level))
                {
                    RuntimeController.Clear();
                    return;
                }

                RuntimeController.Open(__instance, _info, _level, __state);
            }
            catch (Exception exception)
            {
                RuntimeController.ReportRuntimeException("打开装备详情选择器", exception);
                RuntimeController.Clear();
            }
        }
    }
}
