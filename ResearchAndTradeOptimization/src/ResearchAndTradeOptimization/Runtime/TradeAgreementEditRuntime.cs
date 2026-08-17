using System;
using CasselGames.Diplomatic.Data;
using CasselGames.Diplomatic.UI;
using HarmonyLib;
using ResearchAndTradeOptimization.Core;
using ResearchAndTradeOptimization.Localization;
using UnityEngine;
using Utility.UI;

namespace ResearchAndTradeOptimization.Runtime
{
    internal static class TradeAgreementEditRuntime
    {
        private static readonly AccessTools.FieldRef<DiplomaticTradeDetailUI, VerticalLayoutUI>
            DetailLayout = AccessTools.FieldRefAccess<DiplomaticTradeDetailUI, VerticalLayoutUI>("_layoutUI");

        private static readonly AccessTools.FieldRef<DiplomaticTradeDetailSlotUI, TypeTradeOrder>
            DetailSlotOrder = AccessTools.FieldRefAccess<DiplomaticTradeDetailSlotUI, TypeTradeOrder>("_typeOrder");

        private static readonly AccessTools.FieldRef<DiplomaticTradeDetailUI, DiplomaticCountryTradeSheetData>
            DetailSheet = AccessTools.FieldRefAccess<DiplomaticTradeDetailUI, DiplomaticCountryTradeSheetData>("_sheetData");

        private static readonly AccessTools.FieldRef<DiplomaticUI, DiplomaticTradeSheetUI>
            SheetUi = AccessTools.FieldRefAccess<DiplomaticUI, DiplomaticTradeSheetUI>("_sheetUI");

        private static readonly AccessTools.FieldRef<DiplomaticUI, DiplomaticTradeUI>
            TradeUi = AccessTools.FieldRefAccess<DiplomaticUI, DiplomaticTradeUI>("_tradeUI");

        private static readonly AccessTools.FieldRef<DiplomaticTradeSheetUI, DiplomaticCountryTradeSheetData>
            SheetNewData = AccessTools.FieldRefAccess<DiplomaticTradeSheetUI, DiplomaticCountryTradeSheetData>("_newData");

        private static readonly AccessTools.FieldRef<DiplomaticTradeSheetUI, bool>
            SheetIsModified = AccessTools.FieldRefAccess<DiplomaticTradeSheetUI, bool>("_isModified");

        private static readonly AccessTools.FieldRef<DiplomaticTradeSheetLayoutUI, VerticalLayoutUI>
            SheetLayout = AccessTools.FieldRefAccess<DiplomaticTradeSheetLayoutUI, VerticalLayoutUI>("_layoutUI");

        private static readonly AccessTools.FieldRef<DiplomaticTradeSheetDetailSlotUI, int>
            DetailMinimum = AccessTools.FieldRefAccess<DiplomaticTradeSheetDetailSlotUI, int>("_minValue");

        private static readonly AccessTools.FieldRef<DiplomaticTradeSheetDetailSlotUI, int>
            DetailMaximum = AccessTools.FieldRefAccess<DiplomaticTradeSheetDetailSlotUI, int>("_maxValue");

        private static readonly AccessTools.FieldRef<DiplomaticTradeSheetDetailSlotUI, GameObject>
            DetailButtonFrame = AccessTools.FieldRefAccess<DiplomaticTradeSheetDetailSlotUI, GameObject>("_buttonFrame");

        private static readonly AccessTools.FieldRef<DiplomaticTradeSheetDetailSlotUI, GameObject>
            DetailEmptyFrame = AccessTools.FieldRefAccess<DiplomaticTradeSheetDetailSlotUI, GameObject>("_emptyFrame");

        private static EditSession _session;

        internal static bool IsEditable(DiplomaticCountryTradeSheetData sheet)
        {
            return sheet != null && TradeAgreementRules.IsEditableAgreement(
                (int)sheet.Resource,
                (int)sheet.State);
        }

        internal static void UpdateDetailSlot(DiplomaticTradeDetailUI detail)
        {
            try
            {
                var layout = DetailLayout(detail);
                if (layout == null || layout.TotalCount <= 1)
                {
                    return;
                }

                var slot = layout[1].GetComponent<DiplomaticTradeDetailSlotUI>();
                if (slot == null)
                {
                    return;
                }

                var sheet = DetailSheet(detail);
                var editable = IsEditable(sheet);
                slot.SetTitle(editable
                    ? ModLocalization.Get("Modify")
                    : UIUtility.GetTranslate("Word/Dip renew"));
                DetailSlotOrder(slot) = editable
                    ? TypeTradeOrder.Modify
                    : TypeTradeOrder.Update;
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError("更新贸易协议调整入口失败。", exception);
            }
        }

        internal static void OpenEditor(
            DiplomaticUI root,
            DiplomaticCountryData country,
            DiplomaticCountryTradeSheetData original)
        {
            try
            {
                if (root == null || country == null || !IsEditable(original))
                {
                    return;
                }

                var sheetUi = SheetUi(root);
                if (sheetUi == null)
                {
                    return;
                }

                var working = original.Clone();
                _session = new EditSession(root, sheetUi, country, original, working);
                SheetIsModified(sheetUi) = false;
                sheetUi.Show();
                sheetUi.SetFocus();
                sheetUi.SetTypeSheet(TypeTradeSheetCategory.Trade);
                sheetUi.SetDataOrNull(null, country, working);

                // 原版 SetDataOrNull 会按打开时市场价改写传入对象；手工调整不能立即重定价。
                working.SetTradeValue(original.TradeValue);
                sheetUi.Refresh();
                sheetUi.SetOnConfirmListener(RequestConfirmation);
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError("打开贸易协议调整面板失败。", exception);
                ClearSession();
            }
        }

        internal static void ConfigureSheetLayout(DiplomaticTradeSheetLayoutUI layout)
        {
            try
            {
                var nativeLayout = SheetLayout(layout);
                if (nativeLayout == null)
                {
                    return;
                }

                for (var index = 0; index < nativeLayout.TotalCount; index++)
                {
                    var slot = nativeLayout[index].GetComponent<DiplomaticTradeSheetSlotUI>();
                    if (slot == null)
                    {
                        continue;
                    }

                    nativeLayout[index].interactable =
                        TradeAgreementRules.IsSheetRowInteractable(
                            IsActiveSession(),
                            (int)slot.TypeTradeSheet);
                }
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError("锁定贸易协议的商品、方向与仓库控件失败。", exception);
            }
        }

        internal static void ConfigureDetailSlot(
            DiplomaticTradeSheetDetailSlotUI slot,
            TypeTradeSheet type)
        {
            try
            {
                if (type == TypeTradeSheet.Period)
                {
                    DetailMinimum(slot) = TradeAgreementRules.GetPeriodMinimum(
                        ordinaryPeriod: true,
                        DetailMinimum(slot));
                }

                if (!IsActiveSession() ||
                    (type != TypeTradeSheet.Count && type != TypeTradeSheet.Period))
                {
                    return;
                }

                DetailButtonFrame(slot)?.SetActive(true);
                DetailEmptyFrame(slot)?.SetActive(false);
                var button = slot.GetComponent<LayoutButtonUI>();
                if (button != null)
                {
                    button.interactable = true;
                }

                if (type == TypeTradeSheet.Period)
                {
                    DetailMaximum(slot) = Math.Max(
                        DetailMaximum(slot),
                        _session.Working.GoalTradeCount + 1);
                }
                else
                {
                    DetailMinimum(slot) = 1;
                    DetailMaximum(slot) = Math.Max(
                        DetailMaximum(slot),
                        _session.Original.Count + 1);
                }
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError("启用贸易协议数量/期限控件失败。", exception);
            }
        }

        internal static bool HandleSubmittedData(
            DiplomaticTradeSheetUI sheetUi,
            DiplomaticCountryTradeSheetData submitted)
        {
            if (!IsActiveSession() || _session.SheetUi != sheetUi)
            {
                return false;
            }

            try
            {
                RestoreLockedFields(submitted, _session.Original);
                submitted.SetTradeValue(_session.Original.TradeValue);
                _session.Working = submitted;
                SheetNewData(sheetUi) = submitted;
                SheetIsModified(sheetUi) = true;
                sheetUi.Refresh();
                return true;
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError("保存贸易协议调整工作副本失败。", exception);
                return true;
            }
        }

        internal static bool IsSessionFor(DiplomaticTradeSheetUI sheetUi)
        {
            return IsActiveSession() && _session.SheetUi == sheetUi;
        }

        internal static void ClearSession(DiplomaticTradeSheetUI sheetUi)
        {
            if (_session != null && _session.SheetUi == sheetUi)
            {
                SheetIsModified(sheetUi) = false;
                ClearSession();
            }
        }

        private static void RequestConfirmation(DiplomaticCountryTradeSheetData data)
        {
            if (!IsActiveSession() || data == null)
            {
                return;
            }

            var maximum = TradeAgreementRules.GetCurrentMaximumCount(
                _session.Country.NowProsperityLevel);
            if (!TradeAgreementRules.IsCountValid(
                    _session.Original.Count,
                    data.Count,
                    maximum))
            {
                _session.Root.ShowAlarm(
                    ModLocalization.Format("InvalidCount", maximum),
                    Color.red);
                return;
            }

            if (!_session.Country.HasSheet(_session.Original) ||
                !IsEditable(_session.Original))
            {
                _session.Root.ShowAlarm(
                    ModLocalization.Get("AgreementChanged"),
                    Color.red);
                return;
            }

            _session.Working = data;
            _session.Root.SetIgnoreConfirmClip();
            _session.Root.ShowCommonPopup(
                ModLocalization.Get("Modify"),
                ModLocalization.Get("ModifyConfirm"),
                null,
                UIUtility.GetSystemPopupConfirmTranslate(),
                UIUtility.GetSystemPopupCancelTranslate(),
                ApplyConfirmedEdit,
                null);
        }

        private static void ApplyConfirmedEdit()
        {
            if (!IsActiveSession())
            {
                return;
            }

            var session = _session;
            try
            {
                if (!session.Country.HasSheet(session.Original) ||
                    !IsEditable(session.Original))
                {
                    session.Root.ShowAlarm(
                        ModLocalization.Get("AgreementChanged"),
                        Color.red);
                    return;
                }

                var replacement = session.Working.Clone();
                RestoreLockedFields(replacement, session.Original);
                // 季度可能在编辑面板打开期间跨过；确认时总是采用真实协议的最新价格。
                replacement.SetTradeValue(session.Original.TradeValue);
                replacement.AgreementTradeSheet(session.Root.NowTime);
                replacement.SetState(session.Original.State);
                replacement.SetID(session.Original.ID);
                session.Country.ReplaceSheet(session.Original, replacement);
                session.SheetUi.Hide();
                session.Root.Refresh(true);
                TradeUi(session.Root)?.UnFocusTradeDetail();
                session.Root.ShowAlarm(
                    ModLocalization.Get("ModifySuccess"),
                    Color.green);
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError("应用贸易协议调整失败。", exception);
            }
        }

        private static void RestoreLockedFields(
            DiplomaticCountryTradeSheetData target,
            DiplomaticCountryTradeSheetData original)
        {
            target.SetResource(original.Resource);
            target.SetTypeTrade(original.TypeTrade);
            target.SetTypeMoney(original.TypeMoney);
            target.SetID(original.ID);
            if (original.BuildingID.HasValue)
            {
                target.SetBuilding(original.BuildingID.Value);
            }
            else
            {
                target.ResetBuilding();
            }
        }

        private static bool IsActiveSession()
        {
            return _session != null;
        }

        private static void ClearSession()
        {
            _session = null;
        }

        private sealed class EditSession
        {
            internal EditSession(
                DiplomaticUI root,
                DiplomaticTradeSheetUI sheetUi,
                DiplomaticCountryData country,
                DiplomaticCountryTradeSheetData original,
                DiplomaticCountryTradeSheetData working)
            {
                Root = root;
                SheetUi = sheetUi;
                Country = country;
                Original = original;
                Working = working;
            }

            internal DiplomaticUI Root { get; }

            internal DiplomaticTradeSheetUI SheetUi { get; }

            internal DiplomaticCountryData Country { get; }

            internal DiplomaticCountryTradeSheetData Original { get; }

            internal DiplomaticCountryTradeSheetData Working { get; set; }
        }
    }
}
