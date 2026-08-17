using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EquipmentReforgeSelector
{
    internal static class RuntimeController
    {
        private static ManualLogSource _logger;
        private static bool _enabled;
        private static bool _sceneHooked;
        private static bool _firstInvocationLogged;
        private static bool _firstInlineInvocationLogged;
        private static readonly PanelStateCoordinator PanelState = new PanelStateCoordinator();
        private static InlineReforgeSelectorView _view;
        private static BuildMidUI _host;
        private static ItemInfo _item;
        private static int _level;

        public static void Initialize(ManualLogSource logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _enabled = true;
            if (!_sceneHooked)
            {
                SceneManager.activeSceneChanged += OnActiveSceneChanged;
                _sceneHooked = true;
            }
        }

        public static void Disable(string reason)
        {
            _enabled = false;
            _logger?.LogError($"运行时功能已停用：{reason}");
            Clear();
        }

        public static void Shutdown()
        {
            _enabled = false;
            if (_sceneHooked)
            {
                SceneManager.activeSceneChanged -= OnActiveSceneChanged;
                _sceneHooked = false;
            }

            Clear();
            _logger = null;
        }

        public static void Open(BuildMidUI host, ItemInfo item, int level, bool detailWasAlreadyOpen)
        {
            if (!_enabled || host == null || host.Obj_Main == null || item == null)
            {
                Clear();
                return;
            }

            if (!_firstInvocationLogged)
            {
                _logger?.LogInfo("装备重铸详情补丁首次调用。");
                _firstInvocationLogged = true;
            }

            var contextChanged = _host != host || _item == null ||
                                 _item.Index != item.Index || _level != level || !detailWasAlreadyOpen;
            if (contextChanged)
            {
                Clear();
            }

            _host = host;
            _item = item;
            _level = level;
        }

        public static void OpenInlineSelector(
            SimpleToolTip tooltip,
            Batch_ResEffect frame,
            int itemType,
            int level)
        {
            if (!_enabled || tooltip == null || frame == null || _host == null || _item == null)
            {
                return;
            }

            if (itemType != _item.m_Type || level != _level || !frame.gameObject.activeInHierarchy)
            {
                SuspendInlineSelector();
                return;
            }

            if (_view == null || _view.Frame != frame)
            {
                SuspendInlineSelector();
                _view = InlineReforgeSelectorView.Create(frame);
                PanelState.Attach(_view);
            }

            if (!_firstInlineInvocationLogged)
            {
                _logger?.LogInfo("原版重铸效果列表内嵌选择器首次绑定。");
                _firstInlineInvocationLogged = true;
            }

            RefreshCurrent();
        }

        public static void Clear()
        {
            var view = _view;
            _view = null;
            _host = null;
            _item = null;
            _level = 0;
            PanelState.Clear();

            if (view != null)
            {
                view.Close();
            }
        }

        public static void SuspendInlineSelector()
        {
            var view = _view;
            _view = null;
            if (view != null)
            {
                PanelState.Detach(view);
                view.Close();
            }
        }

        public static void SelectCandidate(int candidateIndex)
        {
            if (!_enabled || !PanelState.TrySelect(candidateIndex))
            {
                WarnVanillaFallback("所选属性已失效");
                return;
            }

            _view?.SetSelection(PanelState.CurrentSelection);
        }

        public static bool TryCreateOverride(T_Queen queen, ItemInfo item, int level, Res_Ability currentAbility, out OverrideState state)
        {
            state = null;
            if (!_enabled || queen == null || item == null || _item == null ||
                item.Index != _item.Index || level != _level || !PanelState.CurrentSelection.HasValue)
            {
                WarnVanillaFallback("没有可用的当前选择");
                return false;
            }

            if (!TryResolveCandidates(item, level, (int)currentAbility, out var enhanceInfo, out var candidates, out var warning))
            {
                WarnVanillaFallback(warning);
                return false;
            }

            var selected = PanelState.CurrentSelection.Value;
            var valid = candidates.Any(candidate => candidate == selected);
            if (!valid)
            {
                WarnVanillaFallback("所选属性与最新游戏数据不匹配");
                return false;
            }

            ScopedListReferenceOverride<Res_Ability, float> scope;
            var ability = (Res_Ability)selected.AbilityId;
            if (level == 1)
            {
                scope = new ScopedListReferenceOverride<Res_Ability, float>(
                    () => enhanceInfo.List_Ability1,
                    value => enhanceInfo.List_Ability1 = (List<Res_Ability>)value,
                    () => enhanceInfo.List_AbilityValue1,
                    value => enhanceInfo.List_AbilityValue1 = (List<float>)value,
                    ability,
                    selected.Value);
            }
            else if (level == 2)
            {
                scope = new ScopedListReferenceOverride<Res_Ability, float>(
                    () => enhanceInfo.List_Ability2,
                    value => enhanceInfo.List_Ability2 = (List<Res_Ability>)value,
                    () => enhanceInfo.List_AbilityValue2,
                    value => enhanceInfo.List_AbilityValue2 = (List<float>)value,
                    ability,
                    selected.Value);
            }
            else
            {
                WarnVanillaFallback("不支持的重铸等级");
                return false;
            }

            if (!scope.IsApplied)
            {
                scope.Dispose();
                WarnVanillaFallback("无法安全替换候选列表引用");
                return false;
            }

            state = new OverrideState(scope, selected, item.Index, level);
            return true;
        }

        public static void RefreshAfterReforge()
        {
            PanelState.ResetSession();
            _logger?.LogInfo("已应用玩家选择的重铸属性，正在刷新候选项。");
            RefreshCurrent();
        }

        public static void LogRestoration(OverrideState state, Exception originalException)
        {
            if (state == null)
            {
                return;
            }

            if (originalException == null)
            {
                _logger?.LogDebug($"已恢复物品 {state.ItemIndex} 等级 {state.Level} 的原始候选列表引用。");
            }
            else
            {
                _logger?.LogWarning($"原版重铸抛出异常后已恢复原始候选列表引用：{originalException}");
            }
        }

        public static void WarnVanillaFallback(string reason)
        {
            var message = $"使用原版随机：{reason}";
            _logger?.LogWarning(message);
            _view?.ShowStatus(message, true);
        }

        public static void ReportRuntimeException(string stage, Exception exception)
        {
            _logger?.LogError($"{stage}发生运行时异常：{exception}");
        }

        public static bool IsViewCurrent(InlineReforgeSelectorView view, Batch_ResEffect frame)
        {
            if (!_enabled || view == null || view != _view || frame == null ||
                !frame.gameObject.activeInHierarchy || _host == null || _host.Obj_Main == null ||
                !_host.Obj_Main.activeInHierarchy)
            {
                return false;
            }

            try
            {
                var manager = GameMgr.Instance;
                var buildUi = manager != null && manager._ConstructUI != null ? manager._ConstructUI.m_BuildUI : null;
                return buildUi != null && buildUi.m_BuildType == 3;
            }
            catch (Exception exception)
            {
                ReportRuntimeException("检查详情面板生命周期", exception);
                return false;
            }
        }

        public static void ViewDisabled(InlineReforgeSelectorView view)
        {
            ReleaseView(view);
        }

        public static void ViewDestroyed(InlineReforgeSelectorView view)
        {
            ReleaseView(view);
        }

        private static void ReleaseView(InlineReforgeSelectorView view)
        {
            if (view != _view)
            {
                return;
            }

            _view = null;
            PanelState.Detach(view);
            if (!IsDetailHostActive())
            {
                _host = null;
                _item = null;
                _level = 0;
                PanelState.Clear();
            }
        }

        private static bool IsDetailHostActive()
        {
            return _host != null && _host.Obj_Main != null && _host.Obj_Main.activeInHierarchy;
        }

        private static void RefreshCurrent()
        {
            if (!_enabled || _host == null || _item == null || _host.Obj_Main == null)
            {
                Clear();
                return;
            }

            if (_view == null)
            {
                return;
            }

            var currentAbility = FindCurrentAbility(_item.Index, _level);
            if (!TryResolveCandidates(_item, _level, currentAbility, out _, out var candidates, out var warning))
            {
                PanelState.RefreshFailed(warning, _view);
                _logger?.LogWarning("使用原版随机：" + warning);
                return;
            }

            if (candidates.Count > _view.Capacity)
            {
                warning = "原版效果列表无法容纳全部候选";
                PanelState.RefreshFailed(warning, _view);
                _logger?.LogWarning("使用原版随机：" + warning);
                return;
            }

            PanelState.Refresh(_item.Index, _level, candidates, _view);
        }

        private static bool TryResolveCandidates(
            ItemInfo item,
            int level,
            int currentAbility,
            out ItemEnhanceInfo enhanceInfo,
            out IReadOnlyList<ReforgeCandidate> candidates,
            out string warning)
        {
            enhanceInfo = null;
            candidates = new ReforgeCandidate[0];
            warning = null;

            var manager = GameMgr.Instance;
            var database = manager != null && manager._DB_Mgr != null ? manager._DB_Mgr.m_ItemEnhanceDB : null;
            var list = database != null ? database._list : null;
            enhanceInfo = list?.Find(info => info != null && info.Type == item.m_Type);
            if (enhanceInfo == null)
            {
                warning = "找不到匹配物品类型的强化数据";
                return false;
            }

            var abilities = level == 1 ? enhanceInfo.List_Ability1 : level == 2 ? enhanceInfo.List_Ability2 : null;
            var values = level == 1 ? enhanceInfo.List_AbilityValue1 : level == 2 ? enhanceInfo.List_AbilityValue2 : null;
            if (abilities == null || values == null)
            {
                warning = "强化候选数据缺失或等级不受支持";
                return false;
            }

            var resolution = CandidateResolver.Resolve(
                level,
                currentAbility,
                abilities.Select(ability => (int)ability).ToArray(),
                values);
            if (!resolution.IsAvailable || resolution.Candidates.Count == 0)
            {
                warning = resolution.IsAvailable ? "没有可选的新属性" : "强化候选数据不一致";
                return false;
            }

            candidates = resolution.Candidates;
            return true;
        }

        private static int FindCurrentAbility(int itemIndex, int level)
        {
            var manager = GameMgr.Instance;
            var queen = manager != null && manager._T_UnitMgr != null ? manager._T_UnitMgr.m_Queen : null;
            if (queen == null || queen.Dic_ItemPlusEffect == null ||
                !queen.Dic_ItemPlusEffect.TryGetValue(itemIndex, out var effects) || effects == null)
            {
                return int.MinValue;
            }

            var sameLevel = effects.Find(effect => effect != null && effect.Level == level);
            return sameLevel != null ? (int)sameLevel.m_Ability : int.MinValue;
        }

        private static void OnActiveSceneChanged(Scene previous, Scene next)
        {
            _logger?.LogDebug($"场景从 {previous.name} 切换到 {next.name}，清理重铸选择会话。");
            Clear();
        }
    }
}
