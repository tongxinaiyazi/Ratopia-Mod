using System;
using System.Collections.Generic;
using System.Threading;
using BepInEx.Logging;
using BroadcastStationGlobalCoverage.Core;
using UnityEngine;

namespace BroadcastStationGlobalCoverage.Runtime
{
    internal static class BroadcastSignalRuntime
    {
        private static ManualLogSource _logger;
        private static int _manualLogged;
        private static int _automaticLogged;

        internal static void Configure(ManualLogSource logger)
        {
            _logger = logger;
        }

        internal static void EnsureManualCandidates(List<Building> candidates)
        {
            TryRun("补齐电视全图广播站候选", () =>
            {
                var manager = GameMgr.Instance?._BuildingMgr;
                if (manager?.List_Building == null || candidates == null)
                {
                    return;
                }

                BroadcastSignalPolicy.AppendMissing(
                    candidates,
                    manager.List_Building,
                    IsBroadcastStation);

                if (Interlocked.Exchange(ref _manualLogged, 1) == 0)
                {
                    _logger?.LogInfo("电视手动选台已使用全图广播站候选；未修改建筑或电路范围。");
                }
            });
        }

        internal static void EnsureAutomaticSource(Building_ElecBandstand television)
        {
            TryRun("刷新电视全图广播信号源", () =>
            {
                if (television?.m_Info == null ||
                    television.m_Info.Name != BuildingName.Television ||
                    television.m_ControlNum != 0 ||
                    television.m_BuildState != 0 ||
                    television.m_ElecNum == 0)
                {
                    return;
                }

                var manager = GameMgr.Instance?._BuildingMgr;
                if (manager?.List_Building == null)
                {
                    return;
                }

                var origin = (Vector2)television.Tf.position;
                television.m_End_Building = BroadcastSignalPolicy.FindNearest(
                    manager.List_Building,
                    IsWorkingBroadcastStation,
                    candidate => ((Vector2)candidate.Tf.position - origin).sqrMagnitude,
                    television);

                if (Interlocked.Exchange(ref _automaticLogged, 1) == 0)
                {
                    _logger?.LogInfo("电视自动选台已使用全图广播站候选；未修改建筑或电路范围。");
                }
            });
        }

        private static bool IsBroadcastStation(Building building)
        {
            return building?.m_Info != null &&
                   BroadcastSignalPolicy.IsBroadcastStation((int)building.m_Info.Name);
        }

        private static bool IsWorkingBroadcastStation(Building building)
        {
            return IsBroadcastStation(building) &&
                   building.m_BuildInfoUI != null &&
                   building.m_BuildInfoUI.IsMasterReady();
        }

        private static void TryRun(string operation, Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                _logger?.LogError($"{operation}失败；已跳过本次处理以保护游戏流程：{exception}");
            }
        }
    }
}
