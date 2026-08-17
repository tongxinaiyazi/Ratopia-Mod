using System;
using System.Collections.Generic;
using HarmonyLib;
using ScaffoldMod.Core;
using ScaffoldMod.Runtime;
using UnityEngine;

namespace ScaffoldMod.Patches
{
    [HarmonyPatch(typeof(DB_Mgr), "Build_DB_Setting")]
    internal static class BuildDatabasePatch
    {
        private static void Postfix(DB_Mgr __instance)
        {
            ScaffoldCatalog.Register(__instance);
        }
    }

    [HarmonyPatch(typeof(MiningBox), "BuildEnableCheck")]
    internal static class BuildEnableCheckPatch
    {
        private static void Postfix(
            ref int __result,
            BuildInfo ___m_BuildInfo,
            Transform ___Tf,
            ref bool ___m_BuildEnable,
            SpriteRenderer ___m_Spr,
            SpriteRenderer ___m_SprIcon)
        {
            if (!ScaffoldCatalog.IsScaffold(___m_BuildInfo) || ___Tf == null)
            {
                return;
            }

            var x = Mathf.CeilToInt(___Tf.position.x);
            var y = Mathf.CeilToInt(___Tf.position.y);
            var positionAllowed = ScaffoldRuntime.CanPlace(x, y, checkInventory: false);
            var inventoryAllowed = positionAllowed && ScaffoldRuntime.CanPlace(x, y, checkInventory: true);
            ___m_BuildEnable = inventoryAllowed;
            __result = inventoryAllowed ? 0 : (positionAllowed ? 1 : 2);
            var color = inventoryAllowed ? Color.white : Color.red;
            if (___m_Spr != null)
            {
                ___m_Spr.color = color;
            }
            if (___m_SprIcon != null)
            {
                ___m_SprIcon.color = color;
            }
        }
    }

    [HarmonyPatch(typeof(BP_Building), "BluePrintSet",
        new[] { typeof(BuildInfo), typeof(Vector2), typeof(int), typeof(int) })]
    internal static class BlueprintSetPatch
    {
        private static void Postfix(BuildInfo info, BP_Building __result)
        {
            if (!ScaffoldCatalog.IsScaffold(info) || __result == null)
            {
                return;
            }

            var x = Mathf.RoundToInt(__result.Pos_Tile.x);
            var y = Mathf.RoundToInt(__result.Pos_Tile.y);
            var lumberDeducted = false;
            try
            {
                __result.CancelBP();

                if (!ScaffoldRuntime.CanPlace(x, y, checkInventory: false))
                {
                    GameMgr.Instance?._CenterAlarmUI?.CenterAlarmCustomSet("此处不能放置脚手架。", Color.red);
                    return;
                }

                var buildings = GameMgr.Instance?._BuildingMgr;
                if (buildings == null || !buildings.IsMatEnoughByNoBP_Check(TileType.Lumber, 1))
                {
                    GameMgr.Instance?._CenterAlarmUI?.CenterAlarmSet(C_AlarmState.NeedResource);
                    return;
                }

                buildings.UseStorageResource(TileType.Lumber, 1);
                lumberDeducted = true;
                var currentMinute = GameMgr.Instance?._SysMgr?.GetMinuteTime() ?? 0;
                if (!ScaffoldRuntime.TryPlace(x, y, currentMinute))
                {
                    ScaffoldRuntime.SpawnLumber(x, y);
                    lumberDeducted = false;
                    ScaffoldRuntime.LogWarning($"脚手架 ({x},{y}) 创建失败，已立即返还木板。");
                }
            }
            catch (Exception exception)
            {
                if (lumberDeducted)
                {
                    ScaffoldRuntime.SpawnLumber(x, y);
                }
                ScaffoldRuntime.LogError($"即时建造脚手架 ({x},{y}) 失败并尝试返还木板：{exception}");
            }
        }
    }

    [HarmonyPatch(typeof(Func), "LoadSprite", typeof(string))]
    internal static class SpriteLoadPatch
    {
        private static bool Prefix(string name, ref Sprite __result)
        {
            if (string.Equals(name, "GameScene/Map/Building/Building_Scaffold", StringComparison.Ordinal))
            {
                __result = ScaffoldAssets.Menu;
                return false;
            }
            if (string.Equals(name, "GameScene/Map/Building/BluePrint/Building_Scaffold", StringComparison.Ordinal))
            {
                __result = ScaffoldAssets.Blueprint;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(TileMgr), "Update")]
    internal static class TileManagerUpdatePatch
    {
        private static void Postfix(TileMgr __instance)
        {
            ScaffoldRuntime.Tick(__instance);
        }
    }

    [HarmonyPatch(typeof(TileMgr), "MapDataMapping", typeof(D_Data))]
    internal static class MapDataMappingPatch
    {
        private static void Postfix(D_Data _data)
        {
            ScaffoldRuntime.MapDataMapping(_data);
        }
    }

    [HarmonyPatch(typeof(TileMgr), "NodeTypeCheck",
        new[] { typeof(int), typeof(int), typeof(bool) })]
    internal static class NodeTypeCheckPatch
    {
        private static void Prefix(TileMgr __instance, int _x, int _y, out bool __state)
        {
            ScaffoldRuntime.NodeTypeCheckPrefix(__instance, _x, _y, out __state);
        }

        private static void Postfix(TileMgr __instance, int _x, int _y, bool __state)
        {
            ScaffoldRuntime.NodeTypeCheckPostfix(__instance, _x, _y, __state);
        }
    }

    [HarmonyPatch(typeof(MiningBox), "IsMiningEnableTile", typeof(Vector2))]
    internal static class MiningEnableTilePatch
    {
        private static void Postfix(Vector2 t_pos, MiningBoxMode ___m_Mode, bool ___m_DeleteMode, ref bool __result)
        {
            if (___m_Mode == MiningBoxMode.Demolition && !___m_DeleteMode &&
                ScaffoldRuntime.Has(Mathf.RoundToInt(t_pos.x), Mathf.RoundToInt(t_pos.y)))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(WorkMark), "MarkRefresh", typeof(Building))]
    internal static class DemolitionBuildingPriorityPatch
    {
        private static bool Prefix(WorkMark __instance)
        {
            if (__instance.m_Kind != WorkMarkKind.Demolition ||
                !ScaffoldRuntime.Has(__instance.m_X, __instance.m_Y))
            {
                return true;
            }

            __instance.m_Building = null;
            __instance.m_Tile = null;
            return false;
        }
    }

    [HarmonyPatch(typeof(MiningBox), "Update")]
    internal static class MiningBoxUpdatePatch
    {
        private static void Prefix(
            MiningBoxMode ___m_Mode,
            bool ___m_DeleteMode,
            List<WorkMark> ___List_WorkMark)
        {
            if (___m_Mode != MiningBoxMode.Demolition || ___m_DeleteMode || ___List_WorkMark == null ||
                !Input.GetKeyUp(KeyCode.Mouse0))
            {
                return;
            }

            for (var index = ___List_WorkMark.Count - 1; index >= 0; index--)
            {
                var mark = ___List_WorkMark[index];
                if (mark == null || !ScaffoldRuntime.Has(mark.m_X, mark.m_Y))
                {
                    continue;
                }

                ScaffoldRuntime.Remove(mark.m_X, mark.m_Y, RemovalReason.Manual, refund: true);
                mark.WorkMarkDestroy(_IsFinish: false);
                ___List_WorkMark.RemoveAt(index);
            }
        }
    }

    [HarmonyPatch(typeof(PlayDataMgr), "LoadData", typeof(D_Data))]
    internal static class LoadDataPatch
    {
        private static void Postfix(D_Data data)
        {
            ScaffoldRuntime.OnLoadData(data);
        }
    }

    [HarmonyPatch(typeof(PlayDataMgr), "BeforeLoad")]
    internal static class BeforeLoadPatch
    {
        private static void Prefix()
        {
            ScaffoldRuntime.BeforeLoad();
        }
    }

    [HarmonyPatch(typeof(MiniInfoBox), "InfoUpdate")]
    internal static class SelectionInfoPatch
    {
        private static void Postfix(MiniInfoBox __instance)
        {
            ScaffoldRuntime.TryCustomizeSelection(__instance);
        }
    }
}
