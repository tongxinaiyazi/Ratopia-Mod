using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using ScaffoldMod.Core;
using UnityEngine;

namespace ScaffoldMod.Runtime
{
    internal enum RemovalReason
    {
        Manual,
        Expired,
        SolidTileConflict
    }

    internal static class ScaffoldRuntime
    {
        private const int MapWidth = 256;
        private const int MapHeight = 256;
        private static readonly ScaffoldRegistry Registry = new ScaffoldRegistry();
        private static readonly Dictionary<long, ScaffoldView> Views = new Dictionary<long, ScaffoldView>();

        private static ManualLogSource logger;
        private static D_Data pendingData;
        private static bool sessionInitialized;
        private static bool suppressNodeOverlay;
        private static int lastProcessedMinute = int.MinValue;

        internal static void Initialize(ManualLogSource logSource)
        {
            logger = logSource;
        }

        internal static void Shutdown()
        {
            ClearSession(restoreNodes: true);
            ScaffoldAssets.Clear();
        }

        internal static void LogInfo(string message)
        {
            logger?.LogInfo(message);
        }

        internal static void LogWarning(string message)
        {
            logger?.LogWarning(message);
        }

        internal static void LogError(string message)
        {
            logger?.LogError(message);
        }

        internal static bool Has(int x, int y)
        {
            return Registry.TryGet(x, y, out _);
        }

        internal static bool TryGet(int x, int y, out ScaffoldRecord record)
        {
            return Registry.TryGet(x, y, out record);
        }

        internal static void OnLoadData(D_Data data)
        {
            ClearSession(restoreNodes: true);
            pendingData = data;
            sessionInitialized = false;
            lastProcessedMinute = int.MinValue;
            LogInfo("收到存档数据，等待地图完成后重建脚手架。");
        }

        internal static void BeforeLoad()
        {
            ClearSession(restoreNodes: true);
            pendingData = null;
            sessionInitialized = false;
            lastProcessedMinute = int.MinValue;
        }

        internal static void Tick(TileMgr tileManager)
        {
            try
            {
                if (tileManager == null || tileManager.m_MapLoading || GameMgr.Instance == null)
                {
                    return;
                }

                ScaffoldCatalog.Register(GameMgr.Instance._DB_Mgr);
                if (!sessionInitialized)
                {
                    InitializeSession(tileManager, pendingData ?? PlayDataMgr.Instance?.m_GameData);
                }

                var systemManager = GameMgr.Instance._SysMgr;
                if (systemManager == null)
                {
                    return;
                }

                var minute = systemManager.GetMinuteTime();
                if (minute == lastProcessedMinute)
                {
                    return;
                }

                lastProcessedMinute = minute;
                ProcessMinute(tileManager, minute);
            }
            catch (Exception exception)
            {
                LogError("运行时更新异常：" + exception);
            }
        }

        internal static bool CanPlace(int x, int y, bool checkInventory)
        {
            try
            {
                if (Has(x, y) || GameMgr.Instance?._TileMgr == null)
                {
                    return false;
                }

                if (!IsPositionAllowed(GameMgr.Instance._TileMgr, x, y))
                {
                    return false;
                }

                return !checkInventory ||
                       GameMgr.Instance._BuildingMgr?.IsMatEnoughByNoBP_Check(TileType.Lumber, 1) == true;
            }
            catch (Exception exception)
            {
                LogWarning($"检查放置位置 ({x},{y}) 失败：{exception.Message}");
                return false;
            }
        }

        internal static bool TryPlace(int x, int y, int currentMinute)
        {
            var tileManager = GameMgr.Instance?._TileMgr;
            if (tileManager == null || !CanPlace(x, y, checkInventory: false))
            {
                return false;
            }

            var node = tileManager.GetNode(x, y);
            if (node == null)
            {
                return false;
            }

            var record = new ScaffoldRecord(
                x,
                y,
                ScaffoldClock.GetExpiryMinute(currentMinute),
                (int)node.m_NodeType);
            if (!Registry.TryAdd(record))
            {
                return false;
            }

            try
            {
                var view = ScaffoldView.Create(x, y);
                if (view == null)
                {
                    throw new InvalidOperationException("无法创建脚手架视觉对象。");
                }

                Views.Add(CoordinateKey(x, y), view);
                ApplyOverlay(tileManager, x, y);
                PersistCurrent();
                LogInfo($"脚手架已放置：({x},{y})，到期分钟 {record.ExpiryMinute}。");
                return true;
            }
            catch (Exception exception)
            {
                Registry.TryRemove(x, y, out _);
                if (Views.TryGetValue(CoordinateKey(x, y), out var view) && view != null)
                {
                    UnityEngine.Object.Destroy(view.gameObject);
                }
                Views.Remove(CoordinateKey(x, y));
                RestoreNode(tileManager, record);
                LogError($"创建脚手架 ({x},{y}) 失败：{exception}");
                return false;
            }
        }

        internal static bool Remove(int x, int y, RemovalReason reason, bool refund)
        {
            if (!Registry.TryRemove(x, y, out var record))
            {
                return false;
            }

            Exception removalError = null;
            try
            {
                var tileManager = GameMgr.Instance?._TileMgr;
                if (tileManager != null)
                {
                    var current = tileManager.GetNode(x, y);
                    if (current != null && current.m_NodeType != NodeType.Ladder)
                    {
                        record = new ScaffoldRecord(x, y, record.ExpiryMinute, (int)current.m_NodeType);
                    }
                    RestoreNode(tileManager, record);
                }

            }
            catch (Exception exception)
            {
                removalError = exception;
            }

            DestroyView(x, y);
            PersistCurrent();
            if (refund && !SpawnLumber(x, y))
            {
                LogError($"脚手架 ({x},{y}) 已拆除，但木板返还生成失败。");
            }
            if (removalError != null)
            {
                LogError($"拆除脚手架 ({x},{y}) 时发生异常：{removalError}");
            }

            LogInfo($"脚手架已拆除：({x},{y})，原因 {reason}，返还木板 {refund}。");
            return true;
        }

        internal static void NodeTypeCheckPrefix(TileMgr tileManager, int x, int y, out bool tracked)
        {
            tracked = false;
            if (suppressNodeOverlay || tileManager == null || !Registry.TryGet(x, y, out var record))
            {
                return;
            }

            var node = tileManager.GetNode(x, y);
            if (node == null)
            {
                return;
            }

            node.m_NodeType = (NodeType)record.UnderlyingNodeType;
            tracked = true;
        }

        internal static void NodeTypeCheckPostfix(TileMgr tileManager, int x, int y, bool tracked)
        {
            if (!tracked || suppressNodeOverlay || tileManager == null ||
                !Registry.TryGet(x, y, out var record))
            {
                return;
            }

            var node = tileManager.GetNode(x, y);
            if (node == null)
            {
                return;
            }

            var rebuiltUnderlying = (int)node.m_NodeType;
            ReplaceRecord(new ScaffoldRecord(x, y, record.ExpiryMinute, rebuiltUnderlying));
            if (!IsPositionAllowedIgnoringDuplicate(tileManager, x, y))
            {
                Remove(x, y, RemovalReason.SolidTileConflict, refund: true);
                return;
            }
            node.m_NodeType = NodeType.Ladder;
        }

        internal static void MapDataMapping(D_Data data)
        {
            if (data?.Map_NodeType == null)
            {
                return;
            }

            foreach (var record in Registry.Snapshot())
            {
                var index = record.Y * MapWidth + record.X;
                if (index >= 0 && index < data.Map_NodeType.Length)
                {
                    data.Map_NodeType[index] = (byte)record.UnderlyingNodeType;
                }
            }

            ScaffoldSaveStore.Save(data, Registry.Snapshot());
        }

        internal static bool TryCustomizeSelection(MiniInfoBox box)
        {
            var tile = box?.m_Info?.m_Tile;
            var view = tile?.GetComponent<ScaffoldView>();
            if (view == null || !Registry.TryGet(view.X, view.Y, out var record))
            {
                return false;
            }

            var currentMinute = GameMgr.Instance?._SysMgr?.GetMinuteTime() ?? 0;
            if (box.Img_Icon != null)
            {
                box.Img_Icon.enabled = true;
                box.Img_Icon.sprite = ScaffoldAssets.Menu;
            }
            if (box.Txt_Name != null)
            {
                box.Txt_Name.text = "脚手架（剩余" +
                                    ScaffoldClock.FormatRemaining(currentMinute, record.ExpiryMinute) +
                                    "）";
            }
            return true;
        }

        internal static bool SpawnLumber(int x, int y)
        {
            try
            {
                var objectPool = GameMgr.Instance?._PoolMgr?.Pool_TileObject;
                var gameObject = objectPool?.GetNextObj();
                var tileObject = gameObject?.GetComponent<TileObject>();
                if (tileObject == null)
                {
                    return false;
                }

                tileObject.ObjectInit(TileType.Lumber, TObjState.Basic, new Vector3(x, y, 0f), 1);
                return true;
            }
            catch (Exception exception)
            {
                LogError($"生成返还木板 ({x},{y}) 失败：{exception}");
                return false;
            }
        }

        private static void InitializeSession(TileMgr tileManager, D_Data data)
        {
            if (tileManager.GetNode(0, 0) == null)
            {
                return;
            }

            var loaded = ScaffoldSaveStore.Load(data);
            var accepted = 0;
            foreach (var record in loaded)
            {
                if (record.X < 0 || record.Y < 0 ||
                    record.X >= MapWidth || record.Y >= MapHeight ||
                    !IsPositionAllowed(tileManager, record.X, record.Y) ||
                    !Registry.TryAdd(record))
                {
                    continue;
                }

                try
                {
                    Views.Add(CoordinateKey(record.X, record.Y), ScaffoldView.Create(record.X, record.Y));
                    ApplyOverlay(tileManager, record.X, record.Y);
                    accepted++;
                }
                catch (Exception exception)
                {
                    Registry.TryRemove(record.X, record.Y, out _);
                    DestroyView(record.X, record.Y);
                    LogWarning($"忽略无效脚手架记录 ({record.X},{record.Y})：{exception.Message}");
                }
            }

            sessionInitialized = true;
            pendingData = data;
            PersistCurrent();
            LogInfo($"脚手架会话初始化完成，恢复 {accepted} 个对象。");
        }

        private static void ProcessMinute(TileMgr tileManager, int currentMinute)
        {
            foreach (var record in Registry.Snapshot())
            {
                if (!IsPositionAllowedIgnoringDuplicate(tileManager, record.X, record.Y))
                {
                    Remove(record.X, record.Y, RemovalReason.SolidTileConflict, refund: true);
                    continue;
                }

                var node = tileManager.GetNode(record.X, record.Y);
                if (node != null)
                {
                    if (node.m_NodeType != NodeType.Ladder)
                    {
                        ReplaceRecord(new ScaffoldRecord(
                            record.X,
                            record.Y,
                            record.ExpiryMinute,
                            (int)node.m_NodeType));
                    }
                    node.m_NodeType = NodeType.Ladder;
                }

                if (ScaffoldClock.IsExpired(currentMinute, record.ExpiryMinute))
                {
                    Remove(record.X, record.Y, RemovalReason.Expired, refund: true);
                }
            }

            PersistCurrent();
            RefreshVisibleSelectionBoxes();
        }

        private static bool IsPositionAllowed(TileMgr tileManager, int x, int y)
        {
            return !Has(x, y) && IsPositionAllowedIgnoringDuplicate(tileManager, x, y);
        }

        private static bool IsPositionAllowedIgnoringDuplicate(TileMgr tileManager, int x, int y)
        {
            var node = tileManager.GetNode(x, y);
            if (node == null)
            {
                return false;
            }

            if (Helpers.IsLadderType(node.m_TileType))
            {
                return ScaffoldPlacementRules.CanPlace(ScaffoldCellKind.Ladder, alreadyHasScaffold: false);
            }

            var tile = tileManager.GetTile(x, y);
            if (tile == null)
            {
                return ScaffoldPlacementRules.CanPlace(ScaffoldCellKind.Empty, alreadyHasScaffold: false);
            }

            var kind = tile.IsBuilding
                ? ScaffoldCellKind.Building
                : tile.IsWater
                    ? ScaffoldCellKind.Water
                    : tile.IsMine
                        ? ScaffoldCellKind.Mineral
                        : ScaffoldCellKind.SolidTerrain;
            return ScaffoldPlacementRules.CanPlace(kind, alreadyHasScaffold: false);
        }

        private static void ApplyOverlay(TileMgr tileManager, int x, int y)
        {
            var node = tileManager.GetNode(x, y);
            if (node == null)
            {
                throw new InvalidOperationException("放置位置没有有效寻路节点。");
            }

            node.m_NodeType = NodeType.Ladder;
            tileManager.NodeUpdate(x, y);
            node.m_NodeType = NodeType.Ladder;
        }

        private static void RestoreNode(TileMgr tileManager, ScaffoldRecord record)
        {
            var node = tileManager.GetNode(record.X, record.Y);
            if (node == null)
            {
                return;
            }

            suppressNodeOverlay = true;
            try
            {
                node.m_NodeType = (NodeType)record.UnderlyingNodeType;
                tileManager.NodeUpdate(record.X, record.Y);
            }
            finally
            {
                suppressNodeOverlay = false;
            }
        }

        private static void ReplaceRecord(ScaffoldRecord record)
        {
            Registry.TryRemove(record.X, record.Y, out _);
            Registry.TryAdd(record);
        }

        private static void ClearSession(bool restoreNodes)
        {
            var tileManager = GameMgr.Instance?._TileMgr;
            if (restoreNodes && tileManager != null)
            {
                foreach (var record in Registry.Snapshot())
                {
                    RestoreNode(tileManager, record);
                }
            }

            foreach (var view in Views.Values.Where(view => view != null).ToArray())
            {
                UnityEngine.Object.Destroy(view.gameObject);
            }
            Views.Clear();
            Registry.Clear();
        }

        private static void DestroyView(int x, int y)
        {
            var key = CoordinateKey(x, y);
            if (Views.TryGetValue(key, out var view) && view != null)
            {
                UnityEngine.Object.Destroy(view.gameObject);
            }
            Views.Remove(key);
        }

        private static void PersistCurrent()
        {
            ScaffoldSaveStore.Save(
                pendingData ?? PlayDataMgr.Instance?.m_GameData,
                Registry.Snapshot());
        }

        private static void RefreshVisibleSelectionBoxes()
        {
            foreach (var box in UnityEngine.Object.FindObjectsOfType<MiniInfoBox>())
            {
                if (box != null && box.gameObject.activeInHierarchy)
                {
                    TryCustomizeSelection(box);
                }
            }
        }

        private static long CoordinateKey(int x, int y)
        {
            return ((long)x << 32) ^ (uint)y;
        }
    }
}
