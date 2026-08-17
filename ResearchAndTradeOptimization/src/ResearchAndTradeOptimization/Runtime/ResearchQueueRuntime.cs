using System;
using HarmonyLib;
using UnityEngine;
using ResearchAndTradeOptimization.Core;

namespace ResearchAndTradeOptimization.Runtime
{
    internal static class ResearchQueueRuntime
    {
        private static readonly AccessTools.FieldRef<ResearchingGroup, TechNode[]> NodeArray =
            AccessTools.FieldRefAccess<ResearchingGroup, TechNode[]>("Arr_Technode");

        private static bool _loggedFirstExpansion;

        internal static int GetEffectiveLimit()
        {
            try
            {
                var research = GameMgr.Instance?._ResearchUI;
                if (research == null || research.m_ResearchingGroup == null)
                {
                    return QueueRules.GetResearchLimit(false);
                }

                var desiredCount = GetCurrentQueueCount(research) + 1;
                return QueueRules.GetResearchLimit(
                    EnsureVisibleCapacity(research.m_ResearchingGroup, desiredCount));
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError("计算研究队列上限时发生异常，已回退原版上限 3。", exception);
                return QueueRules.GetResearchLimit(false);
            }
        }

        internal static void EnsureCurrentQueueVisible(ResearchingGroup group)
        {
            try
            {
                var research = GameMgr.Instance?._ResearchUI;
                if (research != null)
                {
                    EnsureVisibleCapacity(group, GetCurrentQueueCount(research));
                }
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError("刷新研究队列可见节点时发生异常。", exception);
            }
        }

        internal static bool EnsureVisibleCapacity(ResearchingGroup group, int desiredCount)
        {
            try
            {
                if (group == null)
                {
                    return false;
                }

                var nodes = NodeArray(group);
                if (nodes == null || nodes.Length == 0)
                {
                    return false;
                }

                var metrics = default(ResearchQueueLayoutMetrics);
                if (desiredCount > 3 &&
                    !ResearchQueueLayoutRuntime.TryGetMetrics(
                        group,
                        nodes,
                        out metrics))
                {
                    return false;
                }

                if (desiredCount <= nodes.Length)
                {
                    return true;
                }

                if (desiredCount <= 3)
                {
                    return false;
                }

                var originalLength = nodes.Length;
                var expanded = new TechNode[desiredCount];
                Array.Copy(nodes, expanded, nodes.Length);

                var source = nodes[0];
                var sourceRect = source != null
                    ? source.transform as RectTransform
                    : null;
                if (sourceRect == null)
                {
                    return false;
                }

                for (var index = nodes.Length; index < desiredCount; index++)
                {
                    var clone = UnityEngine.Object.Instantiate(
                        source,
                        metrics.NodeParent);
                    var cloneRect = clone.transform as RectTransform;
                    if (cloneRect == null)
                    {
                        return false;
                    }

                    clone.name = $"TechNode_Queue_{index}";
                    var position = ResearchQueueLayoutRules.GetRowPosition(
                        metrics.FirstPosition,
                        metrics.HorizontalStep,
                        index);
                    cloneRect.anchoredPosition = new Vector2(
                        position.X,
                        position.Y);
                    cloneRect.SetSiblingIndex(
                        sourceRect.GetSiblingIndex() + index);
                    clone.gameObject.SetActive(false);
                    expanded[index] = clone;
                }

                NodeArray(group) = expanded;
                if (!_loggedFirstExpansion)
                {
                    _loggedFirstExpansion = true;
                    Plugin.LogRuntimeInfo(
                        $"研究队列界面首次扩容：" +
                        $"{originalLength} -> {expanded.Length} 个节点；" +
                        $"摘要固定显示前 " +
                        $"{ResearchQueueLayoutRules.MaximumVisibleResearchCount} 项，" +
                        $"其余使用省略号。");
                }

                return true;
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError("扩充研究队列界面失败，已回退原版上限 3。", exception);
                return false;
            }
        }

        internal static int GetCurrentQueueCount(ResearchUI research)
        {
            switch (research.m_ResearchType)
            {
                case ResearchUI.ResearchType.Science:
                    return research.List_UpgradeNodeByScience?.Count ?? 0;
                case ResearchUI.ResearchType.Magic:
                    return research.List_UpgradeNodeByMagician?.Count ?? 0;
                default:
                    return research.List_UpgradeNode?.Count ?? 0;
            }
        }
    }
}
