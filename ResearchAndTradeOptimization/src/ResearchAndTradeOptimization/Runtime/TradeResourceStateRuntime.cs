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
    internal static class TradeResourceStateRuntime
    {
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

        private static readonly AccessTools.FieldRef<
            DiplomaticWorldDetailResourceLayoutUI,
            List<DiplomaticWorldDetailResourceSlotUI>> Slots =
                AccessTools.FieldRefAccess<
                    DiplomaticWorldDetailResourceLayoutUI,
                    List<DiplomaticWorldDetailResourceSlotUI>>("_slotsUI");

        private static readonly AccessTools.FieldRef<
            DiplomaticWorldDetailResourceSlotUI,
            TileType> SlotTileType =
                AccessTools.FieldRefAccess<
                    DiplomaticWorldDetailResourceSlotUI,
                    TileType>("_tileType");

        private static readonly AccessTools.FieldRef<
            DiplomaticWorldDetailResourceSlotUI,
            Image> SlotIcon =
                AccessTools.FieldRefAccess<
                    DiplomaticWorldDetailResourceSlotUI,
                    Image>("_icon");

        private static readonly ConditionalWeakTable<
            DiplomaticWorldDetailResourceSlotUI,
            Image> HighlightBackgrounds =
                new ConditionalWeakTable<
                    DiplomaticWorldDetailResourceSlotUI,
                    Image>();

        private static bool _loggedHighlightFailure;

        internal static void ApplyActiveTradeHighlight(
            DiplomaticWorldDetailUI detail)
        {
            try
            {
                if (detail == null)
                {
                    return;
                }

                var country = Country(detail);
                if (country == null)
                {
                    return;
                }

                ApplyToLayout(ImportsLayout(detail), country);
                ApplyToLayout(ExportsLayout(detail), country);
            }
            catch (Exception exception)
            {
                if (!_loggedHighlightFailure)
                {
                    _loggedHighlightFailure = true;
                    Plugin.LogRuntimeError(
                        "应用国家详情贸易中商品高亮失败，已跳过本帧高亮。",
                        exception);
                }
            }
        }

        private static void ApplyToLayout(
            DiplomaticWorldDetailResourceLayoutUI layout,
            DiplomaticCountryData country)
        {
            var slots = layout == null ? null : Slots(layout);
            if (slots == null)
            {
                return;
            }

            for (var index = 0; index < slots.Count; index++)
            {
                var slot = slots[index];
                if (slot == null || !slot.IsActivate)
                {
                    continue;
                }

                var tileType = SlotTileType(slot);
                var kind = GetHighlightKind(country, tileType);
                if (kind == TradeHighlightKind.None)
                {
                    HideHighlight(slot);
                    continue;
                }

                ShowHighlight(
                    slot,
                    kind == TradeHighlightKind.Infinite
                        ? Plugin.InfiniteTradeHighlightColor
                        : Plugin.ActiveTradeHighlightColor);
            }
        }

        private static TradeHighlightKind GetHighlightKind(
            DiplomaticCountryData country,
            TileType tileType)
        {
            var sheets = country?.Sheets;
            if (sheets == null)
            {
                return TradeHighlightKind.None;
            }

            for (var index = 0; index < sheets.Count; index++)
            {
                var sheet = sheets[index];
                if (sheet == null ||
                    sheet.Resource != tileType ||
                    sheet.IsEnded())
                {
                    continue;
                }

                return TradeResourceStateRules.GetHighlightKind(
                    isVisibleSlot: true,
                    isCurrentlyTrading: true,
                    isInfinitePeriod: sheet.IsInfinitePeriod());
            }

            return TradeHighlightKind.None;
        }

        private static void ShowHighlight(
            DiplomaticWorldDetailResourceSlotUI slot,
            Color color)
        {
            var background = GetOrCreateBackground(slot);
            background.color = color;
            background.gameObject.SetActive(true);

            var icon = SlotIcon(slot);
            if (icon != null)
            {
                var outline = icon.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = icon.gameObject.AddComponent<Outline>();
                }

                outline.effectColor = color;
                outline.effectDistance = new Vector2(2f, -2f);
                outline.enabled = true;
            }
        }

        private static void HideHighlight(
            DiplomaticWorldDetailResourceSlotUI slot)
        {
            if (HighlightBackgrounds.TryGetValue(slot, out var background) &&
                background != null)
            {
                background.gameObject.SetActive(false);
            }

            var icon = SlotIcon(slot);
            if (icon != null)
            {
                var outline = icon.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = false;
                }
            }
        }

        private static Image GetOrCreateBackground(
            DiplomaticWorldDetailResourceSlotUI slot)
        {
            if (HighlightBackgrounds.TryGetValue(slot, out var existing) &&
                existing != null)
            {
                return existing;
            }

            var gameObject = new GameObject(
                "ActiveTradeHighlight",
                typeof(RectTransform),
                typeof(Image));
            var rect = (RectTransform)gameObject.transform;
            rect.SetParent(slot.transform, false);
            rect.SetAsFirstSibling();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = gameObject.GetComponent<Image>();
            image.raycastTarget = false;
            HighlightBackgrounds.Add(slot, image);
            return image;
        }
    }
}
