using System.Text;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using ResearchAndTradeOptimization.Core;

namespace ResearchAndTradeOptimization.Runtime
{
    internal readonly struct ResearchQueueLayoutMetrics
    {
        internal ResearchQueueLayoutMetrics(
            RectTransform nodeParent,
            NodePosition firstPosition,
            float cardWidth,
            float horizontalStep,
            int slotCapacity)
        {
            NodeParent = nodeParent;
            FirstPosition = firstPosition;
            CardWidth = cardWidth;
            HorizontalStep = horizontalStep;
            SlotCapacity = slotCapacity;
        }

        internal RectTransform NodeParent { get; }

        internal NodePosition FirstPosition { get; }

        internal float CardWidth { get; }

        internal float HorizontalStep { get; }

        internal int SlotCapacity { get; }
    }

    internal static class ResearchQueueLayoutRuntime
    {
        private const string OverflowIndicatorName =
            "UnlimitedResearchQueueOverflowIndicator";

        private static readonly AccessTools.FieldRef<ResearchingGroup, TechNode[]>
            NodeArray = AccessTools.FieldRefAccess<ResearchingGroup, TechNode[]>(
                "Arr_Technode");

        private static readonly AccessTools.FieldRef<ResearchingGroup, RectTransform>
            Area = AccessTools.FieldRefAccess<ResearchingGroup, RectTransform>(
                "Tf_Area");

        private static bool _loggedFirstLayout;

        internal static bool TryGetMetrics(
            ResearchingGroup group,
            TechNode[] nodes,
            out ResearchQueueLayoutMetrics metrics)
        {
            metrics = default;
            if (group == null ||
                nodes == null ||
                nodes.Length < 2 ||
                nodes[0] == null ||
                nodes[1] == null)
            {
                return false;
            }

            var first = nodes[0].transform as RectTransform;
            var second = nodes[1].transform as RectTransform;
            var parent = first != null ? first.parent as RectTransform : null;
            if (first == null || second == null || parent == null)
            {
                return false;
            }

            var firstCorners = new Vector3[4];
            first.GetWorldCorners(firstCorners);
            var firstRight = parent.InverseTransformPoint(firstCorners[2]).x;

            float viewportRight;
            var viewport = FindClippingViewport(parent);
            if (viewport != null)
            {
                var viewportCorners = new Vector3[4];
                viewport.GetWorldCorners(viewportCorners);
                viewportRight = parent.InverseTransformPoint(
                    viewportCorners[2]).x;
            }
            else if (!TryGetRightDetailBoundary(parent, out viewportRight) ||
                     viewportRight <= firstRight)
            {
                var canvas = parent.GetComponentInParent<Canvas>();
                var canvasRect = canvas != null
                    ? canvas.transform as RectTransform
                    : null;
                if (canvasRect == null)
                {
                    return false;
                }

                var viewportCorners = new Vector3[4];
                canvasRect.GetWorldCorners(viewportCorners);
                var firstLeft = parent.InverseTransformPoint(firstCorners[0]).x;
                var viewportLeft = parent.InverseTransformPoint(viewportCorners[0]).x;
                viewportRight = parent.InverseTransformPoint(
                    viewportCorners[2]).x;
                viewportRight = ResearchQueueLayoutRules.GetCanvasFallbackRight(
                    firstLeft,
                    viewportLeft,
                    viewportRight);
            }

            var step = ResearchQueueLayoutRules.GetHorizontalStep(
                first.anchoredPosition.x,
                second.anchoredPosition.x,
                first.rect.width);
            var capacity = ResearchQueueLayoutRules.GetSlotCapacity(
                firstRight,
                viewportRight,
                step);
            if (capacity < ResearchQueueLayoutRules.MinimumSummarySlotCount)
            {
                return false;
            }

            metrics = new ResearchQueueLayoutMetrics(
                parent,
                new NodePosition(
                    first.anchoredPosition.x,
                    first.anchoredPosition.y),
                first.rect.width,
                step,
                capacity);
            return true;
        }

        internal static void ApplySingleRowSummary(ResearchingGroup group)
        {
            try
            {
                if (group == null)
                {
                    return;
                }

                var nodes = NodeArray(group);
                if (!TryGetMetrics(group, nodes, out var metrics))
                {
                    return;
                }

                var research = GameMgr.Instance?._ResearchUI;
                if (research == null)
                {
                    return;
                }

                var queueCount = ResearchQueueRuntime.GetCurrentQueueCount(research);
                var plan = ResearchQueueLayoutRules.CreateDisplayPlan(queueCount);

                ConfigureSingleRowGrid(metrics.NodeParent);
                var indicator = CreateOrGetOverflowIndicator(
                    nodes[0],
                    metrics.NodeParent);

                for (var index = 0; index < nodes.Length; index++)
                {
                    var node = nodes[index];
                    var rect = node != null
                        ? node.transform as RectTransform
                        : null;
                    if (rect == null)
                    {
                        continue;
                    }

                    var isVisible = index < plan.VisibleResearchCount;
                    if (isVisible)
                    {
                        var position = ResearchQueueLayoutRules.GetRowPosition(
                            metrics.FirstPosition,
                            metrics.HorizontalStep,
                            index);
                        rect.anchoredPosition = new Vector2(
                            position.X,
                            position.Y);
                    }

                    node.gameObject.SetActive(isVisible);
                }

                if (indicator != null)
                {
                    var indicatorRect = indicator.transform as RectTransform;
                    if (indicatorRect != null && plan.ShowOverflow)
                    {
                        var position = ResearchQueueLayoutRules.GetRowPosition(
                            metrics.FirstPosition,
                            metrics.HorizontalStep,
                            plan.DisplayedSlotCount - 1);
                        indicatorRect.anchoredPosition = new Vector2(
                            position.X,
                            position.Y);
                        indicatorRect.SetAsLastSibling();
                    }

                    indicator.gameObject.SetActive(plan.ShowOverflow);
                }

                var area = Area(group);
                if (area != null)
                {
                    area.sizeDelta = new Vector2(
                        ResearchQueueLayoutRules.GetContentWidth(
                            plan.DisplayedSlotCount,
                            metrics.CardWidth,
                            metrics.HorizontalStep),
                        130f);
                    AlignAreaToCanvasLeft(area);
                }

                if (queueCount > ResearchQueueLayoutRules.MaximumVisibleResearchCount &&
                    !_loggedFirstLayout)
                {
                    Plugin.LogRuntimeInfo(BuildLayoutDiagnostic(
                        group,
                        nodes,
                        metrics,
                        queueCount,
                        plan));
                    _loggedFirstLayout = true;
                }
            }
            catch (System.Exception exception)
            {
                Plugin.LogRuntimeError(
                    "应用研究队列单行摘要失败，保留原版显示。",
                    exception);
            }
        }

        private static void ConfigureSingleRowGrid(RectTransform nodeParent)
        {
            var grid = nodeParent.GetComponent<GridLayoutGroup>();
            if (grid == null)
            {
                return;
            }

            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            grid.constraintCount = 1;
        }

        private static bool AlignAreaToCanvasLeft(RectTransform area)
        {
            if (area == null)
            {
                return false;
            }

            var parent = area.parent as RectTransform;
            var canvas = area.GetComponentInParent<Canvas>();
            var canvasRect = canvas != null
                ? canvas.transform as RectTransform
                : null;
            if (parent == null || canvasRect == null)
            {
                return false;
            }

            var areaCorners = new Vector3[4];
            var canvasCorners = new Vector3[4];
            area.GetWorldCorners(areaCorners);
            canvasRect.GetWorldCorners(canvasCorners);
            var areaLeft = parent.InverseTransformPoint(areaCorners[0]).x;
            var canvasLeft = parent.InverseTransformPoint(canvasCorners[0]).x;
            var shift = ResearchQueueLayoutRules.GetHorizontalAlignmentShift(
                areaLeft,
                canvasLeft);
            if (float.IsNaN(shift) || float.IsInfinity(shift))
            {
                return false;
            }

            var position = area.anchoredPosition;
            area.anchoredPosition = new Vector2(
                position.x + shift,
                position.y);
            return true;
        }

        private static string BuildLayoutDiagnostic(
            ResearchingGroup group,
            TechNode[] nodes,
            ResearchQueueLayoutMetrics metrics,
            int queueCount,
            ResearchQueueDisplayPlan plan)
        {
            var message = new StringBuilder();
            message.Append(
                $"研究队列单行摘要首次应用：队列={queueCount}，" +
                $"节点={nodes.Length}，容量={metrics.SlotCapacity}，" +
                $"真实可见={plan.VisibleResearchCount}，" +
                $"显示槽位={plan.DisplayedSlotCount}，" +
                $"省略号={plan.ShowOverflow}。");

            message.AppendLine();
            message.Append("Nodes:");
            var nodeLimit = nodes.Length < 12 ? nodes.Length : 12;
            for (var index = 0; index < nodeLimit; index++)
            {
                var node = nodes[index];
                var rect = node != null
                    ? node.transform as RectTransform
                    : null;
                message.AppendLine();
                if (rect == null)
                {
                    message.Append($"  [{index}] null");
                    continue;
                }

                message.Append(
                    $"  [{index}] name={node.name}, active={node.gameObject.activeSelf}, " +
                    $"pos=({rect.anchoredPosition.x:F1},{rect.anchoredPosition.y:F1}), " +
                    $"size=({rect.rect.width:F1},{rect.rect.height:F1}), " +
                    $"sibling={rect.GetSiblingIndex()}, parent={rect.parent?.name}");
            }

            message.AppendLine();
            message.Append("Ancestors:");
            var depth = 0;
            for (var current = (Transform)metrics.NodeParent;
                 current != null && depth < 12;
                 current = current.parent, depth++)
            {
                message.AppendLine();
                var rect = current as RectTransform;
                message.Append(
                    $"  [{depth}] name={current.name}, active={current.gameObject.activeSelf}, " +
                    $"sibling={current.GetSiblingIndex()}");
                if (rect != null)
                {
                    message.Append(
                        $", pos=({rect.anchoredPosition.x:F1},{rect.anchoredPosition.y:F1}), " +
                        $"rect=({rect.rect.xMin:F1},{rect.rect.xMax:F1}," +
                        $"{rect.rect.width:F1},{rect.rect.height:F1}), " +
                        $"sizeDelta=({rect.sizeDelta.x:F1},{rect.sizeDelta.y:F1})");
                }

                message.Append(", components=");
                var components = current.GetComponents<Component>();
                for (var componentIndex = 0;
                     componentIndex < components.Length;
                     componentIndex++)
                {
                    if (componentIndex > 0)
                    {
                        message.Append('|');
                    }

                    message.Append(components[componentIndex].GetType().Name);
                }
            }

            var area = Area(group);
            message.AppendLine();
            message.Append(
                $"Area: name={area?.name}, " +
                $"sizeDelta=({area?.sizeDelta.x:F1},{area?.sizeDelta.y:F1})");
            return message.ToString();
        }

        private static TechNode CreateOrGetOverflowIndicator(
            TechNode source,
            RectTransform parent)
        {
            var existing = parent.Find(OverflowIndicatorName);
            var indicator = existing != null
                ? existing.GetComponent<TechNode>()
                : null;
            if (indicator != null)
            {
                return indicator;
            }

            indicator = Object.Instantiate(source, parent);
            indicator.name = OverflowIndicatorName;
            ConfigureOverflowIndicator(indicator);
            indicator.gameObject.SetActive(false);
            return indicator;
        }

        internal static void ConfigureOverflowIndicator(TechNode indicator)
        {
            indicator.enabled = false;
            var canvasGroup = indicator.GetComponent<CanvasGroup>() ??
                              indicator.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            if (indicator.Txt_Name != null)
            {
                indicator.Txt_Name.text = "...";
                indicator.Txt_Name.gameObject.SetActive(true);
            }

            Hide(indicator.Img_Icon);
            Hide(indicator.Img_Lock);
            Hide(indicator.Img_CatIcon);
            Hide(indicator.Obj_Highlight);
            Hide(indicator.Obj_CatFrame);
            Hide(indicator.m_ReligionFrame);
            Hide(indicator.m_Gauge);
            Hide(indicator.m_TimePad);
        }

        private static void Hide(Component component)
        {
            if (component != null)
            {
                component.gameObject.SetActive(false);
            }
        }

        private static void Hide(GameObject gameObject)
        {
            if (gameObject != null)
            {
                gameObject.SetActive(false);
            }
        }

        private static bool TryGetRightDetailBoundary(
            RectTransform nodeParent,
            out float rightBoundary)
        {
            rightBoundary = 0f;
            var research = GameMgr.Instance?._ResearchUI;
            var detail = research?.m_Tech_RPInfo;
            var detailRect = detail != null
                ? detail.transform as RectTransform
                : null;
            if (detailRect == null)
            {
                return false;
            }

            var corners = new Vector3[4];
            detailRect.GetWorldCorners(corners);
            rightBoundary = nodeParent.InverseTransformPoint(corners[0]).x;
            return !float.IsNaN(rightBoundary) &&
                   !float.IsInfinity(rightBoundary);
        }

        private static RectTransform FindClippingViewport(
            RectTransform nodeParent)
        {
            for (var current = (Transform)nodeParent;
                 current != null;
                 current = current.parent)
            {
                var rect = current as RectTransform;
                if (rect != null &&
                    (current.GetComponent<RectMask2D>() != null ||
                     current.GetComponent<Mask>() != null))
                {
                    return rect;
                }
            }

            return null;
        }
    }
}
