using System;
using System.Collections.Generic;
using ResearchAndTradeOptimization.Core;

namespace ResearchAndTradeOptimization.Runtime
{
    internal static class ResearchReservationRuntime
    {
        private const int StartingPaymentMarker = int.MinValue + 1;

        [ThreadStatic]
        private static Stack<Stack<bool>> _refundFrames;

        private static readonly HashSet<string> MissingCostLogs =
            new HashSet<string>(StringComparer.Ordinal);

        private static UpgradeNode _lastProgressNode;
        private static bool _loggedFirstDeferredReservation;
        private static bool _loggedFirstDeferredStart;

        internal static int GetReservationBudget(ResearchUI research)
        {
            try
            {
                var info = research?.m_SelecNode?.m_Info;
                if (info == null || info.Tech_Name1 == 1)
                {
                    return research?.m_Point ?? 0;
                }

                return int.MaxValue;
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError(
                    "计算研究预约预算时发生异常，已保留原版研究点限制。",
                    exception);
                return research?.m_Point ?? 0;
            }
        }

        internal static bool ShouldAnnounceReservation(List<UpgradeNode> queue)
        {
            try
            {
                var research = GameMgr.Instance?._ResearchUI;
                var info = research?.m_SelecNode?.m_Info;
                if (research == null || info == null)
                {
                    return true;
                }

                return ResearchReservationRules.ShouldAnnounceReservation(
                    queue?.Count ?? 0,
                    research.m_Point,
                    info.Point);
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError(
                    "判断研究预约提示时发生异常，已按预约提示处理。",
                    exception);
                return true;
            }
        }

        internal static void OnResearchQueued(ResearchUI research, int originalPointDelta)
        {
            try
            {
                if (research == null)
                {
                    return;
                }

                var queue = GetCurrentQueue(research);
                if (queue == null || queue.Count == 0)
                {
                    return;
                }

                var queued = queue[queue.Count - 1];
                if (queued == null)
                {
                    return;
                }

                queued.m_StartTime = ResearchReservationRules.GetUnpaidStartTime();
                if (!_loggedFirstDeferredReservation)
                {
                    _loggedFirstDeferredReservation = true;
                    Plugin.LogRuntimeInfo("已创建首个延迟扣点研究预约。");
                }

                if (queue.Count == 1)
                {
                    TryPayForHead(research, queued);
                }
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError(
                    "登记延迟扣点研究预约时发生异常；该项目将保持等待。",
                    exception);
            }
        }

        internal static bool TryStartAndCheck(UpgradeNode node)
        {
            _lastProgressNode = node;
            try
            {
                if (node == null)
                {
                    return false;
                }

                var research = GameMgr.Instance?._ResearchUI;
                if (research == null)
                {
                    return false;
                }

                if (ResearchReservationRules.IsUnpaid(node.m_StartTime) &&
                    !TryPayForHead(research, node))
                {
                    return false;
                }

                return node.StateCheck();
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError(
                    "检查延迟扣点研究进度时发生异常；该项目将保持等待。",
                    exception);
                return false;
            }
        }

        internal static bool CanUseFastResearch(CheatMgr cheat)
        {
            try
            {
                return cheat != null &&
                       cheat.IsResearchFast &&
                       _lastProgressNode != null &&
                       !ResearchReservationRules.IsUnpaid(
                           _lastProgressNode.m_StartTime);
            }
            finally
            {
                _lastProgressNode = null;
            }
        }

        internal static bool CanRefresh(UpgradeNode node)
        {
            return node == null ||
                   !ResearchReservationRules.IsUnpaid(node.m_StartTime);
        }

        internal static void RemoveAndRememberRefund(
            List<UpgradeNode> queue,
            int index)
        {
            var removed = queue[index];
            var shouldRefund = removed == null ||
                               ResearchReservationRules.ShouldRefund(
                                   removed.m_StartTime);
            queue.RemoveAt(index);
            if (_refundFrames == null || _refundFrames.Count == 0)
            {
                BeginRefundOperation();
            }

            _refundFrames.Peek().Push(shouldRefund);
        }

        internal static void RefundRemovedResearch(ResearchUI research, int amount)
        {
            var shouldRefund = true;
            if (_refundFrames != null &&
                _refundFrames.Count > 0 &&
                _refundFrames.Peek().Count > 0)
            {
                shouldRefund = _refundFrames.Peek().Pop();
            }
            else
            {
                Plugin.LogRuntimeInfo(
                    "未找到研究取消付款状态，已按原版规则退款。");
            }

            if (shouldRefund)
            {
                research.PointUp(amount);
            }
        }

        internal static void BeginRefundOperation()
        {
            if (_refundFrames == null)
            {
                _refundFrames = new Stack<Stack<bool>>();
            }

            _refundFrames.Push(new Stack<bool>());
        }

        internal static void EndRefundOperation()
        {
            if (_refundFrames == null || _refundFrames.Count == 0)
            {
                return;
            }

            _refundFrames.Pop();
            if (_refundFrames.Count == 0)
            {
                _refundFrames = null;
            }
        }

        private static bool TryPayForHead(ResearchUI research, UpgradeNode node)
        {
            if (!ResearchReservationRules.IsUnpaid(node.m_StartTime))
            {
                return true;
            }

            if (!TryGetResearchCost(research, node, out var cost))
            {
                LogMissingCost(node);
                return false;
            }

            if (!ResearchReservationRules.CanStartUnpaidHead(
                research.m_Point,
                cost))
            {
                return false;
            }

            var pointsBefore = research.m_Point;
            node.m_StartTime = StartingPaymentMarker;
            try
            {
                research.PointUp(-cost);
                if (research.m_Point != pointsBefore - cost)
                {
                    node.m_StartTime =
                        ResearchReservationRules.GetUnpaidStartTime();
                    Plugin.LogRuntimeInfo(
                        "研究点扣除结果异常，研究项目已恢复等待状态。");
                    return false;
                }

                node.m_StartTime = GameMgr.Instance._SysMgr.GetMinuteTime();
                if (!_loggedFirstDeferredStart)
                {
                    _loggedFirstDeferredStart = true;
                    Plugin.LogRuntimeInfo(
                        $"首个延迟扣点研究已启动并扣除 {cost} 点研究点。");
                }

                return true;
            }
            catch (Exception exception)
            {
                if (research.m_Point == pointsBefore - cost)
                {
                    try
                    {
                        node.m_StartTime =
                            GameMgr.Instance._SysMgr.GetMinuteTime();
                    }
                    catch
                    {
                        node.m_StartTime = 0;
                    }
                }
                else
                {
                    node.m_StartTime =
                        ResearchReservationRules.GetUnpaidStartTime();
                }

                Plugin.LogRuntimeError(
                    "启动延迟扣点研究时发生异常。",
                    exception);
                return !ResearchReservationRules.IsUnpaid(node.m_StartTime);
            }
        }

        private static bool TryGetResearchCost(
            ResearchUI research,
            UpgradeNode node,
            out int cost)
        {
            cost = 0;
            var database = GameMgr.Instance?._DB_Mgr;
            if (database == null)
            {
                return false;
            }

            List<TechInfo> candidates = null;
            if (ContainsReference(research.List_UpgradeNode, node))
            {
                candidates = database.List_Tech_DB;
            }
            else if (ContainsReference(
                research.List_UpgradeNodeByScience,
                node))
            {
                candidates = database.List_ScientistTech_DB;
            }
            else if (ContainsReference(
                research.List_UpgradeNodeByMagician,
                node))
            {
                candidates = database.List_MagicianTech_DB;
            }

            if (candidates == null)
            {
                return false;
            }

            for (var index = 0; index < candidates.Count; index++)
            {
                var info = candidates[index];
                if (info != null &&
                    info.Category == node.m_Category &&
                    info.Index == node.m_Index)
                {
                    cost = info.Point;
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsReference(
            List<UpgradeNode> queue,
            UpgradeNode node)
        {
            if (queue == null)
            {
                return false;
            }

            for (var index = 0; index < queue.Count; index++)
            {
                if (ReferenceEquals(queue[index], node))
                {
                    return true;
                }
            }

            return false;
        }

        private static List<UpgradeNode> GetCurrentQueue(ResearchUI research)
        {
            switch (research.m_ResearchType)
            {
                case ResearchUI.ResearchType.Science:
                    return research.List_UpgradeNodeByScience;
                case ResearchUI.ResearchType.Magic:
                    return research.List_UpgradeNodeByMagician;
                default:
                    return research.List_UpgradeNode;
            }
        }

        private static void LogMissingCost(UpgradeNode node)
        {
            var key = $"{node.m_Category}:{node.m_Index}:{node.Tech_Value1}";
            if (MissingCostLogs.Add(key))
            {
                Plugin.LogRuntimeError(
                    $"无法解析研究项目费用（{key}），该项目已冻结且未扣点。");
            }
        }
    }
}
