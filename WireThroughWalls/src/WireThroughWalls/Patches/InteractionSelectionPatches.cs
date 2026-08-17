using System;
using HarmonyLib;
using UnityEngine;
using WireThroughWalls.Core;

namespace WireThroughWalls.Patches
{
    [HarmonyPatch(typeof(MiniInfoBox), nameof(MiniInfoBox.Selected), new Type[0])]
    internal static class MiniInfoSelectionPatch
    {
        private static void Postfix(MiniInfoBox __instance)
        {
            try
            {
                Plugin.LogFirstInvocation(nameof(MiniInfoSelectionPatch));
                var checkBox = GameMgr.Instance?._T_UnitMgr?.m_Queen?.m_CheckBox;
                Synchronize(checkBox, __instance?.m_Info);
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("同步高亮对象与交互目标", error);
            }
        }

        internal static void Synchronize(QueenCheckBox checkBox, MiniInfo selected)
        {
            if (selected == null || checkBox == null)
            {
                return;
            }

            switch (selected.m_Type)
            {
                case MiniType.Building:
                    checkBox.m_Building = InteractionSelectionRules.PreferSelectedTarget(
                        selected.m_Building,
                        checkBox.m_Building);
                    break;
                case MiniType.BP_Building:
                    checkBox.m_BP_Building = InteractionSelectionRules.PreferSelectedTarget(
                        selected.m_BP_Building,
                        checkBox.m_BP_Building);
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(QueenCheckBox), "OnTriggerEnter2D", new[] { typeof(Collider2D) })]
    internal static class QueenCheckBoxTriggerEnterPatch
    {
        private static void Postfix(QueenCheckBox __instance)
        {
            try
            {
                Plugin.LogFirstInvocation(nameof(QueenCheckBoxTriggerEnterPatch));
                MiniInfoSelectionPatch.Synchronize(__instance, __instance?.m_SelectInfo);
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("碰撞进入后恢复当前交互目标", error);
            }
        }
    }
}
