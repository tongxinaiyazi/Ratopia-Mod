using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CasselGames.Diplomatic.Data;
using CasselGames.Diplomatic.UI;
using HarmonyLib;
using ResearchAndTradeOptimization.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ResearchAndTradeOptimization.Runtime
{
    internal static class TradeResourcePreviewRuntime
    {
        private sealed class ResourceLayoutState
        {
            internal ResourceLayoutState(
                RectTransform root,
                RectTransform contents,
                GridLayoutGroup grid)
            {
                Root = root;
                Contents = contents;
                Grid = grid;
                RootSize = root.sizeDelta;
                ContentsSize = contents.sizeDelta;
                CellSize = grid.cellSize;
                Spacing = grid.spacing;
                Constraint = grid.constraint;
                ConstraintCount = grid.constraintCount;
            }

            internal RectTransform Root { get; }

            internal RectTransform Contents { get; }

            internal GridLayoutGroup Grid { get; }

            internal Vector2 RootSize { get; }

            internal Vector2 ContentsSize { get; }

            internal Vector2 CellSize { get; }

            internal Vector2 Spacing { get; }

            internal GridLayoutGroup.Constraint Constraint { get; }

            internal int ConstraintCount { get; }
        }

        private static readonly AccessTools.FieldRef<
            DiplomaticWorldDetailResourceLayoutUI,
            Transform> Contents =
                AccessTools.FieldRefAccess<
                    DiplomaticWorldDetailResourceLayoutUI,
                    Transform>("_contents");

        private static readonly AccessTools.FieldRef<
            DiplomaticWorldDetailUI,
            DiplomaticCountryData> Country =
                AccessTools.FieldRefAccess<
                    DiplomaticWorldDetailUI,
                    DiplomaticCountryData>("_country");

        private static readonly AccessTools.FieldRef<
            DiplomaticWorldDetailUI,
            DiplomaticWorldDetailResourceLayoutUI> ImportsLayout =
                AccessTools.FieldRefAccess<
                    DiplomaticWorldDetailUI,
                    DiplomaticWorldDetailResourceLayoutUI>("_importsLayoutUI");

        private static readonly AccessTools.FieldRef<
            DiplomaticWorldDetailUI,
            DiplomaticWorldDetailResourceLayoutUI> ExportsLayout =
                AccessTools.FieldRefAccess<
                    DiplomaticWorldDetailUI,
                    DiplomaticWorldDetailResourceLayoutUI>("_exportsLayoutUI");

        private static readonly ConditionalWeakTable<
            DiplomaticWorldDetailResourceLayoutUI,
            ResourceLayoutState> ResourceLayouts =
                new ConditionalWeakTable<
                    DiplomaticWorldDetailResourceLayoutUI,
                    ResourceLayoutState>();

        private static bool _loggedFirstCompactLayout;
        private static bool _loggedLayoutFailure;
        private static bool _loggedLimitFailure;

        internal static void ApplyCompactDetailLayout(
            DiplomaticWorldDetailUI detail)
        {
            try
            {
                if (detail == null)
                {
                    return;
                }

                var imports = ImportsLayout(detail);
                var exports = ExportsLayout(detail);
                if (imports == null || exports == null)
                {
                    return;
                }

                var first = GetOrCreateResourceLayout(imports);
                var second = GetOrCreateResourceLayout(exports);
                var country = Country(detail);
                var importCount = country?.CountryToHometownArray?.Length ?? 0;
                var exportCount = country?.HometownToCountryArray?.Length ?? 0;
                var topPadding = Math.Max(
                    first.Grid.padding.top,
                    second.Grid.padding.top);
                var plan = TradeResourcePreviewRules.CreateDetailPlan(
                    importCount,
                    exportCount,
                    topPadding);

                ApplyResourceLayout(first, plan);
                ApplyResourceLayout(second, plan);

                if (!_loggedFirstCompactLayout && plan.UseCompactGrid)
                {
                    _loggedFirstCompactLayout = true;
                    Plugin.LogRuntimeInfo(
                        $"首次应用国家详情紧凑商品布局：进口 {importCount} 项，出口 {exportCount} 项，固定 {plan.Columns} 列。");
                }
            }
            catch (Exception exception)
            {
                TryRestoreDetailLayouts(detail);
                if (!_loggedLayoutFailure)
                {
                    _loggedLayoutFailure = true;
                    Plugin.LogRuntimeError(
                        "应用国家详情紧凑商品布局失败，已恢复原版网格。",
                        exception);
                }
            }
        }

        internal static void LimitVisibleItems(
            ref KeyValuePair<int, TileType>[] resources)
        {
            try
            {
                resources = resources ?? Array.Empty<KeyValuePair<int, TileType>>();
                var plan = TradeResourcePreviewRules.CreatePlan(resources.Length);
                if (resources.Length <= plan.VisibleCount)
                {
                    return;
                }

                var visible = new KeyValuePair<int, TileType>[plan.VisibleCount];
                Array.Copy(resources, visible, visible.Length);
                resources = visible;
            }
            catch (Exception exception)
            {
                if (!_loggedLimitFailure)
                {
                    _loggedLimitFailure = true;
                    Plugin.LogRuntimeError(
                        "限制国家详情商品安全显示数量失败，已保留原版参数。",
                        exception);
                }
            }
        }

        private static ResourceLayoutState GetOrCreateResourceLayout(
            DiplomaticWorldDetailResourceLayoutUI layout)
        {
            return ResourceLayouts.GetValue(
                layout,
                key =>
                {
                    var root = key.transform as RectTransform;
                    var contents = Contents(key) as RectTransform;
                    var grid = contents?.GetComponent<GridLayoutGroup>();
                    if (root == null || contents == null || grid == null)
                    {
                        throw new InvalidOperationException(
                            "原版贸易商品布局缺少 RectTransform 或 GridLayoutGroup。");
                    }

                    return new ResourceLayoutState(root, contents, grid);
                });
        }

        private static void ApplyResourceLayout(
            ResourceLayoutState state,
            TradeResourceDetailLayoutPlan plan)
        {
            state.Root.sizeDelta = state.RootSize;
            if (plan.UseCompactGrid)
            {
                state.Grid.cellSize = new Vector2(
                    plan.CellWidth,
                    plan.CellHeight);
                state.Grid.spacing = new Vector2(
                    plan.HorizontalSpacing,
                    plan.VerticalSpacing);
                state.Grid.constraint =
                    GridLayoutGroup.Constraint.FixedColumnCount;
                state.Grid.constraintCount = plan.Columns;

                var contentsSize = state.ContentsSize;
                contentsSize.y = plan.ContentHeight;
                state.Contents.sizeDelta = contentsSize;
            }
            else
            {
                RestoreResourceLayout(state);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(state.Root);
        }

        private static void TryRestoreDetailLayouts(
            DiplomaticWorldDetailUI detail)
        {
            try
            {
                TryRestoreResourceLayout(ImportsLayout(detail));
                TryRestoreResourceLayout(ExportsLayout(detail));
            }
            catch
            {
                // 回退路径绝不向游戏主循环传播异常。
            }
        }

        private static void TryRestoreResourceLayout(
            DiplomaticWorldDetailResourceLayoutUI layout)
        {
            if (layout != null &&
                ResourceLayouts.TryGetValue(layout, out var state))
            {
                RestoreResourceLayout(state);
            }
        }

        private static void RestoreResourceLayout(ResourceLayoutState state)
        {
            state.Root.sizeDelta = state.RootSize;
            state.Contents.sizeDelta = state.ContentsSize;
            state.Grid.cellSize = state.CellSize;
            state.Grid.spacing = state.Spacing;
            state.Grid.constraint = state.Constraint;
            state.Grid.constraintCount = state.ConstraintCount;
        }
    }
}
