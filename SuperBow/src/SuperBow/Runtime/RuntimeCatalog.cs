using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using SuperBow.Core;

namespace SuperBow.Runtime
{
    internal static class RuntimeCatalog
    {
        private const float ValueTolerance = 0.0001f;
        private static ManualLogSource _logger;
        private static bool _enabled;
        private static bool _reforgeContextEnabled;
        private static bool _firstInvocationLogged;
        private static DB_Mgr _manager;
        private static CatalogPatchSession _session;
        private static CandidatePatchSession _candidateSession;
        private static ItemEnhanceInfo _bowEnhance;

        public static void Initialize(ManualLogSource logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _enabled = true;
            _reforgeContextEnabled = true;
        }

        public static void TryApplySafely(DB_Mgr manager)
        {
            if (!_enabled || manager == null)
            {
                return;
            }

            if (!_firstInvocationLogged)
            {
                _logger?.LogInfo("装备数据库补丁首次执行。");
                _firstInvocationLogged = true;
            }

            try
            {
                TryApply(manager);
            }
            catch (Exception exception)
            {
                DisposeSession();
                _enabled = false;
                _logger?.LogError($"应用超级弓箭数据失败，已恢复本次修改并停用数据功能：{exception}");
            }
        }

        public static void Shutdown()
        {
            _enabled = false;
            _reforgeContextEnabled = false;
            ClearReforgeContext();
            DisposeSession();
            _bowEnhance = null;
            _manager = null;
            _logger = null;
        }

        public static void ReportLookupFailure(Exception exception)
        {
            ClearReforgeContext();
            DisposeSession();
            _bowEnhance = null;
            _enabled = false;
            _logger?.LogError($"获取装备数据库失败，已停用数据功能：{exception}");
        }

        public static void SetReforgeContextSafely(ItemInfo item)
        {
            if (!_enabled)
            {
                return;
            }

            if (item == null || !QueenBowIdentity.IsMatch(item.Index, item.m_Type, item.Name))
            {
                ClearReforgeContext();
                return;
            }

            if (!_reforgeContextEnabled)
            {
                return;
            }

            try
            {
                SetQueenBowReforgeContext();
            }
            catch (Exception exception)
            {
                ClearReforgeContext();
                _reforgeContextEnabled = false;
                _logger?.LogError(
                    $"设置女王弓专属重铸候选失败，已停用特殊词条候选：{exception}");
            }
        }

        public static void ClearReforgeContext()
        {
            _candidateSession?.Dispose();
            _candidateSession = null;
        }

        private static void TryApply(DB_Mgr manager)
        {
            if (ReferenceEquals(_manager, manager) && _session != null)
            {
                return;
            }

            if (!ReferenceEquals(_manager, manager))
            {
                ClearReforgeContext();
                DisposeSession();
                _bowEnhance = null;
                _manager = manager;
            }

            var weapons = manager.List_WeaponDB;
            var enhanceInfos = manager.m_ItemEnhanceDB != null
                ? manager.m_ItemEnhanceDB._list
                : null;
            if (weapons == null || enhanceInfos == null)
            {
                return;
            }

            var queenBow = RequireSingleWeapon(
                weapons,
                SuperBowConstants.QueenBowIndex,
                SuperBowConstants.QueenBowType,
                SuperBowConstants.QueenBowName);
            var nobleSword = RequireSingleWeapon(
                weapons,
                SuperBowConstants.NobleSwordIndex,
                SuperBowConstants.NobleSwordType,
                SuperBowConstants.NobleSwordName);
            var bowEnhance = enhanceInfos.Single(info =>
                info != null && info.Type == SuperBowConstants.QueenBowType);

            ValidateAligned(queenBow.List_Ability, queenBow.List_AbilityValue, "女王弓基础属性");
            ValidateAligned(nobleSword.List_Ability, nobleSword.List_AbilityValue, "贵族剑基础属性");
            ValidateAligned(bowEnhance.List_Ability1, bowEnhance.List_AbilityValue1, "弓重铸1");
            ValidateAligned(bowEnhance.List_Ability2, bowEnhance.List_AbilityValue2, "弓重铸2");

            var queenAttackIndex = FindSingleAbilityIndex(
                queenBow.List_Ability,
                (Res_Ability)SuperBowConstants.AttackAbilityId,
                "女王弓 ATK");
            var nobleAttackIndex = FindSingleAbilityIndex(
                nobleSword.List_Ability,
                (Res_Ability)SuperBowConstants.AttackAbilityId,
                "贵族剑 ATK");
            var nobleAttack = nobleSword.List_AbilityValue[nobleAttackIndex];
            if (Math.Abs(nobleAttack - SuperBowConstants.QueenBowAttack) > ValueTolerance)
            {
                throw new InvalidOperationException(
                    $"贵族剑 ATK 与已检查数据不一致：预期 {SuperBowConstants.QueenBowAttack}，实际 {nobleAttack}。");
            }

            ListValuePatch attackPatch = null;
            try
            {
                var currentQueenAttack = queenBow.List_AbilityValue[queenAttackIndex];
                if (!ListValuePatch.TryApplyExpectedOrAlreadySet(
                        queenBow.List_AbilityValue,
                        queenAttackIndex,
                        SuperBowConstants.QueenBowOriginalAttack,
                        SuperBowConstants.QueenBowAttack,
                        out attackPatch))
                {
                    throw new InvalidOperationException(
                        $"女王弓 ATK 为 {currentQueenAttack}，不是已检查的原值 2 或目标值 3，" +
                        "拒绝覆盖可能属于其他 Mod 的修改。");
                }

                _bowEnhance = bowEnhance;
                _session = new CatalogPatchSession(attackPatch);
                _logger?.LogInfo(
                    "超级弓箭数据初始化完成：WoodBow ATK=3；特殊候选仅在 WoodBow 重铸期间启用。");
            }
            catch
            {
                attackPatch?.Dispose();
                _bowEnhance = null;
                throw;
            }
        }

        private static void SetQueenBowReforgeContext()
        {
            if (_candidateSession != null)
            {
                return;
            }

            if (_bowEnhance == null)
            {
                var manager = GameMgr.Instance;
                TryApplySafely(manager != null ? manager._DB_Mgr : null);
            }

            if (_bowEnhance == null)
            {
                throw new InvalidOperationException("女王弓重铸数据库尚未初始化。");
            }

            ValidateAligned(
                _bowEnhance.List_Ability1,
                _bowEnhance.List_AbilityValue1,
                "弓重铸1");
            ValidateAligned(
                _bowEnhance.List_Ability2,
                _bowEnhance.List_AbilityValue2,
                "弓重铸2");

            PairedListAppendPatch<Res_Ability> rangePatch = null;
            PairedListAppendPatch<Res_Ability> bleedPatch = null;
            try
            {
                rangePatch = EnsureCandidate(
                    _bowEnhance.List_Ability1,
                    _bowEnhance.List_AbilityValue1,
                    (Res_Ability)SuperBowConstants.RangeAttackAbilityId,
                    SuperBowConstants.RangeAttackValue,
                    "重铸1范围攻击");
                bleedPatch = EnsureCandidate(
                    _bowEnhance.List_Ability2,
                    _bowEnhance.List_AbilityValue2,
                    (Res_Ability)SuperBowConstants.BloodDrainAbilityId,
                    SuperBowConstants.BleedMarkerValue,
                    "重铸2流血");

                _candidateSession = new CandidatePatchSession(rangePatch, bleedPatch);
                _logger?.LogDebug("已为 WoodBow 临时启用范围攻击和流血重铸候选。");
            }
            catch
            {
                bleedPatch?.Dispose();
                rangePatch?.Dispose();
                throw;
            }
        }

        private static ItemInfo RequireSingleWeapon(
            IEnumerable<ItemInfo> weapons,
            int index,
            int type,
            string name)
        {
            return weapons.Single(item =>
                item != null &&
                item.Index == index &&
                item.m_Type == type &&
                string.Equals(item.Name, name, StringComparison.Ordinal));
        }

        private static void ValidateAligned<TAbility>(
            IList<TAbility> abilities,
            IList<float> values,
            string name)
        {
            if (abilities == null || values == null || abilities.Count != values.Count)
            {
                throw new InvalidOperationException($"{name}的能力和值列表不一致。");
            }
        }

        private static int FindSingleAbilityIndex(
            IList<Res_Ability> abilities,
            Res_Ability ability,
            string name)
        {
            var matches = Enumerable.Range(0, abilities.Count)
                .Where(index => abilities[index].Equals(ability))
                .ToArray();
            if (matches.Length != 1)
            {
                throw new InvalidOperationException($"{name}必须且只能出现一次。");
            }

            return matches[0];
        }

        private static PairedListAppendPatch<Res_Ability> EnsureCandidate(
            IList<Res_Ability> abilities,
            IList<float> values,
            Res_Ability ability,
            float value,
            string name)
        {
            for (var index = 0; index < abilities.Count; index++)
            {
                if (!abilities[index].Equals(ability))
                {
                    continue;
                }

                if (Math.Abs(values[index] - value) <= ValueTolerance)
                {
                    return null;
                }

                throw new InvalidOperationException(
                    $"{name}与已有同名能力的数值冲突：{values[index]}。");
            }

            if (!PairedListAppendPatch<Res_Ability>.TryApply(
                    abilities,
                    values,
                    ability,
                    value,
                    out var patch))
            {
                throw new InvalidOperationException($"无法安全追加{name}。");
            }

            return patch;
        }

        private static void DisposeSession()
        {
            _session?.Dispose();
            _session = null;
        }

        private sealed class CatalogPatchSession : IDisposable
        {
            private readonly IDisposable _attack;
            private bool _disposed;

            public CatalogPatchSession(IDisposable attack)
            {
                _attack = attack;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _attack?.Dispose();
            }
        }

        private sealed class CandidatePatchSession : IDisposable
        {
            private readonly IDisposable _range;
            private readonly IDisposable _bleed;
            private bool _disposed;

            public CandidatePatchSession(IDisposable range, IDisposable bleed)
            {
                _range = range;
                _bleed = bleed;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _bleed?.Dispose();
                _range?.Dispose();
            }
        }
    }
}
